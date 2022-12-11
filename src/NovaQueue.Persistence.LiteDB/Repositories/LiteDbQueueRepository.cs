using LiteDB;
using NovaQueue.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaQueue.Persistence.LiteDB
{
	internal class LiteDbQueueRepository<TPayload> : IQueueRepository<TPayload>
	{
		private readonly LiteDBContext<TPayload> context;
		private readonly ILiteCollection<QueueEntry<TPayload>> collection;
		private object dequeueLock = new object();
		public LiteDbQueueRepository(IDatabaseContext<TPayload> context)
		{
			this.context = context as LiteDBContext<TPayload>;
			collection = this.context.Collection;
		}

		#region Sync Methods
		public QueueEntry<TPayload> Get(string id)
			=> collection.FindById(id);
		public IEnumerable<QueueEntry<TPayload>> GetAll()
			=> collection.FindAll().OrderBy(c => c.Sort);
		public IEnumerable<QueueEntry<TPayload>> GetCheckedOutEntries()
			=> collection.Find(c => c.IsCheckedOut).OrderBy(c => c.Sort);
		public IEnumerable<QueueEntry<TPayload>> GetNotCheckedOutEntries()
			=> collection.Find(c => c.IsCheckedOut == false).OrderBy(c => c.Sort);
		public void Insert(QueueEntry<TPayload> entry)
		{
			context.BeginTransaction();
			var maxSort = 0;
			if (collection.Count() > 0)
				maxSort = collection.Max(c => c.Sort);
			entry.Sort = maxSort + 1;
			collection.Insert(entry);
			context.CommitTransaction();
		}

		public void InsertBulk(IEnumerable<QueueEntry<TPayload>> entries)
		{
			context.BeginTransaction();
			var maxSort = collection.Max(c => c.Sort);
			foreach (var entry in entries)
			{
				maxSort++;
				entry.Sort = maxSort;
				collection.Insert(entry);
			}
			context.CommitTransaction();
		}
		public void Update(QueueEntry<TPayload> entry)
			=> collection.Update(entry);
		public void Delete(string id) => collection.Delete(id);
		public void Clear() => collection.DeleteAll();
		public int Count() => collection.Count();
		public int CountNotCheckedOut()
			=> collection.Count(c => c.IsCheckedOut == false);
		public bool HaveEntries() => CountNotCheckedOut() > 0;
		public IEnumerable<QueueEntry<TPayload>> CheckoutEntries(int maxEntries)
		{
			lock (dequeueLock)
			{
				context.Database.BeginTrans();
				var result = GetNotCheckedOutEntries().Take(maxEntries).ToList();
				if (result is null || result.Count() == 0)
					return new List<QueueEntry<TPayload>>();

				foreach (var item in result)
				{
					item.IsCheckedOut = true;
					Update(item);
				}
				context.Database.Commit();
				return result;
			}
		}
		public bool MoveUp(QueueEntry<TPayload> entry)
		{
			lock (dequeueLock)
			{
				context.Database.BeginTrans();
				var entries = collection
					.Find(c => c.Sort < entry.Sort)
					.OrderByDescending(c => c.Sort);

				if (entries == null || entries.Count() == 0)
					return false;

				var entryLow = entries.First();
				var sortLow = entryLow.Sort;
				entryLow.Sort = entry.Sort;
				entry.Sort = sortLow;

				Update(entry);
				Update(entryLow);

				context.Database.Commit();
				return true;
			}
		}

		public bool MoveToEnd(QueueEntry<TPayload> entry)
		{
			lock (dequeueLock)
			{
				context.Database.BeginTrans();
				var maxSort = collection.Max<int>(c => c.Sort);
				if (maxSort == 0 || maxSort == entry.Sort)
					return false;

				entry.Sort = maxSort + 1;
				Update(entry);
				context.Database.Commit();
				return true;
			}
		}
		#endregion


		#region Async Methods
		public Task<QueueEntry<TPayload>> GetAsync(string id)
	=> Task.Run(() => Get(id));
		public Task<IEnumerable<QueueEntry<TPayload>>> GetAllAsync()
			=> Task.Run(() => GetAll());
		public Task<IEnumerable<QueueEntry<TPayload>>> GetCheckedOutEntriesAsync()
			=> Task.Run(() => GetCheckedOutEntries());
		public Task<IEnumerable<QueueEntry<TPayload>>> GetNotCheckedOutEntriesAsync()
			=> Task.Run(() => GetNotCheckedOutEntries());
		public Task<IEnumerable<QueueEntry<TPayload>>> CheckoutEntriesAsync(int maxEntries)
			=> Task.Run(() => CheckoutEntries(maxEntries));
		public Task InsertAsync(QueueEntry<TPayload> entry)
			=> Task.Run(() => Insert(entry));
		public Task InsertBulkAsync(IEnumerable<QueueEntry<TPayload>> entries)
			=> Task.Run(() => InsertBulk(entries));
		public Task UpdateAsync(QueueEntry<TPayload> entry)
			=> Task.Run(() => Update(entry));
		public Task DeleteAsync(string id)
			=> Task.Run(() => Delete(id));
		public Task ClearAsync()
			=> Task.Run(() => Clear());
		public Task<int> CountAsync()
			=> Task.Run(() => Count());
		public Task<int> CountNotCheckedOutAsync()
			=> Task.Run(() => CountNotCheckedOut());
		public Task<bool> HaveEntriesAsync()
			=> Task.Run(() => HaveEntries());
		public Task<bool> MoveUpAsync(QueueEntry<TPayload> entry)
			=> Task.Run(() => MoveUp(entry));
		public Task<bool> MoveToEndAsync(QueueEntry<TPayload> entry)
			=> Task.Run(() => MoveToEnd(entry));

		#endregion
	}
}
