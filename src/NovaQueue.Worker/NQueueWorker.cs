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
			IOptions<QueueOptions<TPayload>> options,
			IQueueJob<TPayload> job)
			: base(logger, queue, options)
		{
			_job = job;
			_job.LogMessageReceived += job_MessageReceived;
		}

		private void job_MessageReceived(QueueEntry<TPayload> sender, string message)
		{
			var attemptLog = new AttemptLogs { Attempt = sender.Attempts, Log=message };
			sender.Logs.Add(attemptLog);
		}

		protected override Task RunJob(QueueEntry<TPayload> item)
		{
			if (item is null)
				throw new NullReferenceException(nameof(item));

			try
			{
				Result result = _job.RunWorker(item);
				_queue.HandleResult(item, result);
			}
			catch (Exception ex)
			{
				_queue.Abort(item, ErrorType.Unhandled, new Error(ex.Message));
			}
			return Task.CompletedTask;
		}
	}

}