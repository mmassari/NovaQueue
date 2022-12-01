using System.Collections.Generic;

namespace NovaQueue.Abstractions
{
	public interface IQueue<T>
	{
		void Clear();
		int Count();
		QueueEntry<T> Dequeue();
		List<QueueEntry<T>> Dequeue(int batchSize);
		void Enqueue(IEnumerable<T> items);
		void Enqueue(T item);
	}
}