using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NovaQueue.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaQueue.Persistence.EFCore.SqlServer
{
	internal class SqlServerQueueRepository<TPayload> : SqlServerRepositoryBase<TPayload>, IQueueRepository<TPayload>
		where TPayload : class
	{
		private readonly QueueOptions<TPayload> options;

		public SqlServerQueueRepository(IDatabaseContext<TPayload> context, IOptions<QueueOptions<TPayload>> options) : base(context)
		{
			tableName = context.CollectionName;
			this.options = options.Value;
		}
		public QueueEntry<TPayload> Get(string id) => GetAsync(id).Result;
		public async Task<QueueEntry<TPayload>> GetAsync(string id)
		{
			using var ctx = await context.CreateDbContextAsync();
			return await ctx.QueueEntries.FindAsync(id);
		}


		public IEnumerable<QueueEntry<TPayload>> GetAll() => GetAllAsync().Result;
		public async Task<IEnumerable<QueueEntry<TPayload>>> GetAllAsync()
		{
			using var ctx = await context.CreateDbContextAsync();
			return await ctx.QueueEntries.ToListAsync();
		}
		public int Count() => CountAsync().Result;
		public async Task<int> CountAsync()
		{
			using var ctx = await context.CreateDbContextAsync();
			return await ctx.QueueEntries.CountAsync();
		}
		public void Update(QueueEntry<TPayload> entry) =>
			Task.WaitAll(UpdateAsync(entry));
		public async Task UpdateAsync(QueueEntry<TPayload> entry)
		{
			using var ctx = await context.CreateDbContextAsync();
			ctx.Entry(entry).State = EntityState.Modified;
			await ctx.SaveChangesAsync();
		}
		public void Delete(string id) => Task.WaitAll(DeleteAsync(id));
		public async Task DeleteAsync(string id)
		{
			using var ctx = await context.CreateDbContextAsync();
			var entity = ctx.QueueEntries.FindAsync(id);
			if (entity != null)
				ctx.Entry(entity).State = EntityState.Deleted;
			await ctx.SaveChangesAsync();
		}
		public void Clear() => Task.WaitAll(ClearAsync());
		public async Task ClearAsync()
		{
			using var ctx = await context.CreateDbContextAsync();
			try
			{
				ctx.BeginTransaction();

				await ctx.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE {tableName}");

				ctx.CommitTransaction();

			}
			catch (Exception)
			{
				ctx.RollbackTransaction();
				throw;
			}
		}
		public IEnumerable<QueueEntry<TPayload>> CheckoutEntries(int maxEntries)
			=> CheckoutEntriesAsync(maxEntries).Result;
		public async Task<IEnumerable<QueueEntry<TPayload>>> CheckoutEntriesAsync(int maxEntries)
		{
			var DbF = EF.Functions;
			using var ctx = await context.CreateDbContextAsync();
			var result = await ctx.QueueEntries
				.Where(c => c.IsCheckedOut == false)
				.Where(c => c.LastAttempt==null || DbF.DateDiffSecond(c.LastAttempt.Value,DateTime.Now) >= options.WaitOnRetry.TotalSeconds)
				.OrderBy(c=>c.Sort)
				.Take(maxEntries)
				.ToListAsync();

			if (result is null || result.Count() == 0)
				return new List<QueueEntry<TPayload>>();

			try
			{
				ctx.BeginTransaction();
				foreach (var item in result)
				{
					item.IsCheckedOut = true;
					ctx.Entry(item).State = EntityState.Modified;
				}

				await ctx.SaveChangesAsync();

				ctx.CommitTransaction();
				return result;
			}
			catch (Exception)
			{
				context.RollbackTransaction();
				throw;
			}
		}

		public int CountNotCheckedOut() => CountNotCheckedOutAsync().Result;
		public async Task<int> CountNotCheckedOutAsync()
		{
			using var ctx = await context.CreateDbContextAsync();
			return await ctx.QueueEntries.CountAsync(c => c.IsCheckedOut == false);
		}


		public IEnumerable<QueueEntry<TPayload>> GetCheckedOutEntries() =>
			GetCheckedOutEntriesAsync().Result;
		public async Task<IEnumerable<QueueEntry<TPayload>>> GetCheckedOutEntriesAsync()
		{
			using var ctx = await context.CreateDbContextAsync();
			return await ctx.QueueEntries.Where(c => c.IsCheckedOut).ToListAsync();
		}

		public IEnumerable<QueueEntry<TPayload>> GetNotCheckedOutEntries() => GetNotCheckedOutEntriesAsync().Result;
		public async Task<IEnumerable<QueueEntry<TPayload>>> GetNotCheckedOutEntriesAsync()
		{
			using var ctx = await context.CreateDbContextAsync();
			return await ctx.QueueEntries.Where(c => !c.IsCheckedOut).ToListAsync();
		}

		public bool HaveEntries() => HaveEntriesAsync().Result;
		public async Task<bool> HaveEntriesAsync()
		{
			using var ctx = await context.CreateDbContextAsync();
			return await ctx.QueueEntries.AnyAsync();
		}
		public void Insert(QueueEntry<TPayload> entry) => Task.WaitAll(InsertAsync(entry));
		public async Task InsertAsync(QueueEntry<TPayload> entry)
		{
			using var ctx = await context.CreateDbContextAsync();
			try
			{
				await ctx.QueueEntries.AddAsync(entry);
				await ctx.SaveChangesAsync();
			}
			catch (Exception)
			{
				throw;
			}
		}

		public void InsertBulk(IEnumerable<QueueEntry<TPayload>> entries) =>
			Task.WaitAll(InsertBulkAsync(entries));
		public async Task InsertBulkAsync(IEnumerable<QueueEntry<TPayload>> entries)
		{
			using var ctx = await context.CreateDbContextAsync();
			var transaction = await ctx.Database.BeginTransactionAsync();
			try
			{
				
				foreach (var item in entries)
				{
					await ctx.QueueEntries.AddAsync(item);
				}

				await ctx.SaveChangesAsync();
				await transaction.CommitAsync();
			}
			catch (Exception)
			{
				await transaction.RollbackAsync();
				throw;
			}
		}

		public bool MoveToEnd(QueueEntry<TPayload> entry) =>
			MoveToEndAsync(entry).Result;
		public async Task<bool> MoveToEndAsync(QueueEntry<TPayload> entry)
		{
			using var ctx = await context.CreateDbContextAsync();
			var transaction = await ctx.Database.BeginTransactionAsync();
			try
			{
				var max = await ctx.QueueEntries.MaxAsync(c => c.Sort);
				if (entry.Sort == max)
					return false;

				entry.Sort = max++;
				ctx.Entry(entry).State = EntityState.Modified;
				await ctx.SaveChangesAsync();

				await transaction.CommitAsync();
				return true;
			}
			catch (Exception)
			{
				await transaction.RollbackAsync();
				throw;
			}
		}

		public bool MoveUp(QueueEntry<TPayload> entry) =>
			MoveUpAsync(entry).Result;
		public async Task<bool> MoveUpAsync(QueueEntry<TPayload> entry)
		{
			using var ctx = await context.CreateDbContextAsync();
			var transaction = await ctx.Database.BeginTransactionAsync(); 
			try
			{
				var prevEntry = await ctx.QueueEntries
					.OrderByDescending(c => c.Sort)
					.FirstOrDefaultAsync(c => c.Sort < entry.Sort);

				if (prevEntry == null)
					return false;
				var s = entry.Sort;
				entry.Sort = prevEntry.Sort;
				prevEntry.Sort = s;
				ctx.Entry(entry).State = EntityState.Modified;
				ctx.Entry(prevEntry).State = EntityState.Modified;
				await ctx.SaveChangesAsync();
				await transaction.CommitAsync();
				return true;
			}
			catch (Exception)
			{
				await transaction.RollbackAsync();
				throw;
			}
		}

		public int GetMaxSortId() => GetMaxSortIdAsync().Result;
		public async Task<int> GetMaxSortIdAsync()
		{
			using var ctx = await context.CreateDbContextAsync();
			try
			{
				return await ctx.QueueEntries.MaxAsync(c => c.Sort);
			}
			catch (Exception)
			{
				return 0;
			}
		}
	}
}
