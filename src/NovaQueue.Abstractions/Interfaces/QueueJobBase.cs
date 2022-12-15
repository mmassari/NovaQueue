using NovaQueue.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;

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
			var errors = Validate(entry.Payload);
			if (errors != null)
				return new ValidationErrorResult("Validation Errors", errors.ToArray());

			try
			{
				JobExecute(entry.Payload);
				return new SuccessResult();
			}
			catch (Exception ex)
			{
				return new ErrorResult("An error occurred while executing job", new Error(ex));
			}
		}
		protected virtual void LogEvent(string message)
		{
			Console.WriteLine(message);
			LogMessageReceived?.Invoke(entry, message);
		}
		protected virtual IEnumerable<ValidationError> Validate(T payload)
		{
			return null;
		}
	}

}