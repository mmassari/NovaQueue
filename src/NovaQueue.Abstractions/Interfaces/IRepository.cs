using System;
using System.Collections.Generic;

namespace NovaQueue.Abstractions
{
	public interface IQueueRepository : IDisposable
	{
		void Initialize(string name, bool deadLetterQueueEnabled = false, bool completedQueueEnabled = false);
		void Delete(long id);
		void Delete(IEnumerable<QueueEntry<object>> entries);
		IEnumerable<QueueEntry<object>> GetCheckoutEntries();
		IEnumerable<CompletedEntry<object>> GetCompletedEntries();
		IEnumerable<QueueEntry<object>> GetDeadLetterEntries();
		IEnumerable<QueueEntry<object>> GetEntries(int maxItems);
		bool HaveEntries();
		void Insert(QueueEntry<object> entry);
		void InsertBulk(IEnumerable<QueueEntry<object>> entries);
		void MoveToCompleted(QueueEntry<object> entry);
		void MoveToDeadLetter(QueueEntry<object> entry);
		void RestoreFromDeadLetter(IEnumerable<DeadLetterEntry<object>> entries);
		IEnumerable<QueueEntry<object>> CheckoutEntries(int maxItems);
		void Update(QueueEntry<object> entry);
		void MoveToLastPosition(QueueEntry<object> item);
		int Count(CollectionType collection);
		void Clear(CollectionType collection);
		void DropCollection();
		int CountNew();
	}
}