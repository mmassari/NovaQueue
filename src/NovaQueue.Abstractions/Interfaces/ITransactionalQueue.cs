using NovaQueue.Abstractions.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NovaQueue.Abstractions
{
	public interface ITransactionalQueue<T> : IQueue<T>
	{
		QueueOptions<T> Options { get; }
		bool IsDeadLetterEnabled { get; }
		void Abort(IEnumerable<QueueEntry<T>> items, ErrorType type, params Error[] errors);
		void Abort(QueueEntry<T> item, ErrorType type, params Error[] errors);
		void Commit(IEnumerable<QueueEntry<T>> items);
		void Commit(QueueEntry<T> item);
		List<QueueEntry<T>> CurrentCheckouts();
		IEnumerable<DeadLetterEntry<T>> DeadLetterEntries();
		IEnumerable<CompletedEntry<T>> CompletedEntries();
		IEnumerable<QueueEntry<T>> QueuedEntries();
		void ResetOrphans();
		void MoveUp(QueueEntry<T> item);

		//Async methods
		Task AbortAsync(IEnumerable<QueueEntry<T>> items, ErrorType type, params Error[] errors);
		Task AbortAsync(QueueEntry<T> item, ErrorType type, params Error[] errors);
		Task CommitAsync(IEnumerable<QueueEntry<T>> items);
		Task CommitAsync(QueueEntry<T> item);
		Task<List<QueueEntry<T>>> CurrentCheckoutsAsync();
		Task<IEnumerable<DeadLetterEntry<T>>> DeadLetterEntriesAsync();
		Task<IEnumerable<CompletedEntry<T>>> CompletedEntriesAsync();
		Task<IEnumerable<QueueEntry<T>>> QueuedEntriesAsync();
		Task ResetOrphansAsync();
		Task MoveUpAsync(QueueEntry<T> item);
	}
}