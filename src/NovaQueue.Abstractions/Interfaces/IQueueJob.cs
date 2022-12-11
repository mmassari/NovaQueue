using NovaQueue.Abstractions.Models;
using System;

namespace NovaQueue.Abstractions
{
	public interface IQueueJob<T>
	{
		event JobLogEventHandler<T> LogMessageReceived;
		Result RunWorker(QueueEntry<T> payload);
	}

}