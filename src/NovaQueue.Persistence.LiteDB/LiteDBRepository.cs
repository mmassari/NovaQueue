using LiteDB;
using NovaQueue.Abstractions;
using NovaQueue.Abstractions.Interfaces.Repository;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NovaQueue.Persistence.LiteDB;
public class LiteDBQueueRepository<TPayload> : IQueueRepository<TPayload>
{
	private object _dequeueLock = new();
	private readonly LiteDBContext<TPayload> context;

	public LiteDBQueueRepository(LiteDBContext<TPayload> context)
	{
		this.context = context; 
	}

	public void InsertBulk(IEnumerable<QueueEntry<TPayload>> entries) =>
		context.Collection.InsertBulk(entries);
	public void Insert(QueueEntry<TPayload> entry) =>
		context.Collection.Insert(entry);
	public void Update(QueueEntry<TPayload> entry) =>
		context.Collection.Update(entry);
	public void Delete(string id) =>
		context.Collection.Delete(new BsonValue(id));

	public void Delete(IEnumerable<QueueEntry<TPayload>> entries)
	{
		var _dequeueLock = new object();
		lock (_dequeueLock)
		{
			try
			{
				context.Database.BeginTrans();

				foreach (var item in entries)
					context.Collection.Delete(item.Id);

				context.Database.Commit();
			}
			catch (Exception)
			{
				context.Database.Rollback();
				throw;
			}
		}
	}
	public IEnumerable<QueueEntry<TPayload>> CheckoutEntries(int maxEntries)
	{
		lock (_dequeueLock)
		{
			var result = context.Collection.Find(c => c.IsCheckedOut == false, 0, maxEntries).ToList();
			if (result.Count() > 0)
				Debug.WriteLine($" € {result.First().Payload} ");

			foreach (var item in result)
			{
				item.IsCheckedOut = true;
				//_collection.Update(new BsonValue(item.Id), item);
				context.Database.Execute($"UPDATE {context.CollectionName} SET IsCheckedOut=true WHERE _id = {item.Id}");
				//					_collection.Update(item);
				//					Debug.WriteLine($" € {item.Payload} ");
			}
			if (result.Count() > 0)
				Debug.WriteLine($" $ {result.First().Payload} ");
			return result;
		}
	}
	public IEnumerable<QueueEntry<TPayload>> GetCheckoutEntries() =>
		context.Collection.Find(c => c.IsCheckedOut == true);
	public IEnumerable<QueueEntry<TPayload>> GetEntries(int maxItems) =>
		context.Collection.Find(c => c.IsCheckedOut == false, 0, maxItems);
	public IEnumerable<CompletedEntry<TPayload>> GetCompletedEntries() =>
		context.CompletedCollection.FindAll();
	public IEnumerable<QueueEntry<TPayload>> GetDeadLetterEntries() =>
		context.Collection.FindAll();
	public bool HaveEntries() =>
		context.Collection.Count(c => c.IsCheckedOut == false) > 0;

	public void MoveToDeadLetter(QueueEntry<TPayload> entry)
	{
		if (!context.DeadLetterQueueEnabled)
			throw new InvalidOperationException("DeadLetterQueue is not enabled");

		try
		{
			context.Database.BeginTrans();

			context.DeadLetterCollection.Insert(new DeadLetterEntry<TPayload>(entry));
			Delete(entry.Id);

			context.Database.Commit();
		}
		catch (Exception ex)
		{
			context.Database.Rollback();
		}
	}
	public void MoveToLastPosition(QueueEntry<TPayload> item)
	{
		try
		{
			context.Database.BeginTrans();

			Delete(item.Id);
			Insert(item);

			context.Database.Commit();
		}
		catch (Exception)
		{
			context.Database.Rollback();
		}
	}
	public void MoveUp(QueueEntry<TPayload> entry)
	{

	}
	public void MoveToCompleted(QueueEntry<TPayload> entry)
	{
		if (!context.CompletedQueueEnabled)
			throw new InvalidOperationException("CompletedQueue is not enabled");

		try
		{
			context.Database.BeginTrans();

			Delete(entry.Id);
			var completed = new CompletedEntry<TPayload>(entry);
			context.CompletedCollection.Insert(completed);

			context.Database.Commit();
		}
		catch (Exception)
		{
			context.Database.Rollback();
		}
	}
	public void RestoreFromDeadLetter(IEnumerable<DeadLetterEntry<TPayload>> entries)
	{
		if (!context.DeadLetterQueueEnabled)
			throw new InvalidOperationException("DeadLetterQueue is not enabled");

		try
		{
			context.Database.BeginTrans();

			foreach (var entry in entries)
			{
				var item = new QueueEntry<TPayload>(entry.Payload);
				context.DeadLetterCollection.Delete(entry.Id);
				context.Collection.Insert(item);
			}

			context.Database.Commit();
		}
		catch (Exception)
		{
			context.Database.Rollback();
		}
	}
	public int CountNew() => context.Collection.Count(c => !c.IsCheckedOut);
	public int Count(CollectionType collection)
	{
		int count = 0;

		if (collection.HasFlag(CollectionType.Queue))
			count += context.Collection.Count();

		if (collection.HasFlag(CollectionType.DeadLetter))
			count += context.DeadLetterCollection.Count();

		if (collection.HasFlag(CollectionType.Completed))
			count += context.CompletedCollection.Count();

		return count;
	}

	public void Clear(CollectionType collection)
	{
		if (collection.HasFlag(CollectionType.Queue))
			context.Collection.DeleteAll();

		if (collection.HasFlag(CollectionType.DeadLetter))
			context.DeadLetterCollection.DeleteAll();

		if (collection.HasFlag(CollectionType.Completed))
			context.CompletedCollection.DeleteAll();
	}
	public void DropCollection()
	{
		context.Database.DropCollection(context.CollectionName);
		context.Database.DropCollection(context.CompletedCollectionName);
		context.Database.DropCollection(context.DeadLetterCollectionName);
	}
	public void Dispose()
	{
		context.Database.Dispose();
	}
}
