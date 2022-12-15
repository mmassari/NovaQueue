using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NovaQueue.Abstractions;
using NovaQueue.Abstractions.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

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
				_logger.LogInformation($"The queue start a new task with id {item.Id}...");
				var result = await _job.RunWorkerAsync(item);

				//_queue.HandleResult(item, result);
				item.LastAttempt = DateTime.Now;
				item.Attempts++;
				item.IsCheckedOut = false;

				if (result.Success)
				{
					_queue.Commit(item);
					_logger.LogInformation("Queue Job completed successfully!");
				}
				else
				{
					_logger.LogError("Queue Job failed");
					if (result is ValidationErrorResult validationResult)
						_queue.Abort(item, ErrorType.Validation, validationResult.Errors.ToArray());
					else if (result is ErrorResult errorResult)
						_queue.Abort(item, ErrorType.JobExecution, errorResult.Errors.Select(c=>new Error(c.Message, c.Exception)).ToArray());
					else
						_queue.Abort(item, ErrorType.Unhandled, new Error("Unexpected error. ErrorResult is null"));
				};
			}
			catch (Exception ex)
			{
				_logger.LogInformation($"The task {item.Id} is failed with error {ex}");
				_queue.Abort(item, ErrorType.Unhandled, new Error(ex.Message));
			}
		}

		public Task RunAsync(QueueEntry<TPayload> entry)
		{
			throw new NotImplementedException();
		}
	}

}