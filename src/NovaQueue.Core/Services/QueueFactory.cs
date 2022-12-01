using Microsoft.Extensions.Options;
using NovaQueue.Abstractions;
using NovaQueue.Core;

namespace NovaQueueLib
{
	//public class QueueFactory : IQueueFactory
	//{
	//	private readonly NovaQueuePersistenceOptions _persistenceOptions;
	//	private readonly IRepository repository;
	//	public QueueFactory(IRepository repository)
	//	{
	//		this.repository = repository;
	//	}

	//	public ITransactionalQueue<T> CreateQueue<T>(string name, NovaQueueOptions<T> options)
	//	{
	//		IOptions<NovaQueueOptions<T>> opt = 
	//		var queue = new NQueue<T>(repository, options);
	//		return queue;
	//	}
	//	public IQueue<T> CreateSimpleQueue<T>(string name)
	//	{
	//		var queue = new NQueueSimple<T>(repository, name);
	//		return queue;
	//	}

	//}
}
