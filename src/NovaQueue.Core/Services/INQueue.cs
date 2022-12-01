using NovaQueue.Abstractions;
using System.Collections.Generic;

namespace NovaQueue.Core
{
	public interface INQueue<T>
	{
		bool IsDeadLetterEnabled { get; }
		NovaQueueOptions<T> Options { get; }

		void Abort(IEnumerable<QueueEntry<T>> items, string error = null);
		void Abort(QueueEntry<T> entry, string error = null);
		void Clear();
		void Commit(IEnumerable<QueueEntry<T>> items);
		void Commit(QueueEntry<T> item);
		int Count();
		int CountToCheckout();
		List<QueueEntry<T>> CurrentCheckouts();
		QueueEntry<T> Dequeue();
		List<QueueEntry<T>> Dequeue(int batchSize);
		void Enqueue(IEnumerable<T> items);
		void Enqueue(T item);
		void ResetOrphans();
	}
}