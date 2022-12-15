using Microsoft.EntityFrameworkCore;
using NovaQueue.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NovaQueue.Persistence.EFCore.SqlServer
{
	public class SqlServerCompletedRepository<TPayload> :
		SqlServerRepositoryBase<TPayload>, ICompletedRepository<TPayload>
		where TPayload : class
	{
		public SqlServerCompletedRepository(IDatabaseContext<TPayload> context) : base(context)
		{
			tableName = context.DeadLetterCollectionName;
		}

		#region Get
		public CompletedEntry<TPayload> Get(string id) => GetAsync(id).Result;
		public async Task<CompletedEntry<TPayload>> GetAsync(string id)
		{
			using var ctx = await context.CreateDbContextAsync();
			return await ctx.CompletedEntries.FindAsync(id);
		}
		#endregion

		#region GetAll
		public IEnumerable<CompletedEntry<TPayload>> GetAll() => GetAllAsync().Result;
		public async Task<IEnumerable<CompletedEntry<TPayload>>> GetAllAsync()
		{
			using var ctx = await context.CreateDbContextAsync();
			return await ctx.CompletedEntries.ToListAsync();
		}
		#endregion

		#region Count
		public int Count() => CountAsync().Result;
		public async Task<int> CountAsync()
		{
			using var ctx = await context.CreateDbContextAsync();
			return await ctx.CompletedEntries.CountAsync();
		}
		#endregion

		#region Insert
		public void Insert(CompletedEntry<TPayload> entry) => Task.WaitAll(InsertAsync(entry));
		public virtual async Task InsertAsync(CompletedEntry<TPayload> entry)
		{
			using var ctx = await context.CreateDbContextAsync();
			try
			{
				ctx.BeginTransaction();
				await ctx.CompletedEntries.AddAsync(entry);
				await ctx.SaveChangesAsync();
				ctx.CommitTransaction();

			}
			catch (Exception)
			{
				ctx.RollbackTransaction();
				throw;
			}
		}

		#endregion

		#region Update
		public void Update(CompletedEntry<TPayload> entry) =>
	Task.WaitAll(UpdateAsync(entry));
		public async Task UpdateAsync(CompletedEntry<TPayload> entry)
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
			var entity = ctx.CompletedEntries.FindAsync(id);
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