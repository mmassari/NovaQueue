using NovaQueue.Abstractions.Models;
using System.Threading.Tasks;

namespace NovaQueue.Abstractions
{
	public interface IQueueJobAsync<T>
	{
		Task<Result<QueueEntryLog>> RunWorkerAsync(T payload);
	}

}