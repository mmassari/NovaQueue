using Microsoft.EntityFrameworkCore;
using NovaQueue.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NovaQueue.Persistence.EFCore.SqlServer
{
	public class SqlServerDeadLetterRepository<TPayload> : SqlServerRepositoryBase<TPayload>, IDeadLetterRepository<TPayload>
		where TPayload : class
	{
		public SqlServerDeadLetterRepository(IDatabaseContext<TPayload> context) : base(context)
		{
			tableName = context.DeadLetterCollectionName;
		}
		#region Get
		public DeadLetterEntry<TPayload> Get(string id) => GetAsync(id).Result;
		public async Task<DeadLetterEntry<TPayload>> GetAsync(string id)
		{
			using var ctx = await context.CreateDbContextAsync();
			return await ctx.DeadLetterEntries.FindAsync(id);
		}
		#endregion

		#region GetAll
		public IEnumerable<DeadLetterEntry<TPayload>> GetAll() => GetAllAsync().Result;
		public async Task<IEnumerable<DeadLetterEntry<TPayload>>> GetAllAsync()
		{
			using var ctx = await context.CreateDbContextAsync();
			return await ctx.DeadLetterEntries.ToListAsync();
		}
		#endregion

		#region Count
		public int Count() => CountAsync().Result;
		public async Task<int> CountAsync()
		{
			using var ctx = await context.CreateDbContextAsync();
			return await ctx.DeadLetterEntries.CountAsync();
		}
		#endregion

		#region Insert
		public void Insert(DeadLetterEntry<TPayload> entry) => Task.WaitAll(InsertAsync(entry));
		public virtual async Task InsertAsync(DeadLetterEntry<TPayload> entry)
		{
			using var ctx = await context.CreateDbContextAsync();
			await ctx.DeadLetterEntries.AddAsync(entry);
			await ctx.SaveChangesAsync();
		}

		#endregion

		#region Update
		public void Update(DeadLetterEntry<TPayload> entry) =>
	Task.WaitAll(UpdateAsync(entry));
		public async Task UpdateAsync(DeadLetterEntry<TPayload> entry)
		{
			using var ctx = await context.CreateDbContextAsync();
			ctx.Entry(entry);
			await ctx.SaveChangesAsync();
		}

		#endregion

		#region Delete
		public void Delete(string id) => Task.WaitAll(DeleteAsync(id));
		public async Task DeleteAsync(string id)
		{
			using var ctx = await context.CreateDbContextAsync();
			var entity = ctx.DeadLetterEntries.FindAsync(id);
			if (entity != null)
				ctx.Entry(entity).State = EntityState.Deleted;
			await ctx.SaveChangesAsync();
		}

		#endregion

		#region Clear
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

		#endregion	}
	}
}
