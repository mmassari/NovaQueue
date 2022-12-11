using System.Collections.Generic;
using System.Threading.Tasks;

namespace NovaQueue.Abstractions
{
	public interface IQueue<TPayload>
	{
		void Delete(string id);
		void DeleteCompleted(string id);
		void DeleteDeadLetter(string id);
		void Clear();
		void ClearCompleted();
		void ClearDeadLetter();
		int Count();
		QueueEntry<TPayload> Get(string id);
		QueueEntry<TPayload> Dequeue();
		List<QueueEntry<TPayload>> Dequeue(int batchSize);
		void Enqueue(IEnumerable<TPayload> items);
		void Enqueue(TPayload item);


		Task DeleteAsync(string id);
		Task DeleteCompletedAsync(string id);
		Task DeleteDeadLetterAsync(string id);
		Task ClearAsync();
		Task ClearCompletedAsync();
		Task ClearDeadLetterAsync();
		Task<int> CountAsync();
		Task<QueueEntry<TPayload>> GetAsync(string id);
		Task<QueueEntry<TPayload>> DequeueAsync();
		Task<List<QueueEntry<TPayload>>> DequeueAsync(int batchSize);
		Task EnqueueAsync(IEnumerable<TPayload> items);
		Task EnqueueAsync(TPayload item);
	}
}