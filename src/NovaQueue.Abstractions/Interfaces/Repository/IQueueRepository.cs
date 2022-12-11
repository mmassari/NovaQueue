using System.Collections.Generic;
using System.Threading.Tasks;

namespace NovaQueue.Abstractions
{
	public interface IQueueRepository<TPayload>
	{
		QueueEntry<TPayload> Get(string id);
		IEnumerable<QueueEntry<TPayload>> GetAll();
		IEnumerable<QueueEntry<TPayload>> GetCheckedOutEntries();
		IEnumerable<QueueEntry<TPayload>> GetNotCheckedOutEntries();
		IEnumerable<QueueEntry<TPayload>> CheckoutEntries(int maxEntries);
		void Insert(QueueEntry<TPayload> entry);
		void InsertBulk(IEnumerable<QueueEntry<TPayload>> entries);
		void Update(QueueEntry<TPayload> entry);
		void Delete(string id);
		void Clear();
		int Count();
		int CountNotCheckedOut();
		bool MoveUp(QueueEntry<TPayload> entry);
		bool MoveToEnd(QueueEntry<TPayload> entry);
		bool HaveEntries();

		//Async methods
		Task<QueueEntry<TPayload>> GetAsync(string id);
		Task<IEnumerable<QueueEntry<TPayload>>> GetAllAsync();
		Task<IEnumerable<QueueEntry<TPayload>>> GetCheckedOutEntriesAsync();
		Task<IEnumerable<QueueEntry<TPayload>>> GetNotCheckedOutEntriesAsync();
		Task<IEnumerable<QueueEntry<TPayload>>> CheckoutEntriesAsync(int maxEntries);
		Task InsertAsync(QueueEntry<TPayload> entry);
		Task InsertBulkAsync(IEnumerable<QueueEntry<TPayload>> entry);
		Task UpdateAsync(QueueEntry<TPayload> entry);
		Task<bool> MoveUpAsync(QueueEntry<TPayload> entry);
		Task<bool> MoveToEndAsync(QueueEntry<TPayload> entry);
		Task<bool> HaveEntriesAsync();
		Task DeleteAsync(string id);
		Task ClearAsync();
		Task<int> CountAsync();
	}
}