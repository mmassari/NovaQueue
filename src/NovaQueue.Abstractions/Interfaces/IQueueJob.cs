using NovaQueue.Abstractions.Models;
using System;

namespace NovaQueue.Abstractions
{
	public interface IQueueJob<T>
	{
		Result<QueueEntryLog> RunWorker(T payload);
	}

}