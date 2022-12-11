using Microsoft.Extensions.Hosting;
using NovaQueue.Abstractions;

namespace NovaQueue.Worker
{
	public interface IQueueWorkerAsync<TPayload> : IHostedService, IDisposable
		where TPayload : class
	{
		void Enqueue(TPayload payload);
		Task EnqueueAsync(TPayload payload);
		Task RunAsync(QueueEntry<TPayload> entry);
	}
}