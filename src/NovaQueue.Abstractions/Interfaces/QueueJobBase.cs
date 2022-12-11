using NovaQueue.Abstractions.Models;
using System;

namespace NovaQueue.Abstractions
{
	public delegate void JobLogEventHandler<T>(QueueEntry<T> entry, string log);
	public abstract class QueueJobBase<T> : IQueueJob<T>
	{
		protected QueueEntry<T> entry;
		public event JobLogEventHandler<T> LogMessageReceived;
		protected abstract void JobExecute(T payload);
		public virtual Result RunWorker(QueueEntry<T> entry)
		{
			this.entry = entry;
			var validation = Validate(entry.Payload);
			if (!validation.Result)
				return new ValidationErrorResult("Validation Errors", validation.Errors);

			try
			{
				JobExecute(entry.Payload);
				return new SuccessResult();
			}
			catch (Exception ex)
			{
				return new ErrorResult(
					"An error has been thrown",
					new Error(ex.Message));
			}
		}
		protected void LogEvent(string message)
		{
			Console.WriteLine(message);
			LogMessageReceived?.Invoke(entry, message);
		}
		protected virtual (bool Result, ValidationError[] Errors) Validate(T payload)
		{
			return (true, null!);
		}
	}

}