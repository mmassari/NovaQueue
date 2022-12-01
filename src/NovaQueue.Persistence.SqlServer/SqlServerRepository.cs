using Dapper;
using Dapper.FluentMap;
using Dapper.FluentMap.Dommel;
using NovaQueue.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace NovaQueue.Persistence.SqlServer
{
	public class SqlServerRepository : IRepository
	{
		private string _tableName;
		private string _deadLetterTableName;
		private string _completedTableName;
		private object _dequeueLock = new();
		private QueueDataStore _dataStore;
		private CompletedDataStore _completedDataStore;
		private DeadLetterDataStore _deadLetterDataStore;
		
		public SqlServerRepository(string connectionString, string collectionName)
		{
			_dataStore = new QueueDataStore(connectionString);
			_completedDataStore = new CompletedDataStore(connectionString);
			_deadLetterDataStore = new DeadLetterDataStore(connectionString);
		}

		public void Initialize(string name, bool deadLetterQueueEnabled = false, bool completedQueueEnabled = false)
		{
			_tableName = name;
			_deadLetterTableName = $"{name}_DeadLetter";
			_completedTableName = $"{name}_Completed";
			SqlMapper.AddTypeHandler(new JsonTypeHandler());
			FluentMapper.Initialize(config =>
			{
				config.AddMap(new QueueEntryMap(_tableName));
				config.AddMap(new DeadLetterEntryMap(_deadLetterTableName));
				config.AddMap(new CompletedEntryMap(_completedTableName));
				config.ForDommel();
			});
		}

		public async Task InsertBulk(IEnumerable<QueueEntry<object>> entries) =>
			await _dataStore.InsertAsync(entries);
		public async Task Insert(QueueEntry<object> entry) =>
			await _dataStore.InsertAsync(entry);
		public async Task Update(QueueEntry<object> entry) =>
			await _dataStore.UpdateAsync(entry);
		public async Task Delete(QueueEntry<object> entry) =>
			await _dataStore.DeleteAsync(entry);

		public async Task Delete(IEnumerable<QueueEntry<object>> entries)
		{
			lock (_dequeueLock)
			{
				foreach (var item in entries)
					await _dataStore.DeleteAsync(item);
			}
		}

		public async Task<IEnumerable<QueueEntry<object>>> CheckoutEntries(int maxEntries)
		{
			var mySemaphore = new SemaphoreSlim(1);
			await mySemaphore.WaitAsync();
			try
			{
				var result = await _dataStore.GetUncheckedOut(maxEntries);

				foreach (var item in result)
				{
					item.IsCheckedOut = true;
					await _dataStore.UpdateAsync(item);
				}
					
				return result;

			}
			finally
			{
				mySemaphore.Release();
			}
		}
	
	public async Task<IEnumerable<QueueEntry<object>>> GetCheckoutEntries() =>
		await _dataStore.GetCheckedOut();
	public async Task<IEnumerable<QueueEntry<object>>> GetEntries(int maxItems) =>
		await _dataStore.GetUncheckedOut(maxItems);
	public async Task<IEnumerable<CompletedEntry<object>>> GetCompletedEntries() =>
		_completedDataStore.GetAll();
	public IEnumerable<QueueEntry<object>> GetDeadLetterEntries() =>
		_deadLetterDataStore.GetAll();
	public bool HaveEntries() =>
		_collection.Count(c => c.IsCheckedOut == false) > 0;

	public void MoveToDeadLetter(QueueEntry<object> entry)
	{
		_deadLetterCollection.Add(new DeadLetterEntry<object>(entry));
		Delete(entry.Id);
	}
	public void MoveToLastPosition(QueueEntry<object> item)
	{
		Delete(item.Id);
		Insert(item);
	}
	public void MoveToCompleted(QueueEntry<object> entry)
	{
		Delete(entry.Id);
		_completedCollection.Add(new CompletedEntry<object>(entry));
	}
	public void RestoreFromDeadLetter(IEnumerable<DeadLetterEntry<object>> entries)
	{
		foreach (var entry in entries)
		{
			var item = new QueueEntry<object>(entry.Payload);
			_deadLetterCollection.Remove(entry);
			Insert(item);
		}
	}
	public int CountNew() => _collection.Count(c => !c.IsCheckedOut);
	public int Count(CollectionType collection)
	{
		int count = 0;

		if (collection.HasFlag(CollectionType.Queue))
			count += _collection.Count();

		if (collection.HasFlag(CollectionType.DeadLetter))
			count += _deadLetterCollection.Count();

		if (collection.HasFlag(CollectionType.Completed))
			count += _completedCollection.Count();

		return count;
	}
	public void Clear(CollectionType collection)
	{
		if (collection.HasFlag(CollectionType.Queue))
			_collection.Clear();

		if (collection.HasFlag(CollectionType.DeadLetter))
			_deadLetterCollection.Clear();

		if (collection.HasFlag(CollectionType.Completed))
			_completedCollection.Clear();
	}
	public void DropCollection() { }
	public void Dispose() { }
}
}
