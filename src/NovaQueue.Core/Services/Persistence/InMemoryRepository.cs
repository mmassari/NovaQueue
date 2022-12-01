using NovaQueue.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace NovaQueue.Core;
public class InMemoryRepository : IQueueRepository
{
	private List<QueueEntry<object>> _collection;
	private List<DeadLetterEntry<object>> _deadLetterCollection;
	private List<CompletedEntry<object>> _completedCollection;
	private object _dequeueLock = new();

	public InMemoryRepository(string connectionString)
	{
	}
	public void Initialize(string name, bool deadLetterQueueEnabled = false, bool completedQueueEnabled = false)
	{
		_collection = new List<QueueEntry<object>>();
		
		if(deadLetterQueueEnabled)
			_deadLetterCollection = new List<DeadLetterEntry<object>>();
		
		if(completedQueueEnabled)
			_completedCollection = new List<CompletedEntry<object>>();
	}
	public void InsertBulk(IEnumerable<QueueEntry<object>> entries) =>
		_collection.AddRange(entries);
	public void Insert(QueueEntry<object> entry) =>
		_collection.Add(entry);
	public void Update(QueueEntry<object> entry) =>
		_collection[_collection.FindIndex(c => c.Id == entry.Id)] = entry;
	public void Delete(long id) =>
		_collection.RemoveAt(_collection.FindIndex(c => c.Id == id));

	public void Delete(IEnumerable<QueueEntry<object>> entries)
	{
		lock (_dequeueLock)
		{
			foreach (var item in entries)
				_collection.RemoveAt(_collection.FindIndex(c => c.Id == item.Id));
		}
	}
	public IEnumerable<QueueEntry<object>> CheckoutEntries(int maxEntries)
	{
		lock (_dequeueLock)
		{
			var result = _collection.Where(c => c.IsCheckedOut == false).Take(maxEntries).ToList();

			foreach (var item in result)
				_collection[_collection.FindIndex(c => c.Id == item.Id)].IsCheckedOut = true;

			return _collection.Where(c => c.IsCheckedOut == false).Take(maxEntries).ToList();
		}
	}
	public IEnumerable<QueueEntry<object>> GetCheckoutEntries() =>
		_collection.Where(c => c.IsCheckedOut == true);
	public IEnumerable<QueueEntry<object>> GetEntries(int maxItems) =>
		_collection.Where(c => c.IsCheckedOut == false).Take(maxItems);
	public IEnumerable<CompletedEntry<object>> GetCompletedEntries() =>
		_completedCollection;
	public IEnumerable<QueueEntry<object>> GetDeadLetterEntries() =>
		_collection;
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
