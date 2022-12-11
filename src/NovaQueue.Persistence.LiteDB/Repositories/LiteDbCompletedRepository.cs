using LiteDB;
using NovaQueue.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NovaQueue.Persistence.LiteDB
{
	public class LiteDbCompletedRepository<TPayload> : ICompletedRepository<TPayload>
	{
		private readonly ILiteCollection<CompletedEntry<TPayload>> collection;
		public LiteDbCompletedRepository(IDatabaseContext<TPayload> context)
		{
			var liteContext = context as LiteDBContext<TPayload>;
			collection = liteContext.CompletedCollection;
		}
		public CompletedEntry<TPayload> Get(string id) => collection.FindById(id);
		public IEnumerable<CompletedEntry<TPayload>> GetAll() => collection.FindAll();
		public void Insert(CompletedEntry<TPayload> entry)=>collection.Insert(entry);
		public void Update(CompletedEntry<TPayload> entry)=>collection.Update(entry);
		public void Delete(string id) => collection.Delete(id);
		public void Clear() => collection.DeleteAll();

		public Task<IEnumerable<CompletedEntry<TPayload>>> GetAllAsync() => Task.Run(() => GetAll());
		public Task<CompletedEntry<TPayload>> GetAsync(string id) => Task.Run(() => Get(id));
		public Task InsertAsync(CompletedEntry<TPayload> entry) => Task.Run(() => Insert(entry));
		public Task UpdateAsync(CompletedEntry<TPayload> entry)=> Task.Run(() => Update(entry));
		public Task DeleteAsync(string id) => Task.Run(() => Delete(id));
		public Task ClearAsync() => Task.Run(() => Clear());
	}
}
