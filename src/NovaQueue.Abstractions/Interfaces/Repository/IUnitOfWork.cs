using NovaQueue.Abstractions;
using System.Threading.Tasks;

namespace NovaQueue.Abstractions
{
    public interface IUnitOfWork<TPayload> where TPayload : class
    {
        void EnqueueFromCompleted(CompletedEntry<TPayload> entry);
        Task EnqueueFromCompletedAsync(CompletedEntry<TPayload> completedEntry);
        void EnqueueFromDeadLetter(DeadLetterEntry<TPayload> entry);
        Task EnqueueFromDeadLetterAsync(DeadLetterEntry<TPayload> deadLetterEntry);
        void MoveToCompleted(QueueEntry<TPayload> entry);
        Task MoveToCompletedAsync(QueueEntry<TPayload> entry);
        void MoveToDeadLetter(QueueEntry<TPayload> entry);
        Task MoveToDeadLetterAsync(QueueEntry<TPayload> entry);
    }
}