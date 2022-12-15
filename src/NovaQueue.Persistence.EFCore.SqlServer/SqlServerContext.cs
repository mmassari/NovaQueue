using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NovaQueue.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NovaQueue.Persistence.EFCore.SqlServer
{
	public class SqlServerContext<TPayload> : IDatabaseContext<TPayload>
	{
		private readonly PersistenceOptions persistenceOptions;
		private readonly IDbContextFactory<NovaQueueDbContext<TPayload>> contextFactory;

		public DbSet<QueueEntry<TPayload>> QueueEntry { get; set; }
		public DbSet<CompletedEntry<TPayload>> CompletedEntry { get; set; }
		public DbSet<DeadLetterEntry<TPayload>> DeadLetterEntry { get; set; }
		public QueueOptions<TPayload> QueueOptions { get; }
		public string CollectionName { get; private set; }
		public string DeadLetterCollectionName { get; private set; }
		public string CompletedCollectionName { get; private set; }
		public bool DeadLetterQueueEnabled => QueueOptions.DeadLetter.IsEnabled;
		public bool CompletedQueueEnabled => QueueOptions.Completed.IsEnabled;
		public SqlServerContext(
			IOptions<PersistenceOptions> persistenceOptions,
			IOptions<QueueOptions<TPayload>> queueOptions,
			IDbContextFactory<NovaQueueDbContext<TPayload>> contextFactory)
		{
			QueueOptions = queueOptions.Value;
			CollectionName = $"{QueueOptions.Name}_Queue";
			DeadLetterCollectionName = $"{QueueOptions.Name}_DeadLetter";
			CompletedCollectionName = $"{QueueOptions.Name}_Completed";
			this.persistenceOptions = persistenceOptions.Value;
			this.contextFactory = contextFactory;			
		}
		public NovaQueueDbContext<TPayload> CreateDbContext() => 
			contextFactory.CreateDbContext();
		public async Task<NovaQueueDbContext<TPayload>> CreateDbContextAsync() => 
			await contextFactory.CreateDbContextAsync();
		public void BeginTransaction()
		{
			
		}

		public void CommitTransaction()
		{
			
		}

		public void RollbackTransaction()
		{
			
		}
	}
}
