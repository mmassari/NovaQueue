using NovaQueue.Abstractions.Models;
using System;

namespace NovaQueue.Abstractions
{
	public abstract class QueueJobBase<T> : IQueueJob<T>
	{
		protected QueueEntryLog logs { get; }
		public QueueJobBase()
		{
			logs = new QueueEntryLog();
		}
		protected abstract void JobExecute(T payload);
		public virtual Result<QueueEntryLog> RunWorker(T payload)
		{
			var validation = Validate(payload);
			if (!validation.Result)
				return new ValidationErrorResult<QueueEntryLog>("Validation Errors", validation.Errors);

			try
			{
				JobExecute(payload);
				return new SuccessResult<QueueEntryLog>(logs);
			}
			catch (Exception ex)
			{
				return new ErrorResult<QueueEntryLog>(
					"An error has been thrown",
					new Error(ex.Message))
				{
					Data = logs
				};
			}
		}
		protected void LogEvent(string message)
		{
			logs.Add(message);
		}
		protected virtual (bool Result, ValidationError[] Errors) Validate(T payload)
		{
			return (true, null!);
		}
	}

}