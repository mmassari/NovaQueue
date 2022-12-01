using System.Collections.Generic;

namespace NovaQueue.Abstractions
{
	public interface ITransactionalQueue<T> : IQueue<T>
	{
		NovaQueueOptions<T> Options { get; }
		bool IsDeadLetterEnabled { get; }
		void Abort(IEnumerable<QueueEntry<T>> items, string error = null);
		void Abort(QueueEntry<T> item, string error = null);
		void Commit(IEnumerable<QueueEntry<T>> items);
		void Commit(QueueEntry<T> item);
		List<QueueEntry<T>> CurrentCheckouts();
		IEnumerable<DeadLetterEntry<T>> DeadLetterEntries();
		IEnumerable<CompletedEntry<T>> CompletedEntries();
		IEnumerable<QueueEntry<T>> QueuedEntries();
		void ResetOrphans();
	}
}