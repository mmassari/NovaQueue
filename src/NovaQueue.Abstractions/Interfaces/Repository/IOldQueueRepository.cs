using System;
using System.Collections.Generic;

namespace NovaQueue.Abstractions.Interfaces.Repository
{
    public interface IOldQueueRepository<TPayload> : IDisposable
    {
        void Delete(string id);
        void Delete(IEnumerable<QueueEntry<TPayload>> entries);
        IEnumerable<QueueEntry<TPayload>> GetCheckoutEntries();
        IEnumerable<CompletedEntry<TPayload>> GetCompletedEntries();
        IEnumerable<QueueEntry<TPayload>> GetDeadLetterEntries();
        IEnumerable<QueueEntry<TPayload>> GetEntries(int maxItems);
        bool HaveEntries();
        void Insert(QueueEntry<TPayload> entry);
        void InsertBulk(IEnumerable<QueueEntry<TPayload>> entries);
        void MoveToCompleted(QueueEntry<TPayload> entry);
        void MoveToDeadLetter(QueueEntry<TPayload> entry);
        void RestoreFromDeadLetter(IEnumerable<DeadLetterEntry<TPayload>> entries);
        IEnumerable<QueueEntry<TPayload>> CheckoutEntries(int maxItems);
        void Update(QueueEntry<TPayload> entry);
        void MoveToLastPosition(QueueEntry<TPayload> item);
        int Count(CollectionType collection);
        void Clear(CollectionType collection);
        void DropCollection();
        int CountNew();
    }
}