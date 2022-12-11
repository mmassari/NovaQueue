using System.Collections.Generic;

namespace NovaQueue.Abstractions
{
	public interface IQueueFactory
	{
		ITransactionalQueue<T> CreateQueue<T>(string name, QueueOptions<T> options);
		IQueue<T> CreateSimpleQueue<T>(string name);
	}
}