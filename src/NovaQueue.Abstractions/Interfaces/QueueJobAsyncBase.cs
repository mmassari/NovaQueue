using NovaQueue.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NovaQueue.Abstractions
{
	public abstract class QueueJobAsyncBase<T> : IQueueJobAsync<T>
	{
		protected QueueEntry<T> entry;
		public event JobLogEventHandler<T> LogMessageReceived;
		protected abstract Task JobExecuteAsync(T payload);
		public async virtual Task<Result> RunWorkerAsync(QueueEntry<T> entry)
		{
			this.entry = entry;
			var errors = await ValidateAsync(entry.Payload);
			if (errors != null && errors.Count > 0)
				return new ValidationErrorResult("Validation Errors", errors.ToArray());

			try
			{
				await JobExecuteAsync(entry.Payload);
				return new SuccessResult();
			}
			catch (Exception ex)
			{
				LogEvent("Exception has been thrown\n" + ex);
				return new ErrorResult("An error has been thrown", new Error(ex.Message));
			}
		}
		protected virtual void LogEvent(string message)
		{
			LogMessageReceived?.Invoke(entry, message);
		}
		protected virtual Task<List<ValidationError>> ValidateAsync(T payload)
		{
			return Task.FromResult(new List<ValidationError>());
		}
	}
}