using Microsoft.Extensions.Hosting;

namespace NovaQueue.Worker
{
	public interface IQueueWorkerAsync<TPayload> : IHostedService, IDisposable
		where TPayload : class
	{
		void Enqueue(TPayload payload);
		Task EnqueueAsync(TPayload payload);
	}
}