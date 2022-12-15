using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NovaQueue.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaQueue.Persistence.EFCore.SqlServer
{
	public class NovaQueueDbContext<TPayload> : DbContext
	{
		private readonly PersistenceOptions persistenceOptions;
		private readonly UpdateSortOnQueueInsertInterceptor<TPayload> updateSortOnQueueInsertInterceptor;

		public DbSet<QueueEntry<TPayload>> QueueEntries { get; set; }
		public DbSet<CompletedEntry<TPayload>> CompletedEntries { get; set; }
		public DbSet<DeadLetterEntry<TPayload>> DeadLetterEntries { get; set; }
		public QueueOptions<TPayload> QueueOptions { get; }
		public string CollectionName { get; private set; }
		public string DeadLetterCollectionName { get; private set; }
		public string CompletedCollectionName { get; private set; }
		public bool DeadLetterQueueEnabled => QueueOptions.DeadLetter.IsEnabled;
		public bool CompletedQueueEnabled => QueueOptions.Completed.IsEnabled;

		public NovaQueueDbContext(
			IOptions<PersistenceOptions> persistenceOptions,
			IOptions<QueueOptions<TPayload>> queueOptions,
			DbContextOptions<NovaQueueDbContext<TPayload>> options,
			UpdateSortOnQueueInsertInterceptor<TPayload> updateSortOnQueueInsertInterceptor) : base(options)
		{
			QueueOptions = queueOptions.Value;
			CollectionName = $"{QueueOptions.Name}_Queue";
			DeadLetterCollectionName = $"{QueueOptions.Name}_DeadLetter";
			CompletedCollectionName = $"{QueueOptions.Name}_Completed";
			this.persistenceOptions = persistenceOptions.Value;
			this.updateSortOnQueueInsertInterceptor = updateSortOnQueueInsertInterceptor;
		}
		protected override void OnConfiguring(DbContextOptionsBuilder dbContextOptions)
		{
			dbContextOptions.UseSqlServer(persistenceOptions.ConnectionString);
			dbContextOptions.AddInterceptors(updateSortOnQueueInsertInterceptor);
		}
		protected override void OnModelCreating(ModelBuilder builder)
		{
			builder.Entity<QueueEntry<TPayload>>(b =>
			{
				b.ToTable(CollectionName);
				b.HasKey(c => c.Id);
				b.HasIndex(c => c.Sort);
				b.Property(c => c.Payload)
					.HasColumnType("varchar(max)")
					.IsRequired()
					.HasConversion(
						v => JsonConvert.SerializeObject(v),
						v => JsonConvert.DeserializeObject<TPayload>(v)
					);
				b.Property(c => c.Errors)
					.HasColumnType("varchar(max)")
					.HasConversion(
						v => JsonConvert.SerializeObject(v),
						v => JsonConvert.DeserializeObject<List<AttemptErrors>>(v)
					);
				b.Property(c => c.Logs)
					.HasColumnType("varchar(max)")
					.HasConversion(
						v => JsonConvert.SerializeObject(v),
						v => JsonConvert.DeserializeObject<List<AttemptLogs>>(v)
					);
			});
			if (QueueOptions.DeadLetter.IsEnabled)
			{
				builder.Entity<DeadLetterEntry<TPayload>>(b =>
				{
					b.ToTable(DeadLetterCollectionName);
					b.HasKey(c => c.Id);
					b.Property(c => c.Payload)
						.HasColumnType("varchar(max)")
						.IsRequired()
						.HasConversion(
							v => JsonConvert.SerializeObject(v),
							v => JsonConvert.DeserializeObject<TPayload>(v)
						);
					b.Property(c => c.Errors)
						.HasColumnType("varchar(max)")
						.HasConversion(
							v => JsonConvert.SerializeObject(v),
							v => JsonConvert.DeserializeObject<List<AttemptErrors>>(v)
						);
					b.Property(c => c.Logs)
						.HasColumnType("varchar(max)")
						.HasConversion(
							v => JsonConvert.SerializeObject(v),
							v => JsonConvert.DeserializeObject<List<AttemptLogs>>(v)
						);
				});
			}
			if (QueueOptions.Completed.IsEnabled)
			{
				builder.Entity<CompletedEntry<TPayload>>(b =>
				{
					b.ToTable(CompletedCollectionName);
					b.HasKey(c => c.Id);
					b.Property(c => c.Payload)
						.HasColumnType("varchar(max)")
						.IsRequired()
						.HasConversion(
							v => JsonConvert.SerializeObject(v),
							v => JsonConvert.DeserializeObject<TPayload>(v)
						);
					b.Property(c => c.Errors)
						.HasColumnType("varchar(max)")
						.HasConversion(
							v => JsonConvert.SerializeObject(v),
							v => JsonConvert.DeserializeObject<List<AttemptErrors>>(v)
						);
					b.Property(c => c.Logs)
						.HasColumnType("varchar(max)")
						.HasConversion(
							v => JsonConvert.SerializeObject(v),
							v => JsonConvert.DeserializeObject<List<AttemptLogs>>(v)
						);
				});
			}
		}

		public void BeginTransaction()
		{
			Database.BeginTransaction();
		}

		public void CommitTransaction()
		{
			Database.CommitTransaction();
		}

		public void RollbackTransaction()
		{
			Database.RollbackTransaction();
		}
	}
}
