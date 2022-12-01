using LiteDB;
using NovaQueue.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NovaQueue.Persistence.LiteDB;

public class LiteDBRepository : IQueueRepository
{
	private ILiteCollection<QueueEntry<object>> _collection;
	private ILiteCollection<DeadLetterEntry<object>> _deadLetterCollection;
	private ILiteCollection<CompletedEntry<object>> _completedCollection;
	private string _collectionName;
	private string _deadLetterCollectionName;
	private string _completedCollectionName;
	private LiteDatabase Database = null;
	private object _dequeueLock = new();
	private bool _deadLetterQueueEnabled;
	private bool _completedQueueEnabled;
	public LiteDBRepository(string connectionString)
	{
		Database = new LiteDatabase(connectionString);
	}
	public void Initialize(string name, bool deadLetterQueueEnabled = false, bool completedQueueEnabled = false)
	{
		_collectionName = name;
		_deadLetterCollectionName = name + "_DeadLetter";
		_completedCollectionName = name + "_Completed";
		_deadLetterQueueEnabled = deadLetterQueueEnabled;
		_completedQueueEnabled = completedQueueEnabled;

		_collection = Database.GetCollection<QueueEntry<object>>(_collectionName);
		_collection.EnsureIndex(x => x.Id);
		_collection.EnsureIndex(x => x.IsCheckedOut);

		if (deadLetterQueueEnabled)
		{
			_deadLetterCollection = Database.GetCollection<DeadLetterEntry<object>>(_deadLetterCollectionName);
			_deadLetterCollection.EnsureIndex(x => x.Id);

		}
		if (completedQueueEnabled)
		{
			_completedCollection = Database.GetCollection<CompletedEntry<object>>(_completedCollectionName);
			_completedCollection.EnsureIndex(x => x.Id);

		}

	}
	public void InsertBulk(IEnumerable<QueueEntry<object>> entries) =>
		_collection.InsertBulk(entries);
	public void Insert(QueueEntry<object> entry) =>
		_collection.Insert(entry);
	public void Update(QueueEntry<object> entry) =>
		_collection.Update(entry);
	public void Delete(long id) =>
		_collection.Delete(new BsonValue(id));

	public void Delete(IEnumerable<QueueEntry<object>> entries)
	{
		var _dequeueLock = new object();
		lock (_dequeueLock)
		{
			try
			{
				Database.BeginTrans();

				foreach (var item in entries)
					_collection.Delete(item.Id);

				Database.Commit();
			}
			catch (Exception)
			{
				Database.Rollback();
				throw;
			}
		}
	}
	public IEnumerable<QueueEntry<object>> CheckoutEntries(int maxEntries)
	{
		lock (_dequeueLock)
		{
			var result = _collection.Find(c => c.IsCheckedOut == false, 0, maxEntries).ToList();
			if (result.Count() > 0)
				Debug.WriteLine($" € {result.First().Payload} ");

			foreach (var item in result)
			{
				item.IsCheckedOut = true;
				//_collection.Update(new BsonValue(item.Id), item);
				Database.Execute($"UPDATE {_collectionName} SET IsCheckedOut=true WHERE _id = {item.Id}");
				//					_collection.Update(item);
				//					Debug.WriteLine($" € {item.Payload} ");
			}
			if (result.Count() > 0)
				Debug.WriteLine($" $ {result.First().Payload} ");
			return result;
		}
	}
	public IEnumerable<QueueEntry<object>> GetCheckoutEntries() =>
		_collection.Find(c => c.IsCheckedOut == true);
	public IEnumerable<QueueEntry<object>> GetEntries(int maxItems) =>
		_collection.Find(c => c.IsCheckedOut == false, 0, maxItems);
	public IEnumerable<CompletedEntry<object>> GetCompletedEntries() =>
		_completedCollection.FindAll();
	public IEnumerable<QueueEntry<object>> GetDeadLetterEntries() =>
		_collection.FindAll();
	public bool HaveEntries() =>
		_collection.Count(c => c.IsCheckedOut == false) > 0;

	public void MoveToDeadLetter(QueueEntry<object> entry)
	{
		if (!_deadLetterQueueEnabled)
			throw new InvalidOperationException("DeadLetterQueue is not enabled");

		try
		{
			Database.BeginTrans();

			_deadLetterCollection.Insert(new DeadLetterEntry<object>(entry));
			Delete(entry.Id);

			Database.Commit();
		}
		catch (Exception ex)
		{
			Database.Rollback();
		}
	}
	public void MoveToLastPosition(QueueEntry<object> item)
	{
		try
		{
			Database.BeginTrans();

			Delete(item.Id);
			item.Id = 0;
			Insert(item);

			Database.Commit();
		}
		catch (Exception)
		{
			Database.Rollback();
		}
	}
	public void MoveToCompleted(QueueEntry<object> entry)
	{
		if (!_completedQueueEnabled)
			throw new InvalidOperationException("CompletedQueue is not enabled");

		try
		{
			Database.BeginTrans();

			Delete(entry.Id);
			var completed = new CompletedEntry<object>(entry);
			_completedCollection.Insert(completed);

			Database.Commit();
		}
		catch (Exception)
		{
			Database.Rollback();
		}
	}
	public void RestoreFromDeadLetter(IEnumerable<DeadLetterEntry<object>> entries)
	{
		if (!_deadLetterQueueEnabled)
			throw new InvalidOperationException("DeadLetterQueue is not enabled");

		try
		{
			Database.BeginTrans();

			foreach (var entry in entries)
			{
				var item = new QueueEntry<object>(entry.Payload);
				_deadLetterCollection.Delete(entry.Id);
				_collection.Insert(item);
			}

			Database.Commit();
		}
		catch (Exception)
		{
			Database.Rollback();
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
			_collection.DeleteAll();

		if (collection.HasFlag(CollectionType.DeadLetter))
			_deadLetterCollection.DeleteAll();

		if (collection.HasFlag(CollectionType.Completed))
			_completedCollection.DeleteAll();
	}
	public void DropCollection()
	{
		Database.DropCollection(_collectionName);
		Database.DropCollection(_completedCollectionName);
		Database.DropCollection(_deadLetterCollectionName);
	}
	public void Dispose()
	{
		Database.Dispose();
		Database = null;
	}
}
