using System.Collections.Generic;
using System.Threading.Tasks;

namespace NovaQueue.Abstractions
{
	public interface ICompletedRepository<TPayload>
	{
		CompletedEntry<TPayload> Get(string id);
		IEnumerable<CompletedEntry<TPayload>> GetAll();
		void Insert(CompletedEntry<TPayload> entry);
		void Update(CompletedEntry<TPayload> entry);
		void Delete(string id);
		void Clear();
		Task<CompletedEntry<TPayload>> GetAsync(string id);
		Task<IEnumerable<CompletedEntry<TPayload>>> GetAllAsync();
		Task InsertAsync(CompletedEntry<TPayload> entry);
		Task UpdateAsync(CompletedEntry<TPayload> entry);
		Task DeleteAsync(string id);
		Task ClearAsync();

	}
}