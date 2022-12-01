using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NovaQueue.Abstractions;
using NovaQueue.Abstractions.Models;

namespace NovaQueue.Worker
{
	public class NQueueWorker<TPayload> : NQueueWorkerBase<TPayload>
		where TPayload : class
	{
		private readonly IQueueJob<TPayload> _job;

		public NQueueWorker(ILogger<NQueueWorker<TPayload>> logger, 
			ITransactionalQueue<TPayload> queue, 
			IOptions<NovaQueueOptions<TPayload>> options,
			IQueueJob<TPayload> job) 
			: base(logger, queue, options)
		{
			_job = job;
		}

		protected override Task RunJob(QueueEntry<TPayload> item)
		{
			if (item is null)
				throw new NullReferenceException(nameof(item));

			try
			{
				Result<QueueEntryLog> result = _job.RunWorker(item.Payload);
				_queue.HandleResult(item,result);
			}
			catch (Exception ex)
			{
				_queue.Abort(item, "Unhandled exception has been thrown: " + ex.Message);
			}
			return Task.CompletedTask;
		}
	}

}