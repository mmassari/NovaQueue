using NovaQueue.Abstractions.Models;
using System;
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
			var validation = await ValidateAsync(entry.Payload);
			if (!validation.Result)
				return validation.ErrorResult ?? throw new NullReferenceException();

			try
			{
				await JobExecuteAsync(entry.Payload);
				return new SuccessResult();
			}
			catch (Exception ex)
			{
				return new ErrorResult("An error has been thrown", new Error(ex.Message));
			}
		}
		protected void LogEvent(string message)
		{
			LogMessageReceived?.Invoke(entry, message);
		}
		protected async virtual Task<(bool Result, ValidationErrorResult ErrorResult)> ValidateAsync(T payload)
		{
			return await Task.Run<(bool Result, ValidationErrorResult ErrorResult)>(() => { return (true, new ValidationErrorResult("OK")); });
		}
	}
}