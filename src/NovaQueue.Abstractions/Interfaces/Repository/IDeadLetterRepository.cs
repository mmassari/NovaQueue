using System.Collections.Generic;
using System.Threading.Tasks;

namespace NovaQueue.Abstractions
{
	public interface IDeadLetterRepository<TPayload>
	{
		DeadLetterEntry<TPayload> Get(string id);
		IEnumerable<DeadLetterEntry<TPayload>> GetAll();
		void Insert(DeadLetterEntry<TPayload> entry);
		void Update(DeadLetterEntry<TPayload> entry);
		void Delete(string id);
		void Clear();
		Task<DeadLetterEntry<TPayload>> GetAsync(string id);
		Task<IEnumerable<DeadLetterEntry<TPayload>>> GetAllAsync();
		Task InsertAsync(DeadLetterEntry<TPayload> entry);
		Task UpdateAsync(DeadLetterEntry<TPayload> entry);
		Task DeleteAsync(string id);
		Task ClearAsync();

	}
}