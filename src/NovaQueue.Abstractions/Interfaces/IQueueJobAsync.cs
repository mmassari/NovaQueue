using NovaQueue.Abstractions.Models;
using System;
using System.Threading.Tasks;

namespace NovaQueue.Abstractions
{
	public interface IQueueJobAsync<T>
	{
		event JobLogEventHandler<T> LogMessageReceived;
		Task<Result> RunWorkerAsync(QueueEntry<T> payload);
	}

}