using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapster;
using NovaQueue.Abstractions;
using Microsoft.Extensions.Options;
using Quartz.Impl;
using Quartz;
using NovaQueue.Abstractions.Models;

namespace NovaQueue.Core
{
	/// <summary>
	/// Uses LiteDB to provide a persisted, thread safe, (optionally) transactional, FIFO queue.
	/// 
	/// Suitable for use on clients as a lightweight, portable alternative to MSMQ. Not recommended for use 
	/// on large server side applications due to performance limitations of LiteDB.
	/// </summary>
	public class NQueue<TPayload> : ITransactionalQueue<TPayload>
		where TPayload : class
	{
		public QueueOptions<TPayload> Options { get; private set; }
		readonly object _dequeueLock = new object();
		private readonly IDatabaseContext<TPayload> dbContext;
		private readonly IQueueRepository<TPayload> queueRepo;
		private readonly IDeadLetterRepository<TPayload> deadLetterRepo;
		private readonly ICompletedRepository<TPayload> completedRepo;
		private readonly IUnitOfWork<TPayload> unitOfWork;
		public bool IsDeadLetterEnabled => Options.Completed.IsEnabled;

		#region Constructors

		/// <summary>
		/// Creates a collection for you in the database
		/// </summary>
		/// <param name="db">The LiteDB database. You are responsible for its lifecycle (using/dispose)</param>
		/// <param name="collectionName">Name of the collection to create</param>
		/// <param name="transactional">Whether the queue should use transaction logic, default true</param>
		public NQueue(
			IDatabaseContext<TPayload> context,
			IQueueRepository<TPayload> queueRepository,
			IDeadLetterRepository<TPayload> deadLetterRepository,
			ICompletedRepository<TPayload> completedRepository,
			IOptions<QueueOptions<TPayload>> options,
			IUnitOfWork<TPayload> unitOfWork)
		{
			dbContext = context;
			queueRepo = queueRepository;
			deadLetterRepo = deadLetterRepository;
			completedRepo = completedRepository;
			Options = options.Value;
			this.unitOfWork = unitOfWork;
			//dbContext.Initialize(Options.Name);
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

		#region Synchronous Methods

		public QueueEntry<TPayload> Get(string id) =>
			queueRepo.Get(id);

		/// <summary>
		/// Adds a single item to queue. See <see cref="Enqueue(IEnumerable{TPayload})"/> for adding a batch.
		/// </summary>
		/// <param name="item"></param>
		public void Enqueue(TPayload item)
		{
			if (item == null)
			{
				throw new ArgumentNullException(nameof(item));
			}

			queueRepo.Insert(new QueueEntry<TPayload>(item));
		}

		/// <summary>
		/// Adds a batch of items to the queue. See <see cref="Enqueue(TPayload)"/> for adding a single item.
		/// </summary>
		/// <param name="items"></param>
		public void Enqueue(IEnumerable<TPayload> items)
		{
			List<QueueEntry<TPayload>> inserts = new List<QueueEntry<TPayload>>();
			foreach (var item in items)
			{
				inserts.Add(new QueueEntry<TPayload>(item));
			}

			queueRepo.InsertBulk(inserts.Adapt<List<QueueEntry<TPayload>>>());
		}

		/// <summary>
		/// Transactional queues:
		///     Marks item as checked out but does not remove from queue. You are expected to later call <see cref="Commit(QueueEntry{TPayload})"/> or <see cref="Abort(QueueEntry{TPayload})"/>
		/// Non-transactional queues:
		///     Removes item from queue with no need to call <see cref="Commit(QueueEntry{TPayload})"/> or <see cref="Abort(QueueEntry{TPayload})"/>
		/// </summary>
		/// <returns>An item if found or null</returns>
		public QueueEntry<TPayload> Dequeue()
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
		public List<QueueEntry<TPayload>> Dequeue(int batchSize) =>
			queueRepo.CheckoutEntries(batchSize).ToList();


		/// <summary>
		/// Obtains list of items currently checked out (but not yet commited or aborted) as a result of Dequeue calls on a transactional queue
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown when queue is not transactional</exception>
		/// <returns>Items found or empty collection (never null)</returns>
		public List<QueueEntry<TPayload>> CurrentCheckouts() =>
			queueRepo.GetCheckedOutEntries().ToList();

		/// <summary>
		/// Aborts all currently checked out items. Equivalent of calling <see cref="CurrentCheckouts"/> followed by <see cref="Abort(IEnumerable{QueueEntry{TPayload}})"/>
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown when queue is not transactional</exception>
		public void ResetOrphans()
		{
			var entries = CurrentCheckouts();
			foreach (var entry in entries)
			{
				entry.IsCheckedOut = false;
				queueRepo.Update(entry);
			}
		}

		private bool IsMaxAttemptsReached(QueueEntry<TPayload> item) =>
			Options.MaxAttempts > 0 && item.Attempts >= Options.MaxAttempts;

		/// <summary>
		/// Aborts a transaction on a single item. See <see cref="Abort(IEnumerable{QueueEntry{TPayload}})"/> for batches.
		/// </summary>
		/// <param name="item">An item that was obtained from a <see cref="Dequeue"/> call</param>
		/// <exception cref="InvalidOperationException">Thrown when queue is not transactional</exception>
		public void Abort(QueueEntry<TPayload> entry, ErrorType errorType, params Error[] errors)
		{
			var lockAbort = new object();
			lock (lockAbort)
			{
				if (entry == null)
				{
					throw new ArgumentNullException(nameof(entry));
				}
				entry.Errors.Add(new AttemptErrors { Attempt = entry.Attempts, Type = errorType, Errors = errors });
				entry.IsCheckedOut = false;
//				entry.Attempts++;
				entry.LastAttempt = DateTime.UtcNow;


				if (IsMaxAttemptsReached(entry) || errorType==ErrorType.Validation)
				{
					if (IsDeadLetterEnabled)
						unitOfWork.MoveToDeadLetter(entry);
					else
						queueRepo.Delete(entry.Id);
				}
				else
				{
					
					if (Options.OnFailure != OnFailurePolicy.Retry)
						entry.Sort = queueRepo.GetMaxSortId() + 1;

					queueRepo.Update(entry);
				}
			}
		}


		/// <summary>
		/// Aborts a transation on a group of items. See <see cref="Abort(QueueEntry{TPayload})"/> for a single item.
		/// </summary>
		/// <param name="items">Items that were obtained from a <see cref="Dequeue"/> call</param>
		/// <exception cref="InvalidOperationException">Thrown when queue is not transactional</exception>
		public void Abort(IEnumerable<QueueEntry<TPayload>> items, ErrorType type, params Error[] errors)
		{
			var lockAbort = new object();
			lock (lockAbort)
			{
				foreach (var item in items)
				{
					Abort(item, type, errors);
				}
			}
		}

		/// <summary>
		/// Commits a transaction on a single item. See <see cref="Commit(IEnumerable{QueueEntry{TPayload}})"/> for batches.
		/// </summary>
		/// <param name="item">An item that was obtained from a <see cref="Dequeue"/> call</param>
		/// <exception cref="InvalidOperationException">Thrown when queue is not transactional</exception>
		public void Commit(QueueEntry<TPayload> item)
		{
			var lockCommit = new object();
			lock (lockCommit)
			{
				if (item == null)
				{
					throw new ArgumentNullException(nameof(item));
				}

				if (Options.Completed.IsEnabled)
					unitOfWork.MoveToCompleted(item);
				else
					queueRepo.Delete(item.Id);
			}
		}

		/// <summary>
		/// Commits a transation on a group of items. See <see cref="Commit(QueueEntry{TPayload})/> for a single item.
		/// </summary>
		/// <param name="items">Items that were obtained from a <see cref="Dequeue"/> call</param>
		/// <exception cref="InvalidOperationException">Thrown when queue is not transactional</exception>
		public void Commit(IEnumerable<QueueEntry<TPayload>> items)
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
		public int Count() =>
			queueRepo.Count();

		/// <summary>
		/// Number of items in queue, including those that have been checked out.
		/// </summary>
		public int CountToCheckout() =>
			queueRepo.CountNotCheckedOut();

		/// <summary>
		/// Removes all items from queue, including any that have been checked out.
		/// </summary>
		public void Clear() =>
			queueRepo.Clear();


		public IEnumerable<DeadLetterEntry<TPayload>> DeadLetterEntries() =>
			deadLetterRepo.GetAll();

		public IEnumerable<CompletedEntry<TPayload>> CompletedEntries() =>
			completedRepo.GetAll();
		public IEnumerable<QueueEntry<TPayload>> QueuedEntries() =>
			queueRepo.GetAll();

		public void MoveUp(QueueEntry<TPayload> item) =>
			queueRepo.MoveUp(item);

		#endregion

		#region Asynchronous Methods

		public async Task AbortAsync(IEnumerable<QueueEntry<TPayload>> items, ErrorType type, params Error[] errors) =>
			await Task.Run(() => Abort(items, type, errors));

		public async Task AbortAsync(QueueEntry<TPayload> item, ErrorType type, params Error[] errors) =>
			await Task.Run(() => Abort(item, type, errors));

		public async Task CommitAsync(IEnumerable<QueueEntry<TPayload>> items) =>
			await Task.Run(() => Commit(items));

		public async Task CommitAsync(QueueEntry<TPayload> item) =>
			await Task.Run(() => Commit(item));

		public async Task<List<QueueEntry<TPayload>>> CurrentCheckoutsAsync() =>
			await Task.Run(() => CurrentCheckouts());

		public async Task<IEnumerable<DeadLetterEntry<TPayload>>> DeadLetterEntriesAsync() =>
			await Task.Run(() => DeadLetterEntries());

		public async Task<IEnumerable<CompletedEntry<TPayload>>> CompletedEntriesAsync() =>
			await Task.Run(() => CompletedEntries());

		public async Task<IEnumerable<QueueEntry<TPayload>>> QueuedEntriesAsync() =>
			await Task.Run(() => QueuedEntries());

		public async Task ResetOrphansAsync() =>
			await Task.Run(() => ResetOrphans());

		public async Task MoveUpAsync(QueueEntry<TPayload> item) =>
			await Task.Run(() => MoveUp(item));

		public async void Delete(string id) =>
			await Task.Run(() => Delete(id));

		public async void DeleteCompleted(string id) =>
			await Task.Run(() => DeleteCompleted(id));

		public async void DeleteDeadLetter(string id) =>
			await Task.Run(() => DeleteDeadLetter(id));

		public async void ClearCompleted() =>
			await Task.Run(() => ClearCompleted());

		public async void ClearDeadLetter() =>
			await Task.Run(() => ClearDeadLetter());

		public async Task DeleteAsync(string id) =>
			await Task.Run(() => Delete(id));

		public async Task DeleteCompletedAsync(string id) =>
			await Task.Run(() => DeleteCompleted(id));

		public async Task DeleteDeadLetterAsync(string id) =>
			await Task.Run(() => DeleteDeadLetter(id));

		public async Task ClearAsync() =>
			await Task.Run(() => Clear());

		public async Task ClearCompletedAsync() =>
			await Task.Run(() => ClearCompleted());

		public async Task ClearDeadLetterAsync() =>
			await Task.Run(() => ClearDeadLetter());

		public async Task<int> CountAsync() =>
			await Task.Run(() => Count());

		public async Task<QueueEntry<TPayload>> GetAsync(string id) =>
			await Task.Run(() => Get(id));

		public async Task<QueueEntry<TPayload>> DequeueAsync() =>
			await Task.Run(() => Dequeue());

		public async Task<List<QueueEntry<TPayload>>> DequeueAsync(int batchSize) =>
			await Task.Run(() => Dequeue(batchSize));

		public async Task EnqueueAsync(IEnumerable<TPayload> items) =>
			await Task.Run(() => Enqueue(items));

		public async Task EnqueueAsync(TPayload item) =>
			await Task.Run(() => Enqueue(item));

		#endregion	}
	}
}