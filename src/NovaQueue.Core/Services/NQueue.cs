using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapster;
using System.Diagnostics;
using NovaQueue.Abstractions;
using Microsoft.Extensions.Options;
using Quartz.Impl;
using Quartz;

namespace NovaQueue.Core
{
	/// <summary>
	/// Uses LiteDB to provide a persisted, thread safe, (optionally) transactional, FIFO queue.
	/// 
	/// Suitable for use on clients as a lightweight, portable alternative to MSMQ. Not recommended for use 
	/// on large server side applications due to performance limitations of LiteDB.
	/// </summary>
	public class NQueue<T> : ITransactionalQueue<T>
	{
		public NovaQueueOptions<T> Options { get; private set; }
		readonly object _dequeueLock = new object();
		private readonly IQueueRepository repository;
		public bool IsDeadLetterEnabled => Options.Completed.IsEnabled;

		#region Constructors

		/// <summary>
		/// Creates a collection for you in the database
		/// </summary>
		/// <param name="db">The LiteDB database. You are responsible for its lifecycle (using/dispose)</param>
		/// <param name="collectionName">Name of the collection to create</param>
		/// <param name="transactional">Whether the queue should use transaction logic, default true</param>
		public NQueue(IQueueRepository repository, IOptions<NovaQueueOptions<T>> options)
		{
			this.repository = repository;
			Options = options.Value;

			repository.Initialize(Options.Name, Options.DeadLetter.IsEnabled, Options.Completed.IsEnabled);
			if (Options.ResetOrphansOnStartup)
				ResetOrphans();

			//Se ho abilitato la DeadLetterQueue o la CompletedQueue attivo lo scheduler
			if (Options.DeadLetter.IsEnabled || Options.Completed.IsEnabled)
			{
				StdSchedulerFactory factory = new StdSchedulerFactory();
				IScheduler sched = factory.GetScheduler().Result;


			}
		}

		#endregion

		/// <summary>
		/// Adds a single item to queue. See <see cref="Enqueue(IEnumerable{T})"/> for adding a batch.
		/// </summary>
		/// <param name="item"></param>
		public void Enqueue(T item)
		{
			if (item == null)
			{
				throw new ArgumentNullException(nameof(item));
			}

			repository.Insert(
				new QueueEntry<T>(item)
				.Adapt<QueueEntry<object>>());
		}

		/// <summary>
		/// Adds a batch of items to the queue. See <see cref="Enqueue(T)"/> for adding a single item.
		/// </summary>
		/// <param name="items"></param>
		public void Enqueue(IEnumerable<T> items)
		{
			List<QueueEntry<T>> inserts = new List<QueueEntry<T>>();
			foreach (var item in items)
			{
				inserts.Add(new QueueEntry<T>(item));
			}

			repository.InsertBulk(inserts.Adapt<List<QueueEntry<object>>>());
		}

		/// <summary>
		/// Transactional queues:
		///     Marks item as checked out but does not remove from queue. You are expected to later call <see cref="Commit(QueueEntry{T})"/> or <see cref="Abort(QueueEntry{T})"/>
		/// Non-transactional queues:
		///     Removes item from queue with no need to call <see cref="Commit(QueueEntry{T})"/> or <see cref="Abort(QueueEntry{T})"/>
		/// </summary>
		/// <returns>An item if found or null</returns>
		public QueueEntry<T> Dequeue()
		{
			var result = Dequeue(1);
			if (result.Count == 0)
			{
				return null;
			}
			else
			{
				return result[0];
			}
		}

		/// <summary>
		/// Batch equivalent of <see cref="Dequeue"/>
		/// </summary>
		/// <param name="batchSize">The maximum number of items to dequeue</param>
		/// <returns>The items found or an empty collection (never null)</returns>
		public List<QueueEntry<T>> Dequeue(int batchSize)
		{
			var entries = repository.CheckoutEntries(batchSize);
			var ret = new List<QueueEntry<T>>();    // entries.Adapt<IEnumerable<QueueEntry<T>>>();
			foreach (var entry in entries)
			{
				var e = entry.Adapt<QueueEntry<T>>();
				e.Payload = (T)entry.Payload;
				ret.Add(e);
			}
			return ret.ToList();
		}

		/// <summary>
		/// Obtains list of items currently checked out (but not yet commited or aborted) as a result of Dequeue calls on a transactional queue
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown when queue is not transactional</exception>
		/// <returns>Items found or empty collection (never null)</returns>
		public List<QueueEntry<T>> CurrentCheckouts()
		{
			return repository
				.GetCheckoutEntries()
				.Adapt<IEnumerable<QueueEntry<T>>>()
				.ToList();
		}

		/// <summary>
		/// Aborts all currently checked out items. Equivalent of calling <see cref="CurrentCheckouts"/> followed by <see cref="Abort(IEnumerable{QueueEntry{T}})"/>
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown when queue is not transactional</exception>
		public void ResetOrphans()
		{
			Abort(CurrentCheckouts(), "ResetOrphans");
		}

		private bool IsMaxAttemptsReached(QueueEntry<object> item) =>
			Options.MaxAttempts > 0 && item.Attempts >= Options.MaxAttempts;

		/// <summary>
		/// Aborts a transaction on a single item. See <see cref="Abort(IEnumerable{QueueEntry{T}})"/> for batches.
		/// </summary>
		/// <param name="item">An item that was obtained from a <see cref="Dequeue"/> call</param>
		/// <exception cref="InvalidOperationException">Thrown when queue is not transactional</exception>
		public void Abort(QueueEntry<T> entry, string error = null)
		{
			var lockAbort = new object();
			lock (lockAbort)
			{
				if (entry == null)
				{
					throw new ArgumentNullException(nameof(entry));
				}
				var item = entry.Adapt<QueueEntry<object>>();

				item.IsCheckedOut = false;
				item.Attempts++;
				item.LastAttempt = DateTime.UtcNow;

				if (!string.IsNullOrEmpty(error))
					item.Errors.Add(error);

				if (IsMaxAttemptsReached(item))
				{
					if (IsDeadLetterEnabled)
					{
						repository.MoveToDeadLetter(item);
					}
					else
					{
						repository.Delete(item.Id);
					}
				}
				else
				{
					if (Options.OnFailure == OnFailurePolicy.Retry)
					{
						repository.Update(item);
					}
					else
					{
						repository.MoveToLastPosition(item);
					}
				}
			}
		}


		/// <summary>
		/// Aborts a transation on a group of items. See <see cref="Abort(QueueEntry{T})"/> for a single item.
		/// </summary>
		/// <param name="items">Items that were obtained from a <see cref="Dequeue"/> call</param>
		/// <exception cref="InvalidOperationException">Thrown when queue is not transactional</exception>
		public void Abort(IEnumerable<QueueEntry<T>> items, string error = null)
		{
			var lockAbort = new object();
			lock (lockAbort)
			{
				foreach (var item in items)
				{
					Abort(item, error);
				}
			}
		}

		/// <summary>
		/// Commits a transaction on a single item. See <see cref="Commit(IEnumerable{QueueEntry{T}})"/> for batches.
		/// </summary>
		/// <param name="item">An item that was obtained from a <see cref="Dequeue"/> call</param>
		/// <exception cref="InvalidOperationException">Thrown when queue is not transactional</exception>
		public void Commit(QueueEntry<T> item)
		{
			var lockCommit = new object();
			lock (lockCommit)
			{
				if (item == null)
				{
					throw new ArgumentNullException(nameof(item));
				}

				if (Options.Completed.IsEnabled)
					repository.MoveToCompleted(item.Adapt<QueueEntry<object>>());
				else
					repository.Delete(item.Id);
			}
		}

		/// <summary>
		/// Commits a transation on a group of items. See <see cref="Commit(QueueEntry{T})/> for a single item.
		/// </summary>
		/// <param name="items">Items that were obtained from a <see cref="Dequeue"/> call</param>
		/// <exception cref="InvalidOperationException">Thrown when queue is not transactional</exception>
		public void Commit(IEnumerable<QueueEntry<T>> items)
		{
			var lockCommit = new object();
			lock (lockCommit)
			{
				foreach (var item in items)
				{
					Commit(item);
				}
			}
		}

		/// <summary>
		/// Number of items in queue, including those that have been checked out.
		/// </summary>
		public int Count()
		{
			return repository.Count(CollectionType.Queue);
		}
		/// <summary>
		/// Number of items in queue, including those that have been checked out.
		/// </summary>
		public int CountToCheckout()
		{
			return repository.CountNew();
		}
		/// <summary>
		/// Removes all items from queue, including any that have been checked out.
		/// </summary>
		public void Clear()
		{
			repository.Clear(
				CollectionType.Queue |
				CollectionType.DeadLetter |
				CollectionType.Completed);
		}

		public IEnumerable<DeadLetterEntry<T>> DeadLetterEntries() =>
			repository.GetDeadLetterEntries().Adapt<IEnumerable<DeadLetterEntry<T>>>();		

		public IEnumerable<CompletedEntry<T>> CompletedEntries() =>
			repository.GetCompletedEntries().Adapt<IEnumerable<CompletedEntry<T>>>();
		public IEnumerable<QueueEntry<T>> QueuedEntries() =>
			repository.GetEntries(100).Adapt<IEnumerable<QueueEntry<T>>>();
	}
}
