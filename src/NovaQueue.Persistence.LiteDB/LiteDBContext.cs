using LiteDB;
using Microsoft.Extensions.Options;
using NovaQueue.Abstractions;

namespace NovaQueue.Persistence.LiteDB;

public class LiteDBContext<TPayload> : IDatabaseContext<TPayload>
{
	private readonly PersistenceOptions persistenceOptions;
	private readonly QueueOptions<TPayload> queueOptions;

	public ILiteCollection<QueueEntry<TPayload>> Collection { get; private set; }
	public ILiteCollection<DeadLetterEntry<TPayload>> DeadLetterCollection { get; private set; }
	public ILiteCollection<CompletedEntry<TPayload>> CompletedCollection { get; private set; }
	public bool DeadLetterQueueEnabled { get; private set; }
	public bool CompletedQueueEnabled { get; private set; }
	public string CollectionName { get; private set; }
	public string DeadLetterCollectionName { get; private set; }
	public string CompletedCollectionName { get; private set; }
	public LiteDatabase Database { get; private set; }

	public LiteDBContext(
		IOptions<PersistenceOptions> persistenceOptions,
		IOptions<QueueOptions<TPayload>> queueOptions)
	{
		this.persistenceOptions = persistenceOptions.Value;
		this.queueOptions = queueOptions.Value;

		Database = new LiteDatabase(persistenceOptions.Value.ConnectionString);

		CollectionName = queueOptions.Value.Name;
		DeadLetterCollectionName = queueOptions.Value.Name + "_DeadLetter";
		CompletedCollectionName = queueOptions.Value.Name + "_Completed";

		Collection = Database.GetCollection<QueueEntry<TPayload>>(CollectionName);
		Collection.EnsureIndex(x => x.Id);
		Collection.EnsureIndex(x => x.IsCheckedOut);

		DeadLetterCollection = Database.GetCollection<DeadLetterEntry<TPayload>>(DeadLetterCollectionName);
		DeadLetterCollection.EnsureIndex(x => x.Id);
		CompletedCollection = Database.GetCollection<CompletedEntry<TPayload>>(CompletedCollectionName);
		CompletedCollection.EnsureIndex(x => x.Id);
	}
	public void BeginTransaction()
	{
		Database.BeginTrans();
	}
	public void CommitTransaction()
	{
		Database.Commit();
	}
	public void RollbackTransaction()
	{
		Database.Rollback();
	}
}
