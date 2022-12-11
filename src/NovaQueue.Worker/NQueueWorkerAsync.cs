using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NovaQueue.Abstractions;
using NovaQueue.Abstractions.Models;

namespace NovaQueue.Worker
{
	public class NQueueWorkerAsync<TPayload> : NQueueWorkerBase<TPayload>
		where TPayload : class
	{
		private readonly IQueueJobAsync<TPayload> _job;
		public NQueueWorkerAsync(ILogger<NQueueWorkerAsync<TPayload>> logger,
			ITransactionalQueue<TPayload> queue,
			IOptions<QueueOptions<TPayload>> options,
			IQueueJobAsync<TPayload> job) 
			: base(logger, queue, options)
		{
			_job = job;
			_job.LogMessageReceived += job_MessageReceived;
		}

		private void job_MessageReceived(QueueEntry<TPayload> sender, string message)
		{
			var attemptLog = new AttemptLogs { Attempt = sender.Attempts, Log = message };
			sender.Logs.Add(attemptLog);
		}


		protected override async Task RunJob(QueueEntry<TPayload> item)
		{
			if (item is null)
				throw new NullReferenceException(nameof(item));

			try
			{
				var result = await _job.RunWorkerAsync(item);
				_queue.HandleResult(item,result);
			}
			catch (Exception ex)
			{
				_queue.Abort(item, ErrorType.Unhandled, new Error(ex.Message));
			}
		}
	}

}