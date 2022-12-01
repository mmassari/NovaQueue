using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapster;
using System.Diagnostics;
using NovaQueue.Abstractions;

namespace NovaQueue.Core
{
	public class NQueueSimple<T> : IQueue<T>
	{
		private readonly IQueueRepository repository;

		#region Constructors

		/// <summary>
		/// Creates a collection for you in the database
		/// </summary>
		/// <param name="db">The LiteDB database. You are responsible for its lifecycle (using/dispose)</param>
		/// <param name="collectionName">Name of the collection to create</param>
		/// <param name="transactional">Whether the queue should use transaction logic, default true</param>
		public NQueueSimple(IQueueRepository repository, string name)
		{
			this.repository = repository;
			repository.Initialize(name);
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

			repository.Insert(new QueueEntry<T>(item)
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
			var items = repository.GetEntries(batchSize).ToList();
			repository.Delete(items);
			return items
				.Adapt<IEnumerable<QueueEntry<T>>>()
				.ToList();
		}


		/// <summary>
		/// Number of items in queue, including those that have been checked out.
		/// </summary>
		public int Count()
		{
			return repository.Count(CollectionType.Queue);
		}

		/// <summary>
		/// Removes all items from queue, including any that have been checked out.
		/// </summary>
		public void Clear()
		{
			repository.Clear(CollectionType.Queue);
		}
	}
}
