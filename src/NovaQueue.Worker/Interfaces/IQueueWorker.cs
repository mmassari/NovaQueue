using Microsoft.Extensions.Hosting;
using NovaQueue.Abstractions;
using System;
using System.Threading.Tasks;

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
		Task RunAsync(QueueEntry<TPayload> entry);
	}
}