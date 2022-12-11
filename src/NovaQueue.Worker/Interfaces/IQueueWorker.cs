using Microsoft.Extensions.Hosting;

namespace NovaQueue.Worker
{
	public class WorkerStatus
	{
		public bool IsRunning { get; set; }
	}
	public interface IQueueWorker<TPayload> : IHostedService, IDisposable
		where TPayload : class
	{
		void Enqueue(TPayload payload);
		Task EnqueueAsync(TPayload payload);
		Task<WorkerStatus> GetStatusAsync();
		Task StopAsync();
		Task StartAsync();
	}
}