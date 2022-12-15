using Microsoft.Extensions.Logging;
using NovaQueue.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaQueue.Persistence.EFCore.SqlServer
{
	public class SqlServerUnitOfWork<TPayload> : IUnitOfWork<TPayload>
		where TPayload : class
	{
		private readonly SqlServerContext<TPayload> databaseContext;
		private readonly ILogger<SqlServerUnitOfWork<TPayload>> logger;

		public SqlServerUnitOfWork(IDatabaseContext<TPayload> databaseContext, ILogger<SqlServerUnitOfWork<TPayload>> logger)
		{
			this.databaseContext = databaseContext as SqlServerContext<TPayload>;
			this.logger = logger;
		}

		public void MoveToCompleted(QueueEntry<TPayload> entry) =>
			Task.WaitAll(MoveToCompletedAsync(entry));
		public async Task MoveToCompletedAsync(QueueEntry<TPayload> entry)
		{
			using var ctx = await databaseContext.CreateDbContextAsync();
			var transaction = await ctx.Database.BeginTransactionAsync(); 
			try
			{
				await ctx.CompletedEntries.AddAsync(new CompletedEntry<TPayload>(entry));
				ctx.QueueEntries.Remove(entry);
				await ctx.SaveChangesAsync();
				await transaction.CommitAsync();
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				logger.LogError(ex, "Error in MoveToCompletedAsync");
				throw;
			}
		}
		public void MoveToDeadLetter(QueueEntry<TPayload> entry) =>
			Task.WaitAll(MoveToDeadLetterAsync(entry));
		public async Task MoveToDeadLetterAsync(QueueEntry<TPayload> entry)
		{
			using var ctx = await databaseContext.CreateDbContextAsync();
			var transaction = await ctx.Database.BeginTransactionAsync();
			try
			{
				await ctx.DeadLetterEntries.AddAsync(new DeadLetterEntry<TPayload>(entry));
				ctx.QueueEntries.Remove(entry);
				await ctx.SaveChangesAsync();
				await transaction.CommitAsync();
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				logger.LogError(ex, "Error in MoveToDeadLetterAsync");
				throw;
			}
		}
		public void EnqueueFromDeadLetter(DeadLetterEntry<TPayload> entry) =>
			Task.WaitAll(EnqueueFromDeadLetterAsync(entry));
		public async Task EnqueueFromDeadLetterAsync(DeadLetterEntry<TPayload> deadLetterEntry)
		{
			using var ctx = await databaseContext.CreateDbContextAsync();
			var transaction = await ctx.Database.BeginTransactionAsync();
			try
			{
				var queueEntry = new QueueEntry<TPayload>()
				{
					Id = deadLetterEntry.Id,
					Payload = deadLetterEntry.Payload,
				};
				await ctx.QueueEntries.AddAsync(queueEntry);
				ctx.DeadLetterEntries.Remove(deadLetterEntry);
				await ctx.SaveChangesAsync();
				await transaction.CommitAsync();
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				logger.LogError(ex, "Error in EnqueueFromDeadLetterAsync");
				throw;
			}
		}
		public void EnqueueFromCompleted(CompletedEntry<TPayload> entry) =>
			Task.WaitAll(EnqueueFromCompletedAsync(entry));
		public async Task EnqueueFromCompletedAsync(CompletedEntry<TPayload> completedEntry)
		{
			using var ctx = await databaseContext.CreateDbContextAsync();
			var transaction = await ctx.Database.BeginTransactionAsync();
			try
			{
				var queueEntry = new QueueEntry<TPayload>()
				{
					Id = completedEntry.Id,
					Payload = completedEntry.Payload,
				};
				await ctx.QueueEntries.AddAsync(queueEntry);
				ctx.CompletedEntries.Remove(completedEntry);
				await ctx.SaveChangesAsync();
				await transaction.CommitAsync();
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				logger.LogError(ex, "Error in EnqueueFromCompletedAsync");
				throw;
			}
		}
	}
}
