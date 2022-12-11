using LiteDB;
using NovaQueue.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NovaQueue.Persistence.LiteDB.Repositories
{
	internal class LiteDbDeadLetterRepository<TPayload> : IDeadLetterRepository<TPayload>
	{
		private readonly ILiteCollection<DeadLetterEntry<TPayload>> collection;
		public LiteDbDeadLetterRepository(IDatabaseContext<TPayload> context)
		{
			var liteContext = context as LiteDBContext<TPayload>;
			collection = liteContext.DeadLetterCollection;
		}
		public DeadLetterEntry<TPayload> Get(string id) => collection.FindById(id);
		public IEnumerable<DeadLetterEntry<TPayload>> GetAll() => collection.FindAll();
		public void Insert(DeadLetterEntry<TPayload> entry) => collection.Insert(entry);
		public void Update(DeadLetterEntry<TPayload> entry) => collection.Update(entry);
		public void Delete(string id) => collection.Delete(id);
		public void Clear() => collection.DeleteAll();

		public Task<DeadLetterEntry<TPayload>> GetAsync(string id) => Task.Run(() => Get(id));
		public Task<IEnumerable<DeadLetterEntry<TPayload>>> GetAllAsync() => Task.Run(() => GetAll());
		public Task InsertAsync(DeadLetterEntry<TPayload> entry) => Task.Run(() => Insert(entry));
		public Task UpdateAsync(DeadLetterEntry<TPayload> entry) => Task.Run(() => Update(entry));
		public Task DeleteAsync(string id) => Task.Run(() => Delete(id));
		public Task ClearAsync() => Task.Run(() => Clear());
	}
}
