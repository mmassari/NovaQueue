using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapster;
using System.Diagnostics;
using NovaQueue.Abstractions;

namespace NovaQueue.Core
{
    public class NQueueSimple<TPayload> : IQueue<TPayload>
	{
		private readonly Abstractions.IDatabaseContext context;
		private readonly IQueueRepository<TPayload> repository;

		#region Constructors

		/// <summary>
		/// Creates a collection for you in the database
		/// </summary>
		/// <param name="db">The LiteDB database. You are responsible for its lifecycle (using/dispose)</param>
		/// <param name="collectionName">Name of the collection to create</param>
		/// <param name="transactional">Whether the queue should use transaction logic, default true</param>
		public NQueueSimple(IDatabaseContext<TPayload> context, IQueueRepository<TPayload> repository, string name)
		{
			this.context = context;
			this.repository = repository;
			repository.Initialize(name);
		}

		#endregion

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

			repository.Insert(new QueueEntry<TPayload>(item)
				.Adapt<QueueEntry<TPayload>>());
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

			repository.InsertBulk(inserts.Adapt<List<QueueEntry<TPayload>>>());
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
		public List<QueueEntry<TPayload>> Dequeue(int batchSize)
		{
			var items = repository.GetEntries(batchSize).ToList();
			repository.Delete(items);
			return items
				.Adapt<IEnumerable<QueueEntry<TPayload>>>()
				.ToList();
		}


		/// <summary>
		/// Number of items in queue, including those that have been checked out.
		/// </summary>
		public int Count()
		{
			return repository.Count();
		}

		/// <summary>
		/// Removes all items from queue, including any that have been checked out.
		/// </summary>
		public void Clear()
		{
			repository.Clear();
		}

		public void Delete(string id)
		{
			throw new NotImplementedException();
		}

		public void DeleteCompleted(string id)
		{
			throw new NotImplementedException();
		}

		public void DeleteDeadLetter(string id)
		{
			throw new NotImplementedException();
		}

		public void ClearCompleted()
		{
			throw new NotImplementedException();
		}

		public void ClearDeadLetter()
		{
			throw new NotImplementedException();
		}

		public QueueEntry<TPayload> Get(string id)
		{
			throw new NotImplementedException();
		}

		public Task DeleteAsync(string id)
		{
			throw new NotImplementedException();
		}

		public Task DeleteCompletedAsync(string id)
		{
			throw new NotImplementedException();
		}

		public Task DeleteDeadLetterAsync(string id)
		{
			throw new NotImplementedException();
		}

		public Task ClearAsync()
		{
			throw new NotImplementedException();
		}

		public Task ClearCompletedAsync()
		{
			throw new NotImplementedException();
		}

		public Task ClearDeadLetterAsync()
		{
			throw new NotImplementedException();
		}

		public Task<int> CountAsync()
		{
			throw new NotImplementedException();
		}

		public Task<QueueEntry<TPayload>> GetAsync(string id)
		{
			throw new NotImplementedException();
		}

		public Task<QueueEntry<TPayload>> DequeueAsync()
		{
			throw new NotImplementedException();
		}

		public Task<List<QueueEntry<TPayload>>> DequeueAsync(int batchSize)
		{
			throw new NotImplementedException();
		}

		public Task EnqueueAsync(IEnumerable<TPayload> items)
		{
			throw new NotImplementedException();
		}

		public Task EnqueueAsync(TPayload item)
		{
			throw new NotImplementedException();
		}
	}
}
