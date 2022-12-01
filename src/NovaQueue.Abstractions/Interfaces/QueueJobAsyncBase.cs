using NovaQueue.Abstractions.Models;
using System;
using System.Threading.Tasks;

namespace NovaQueue.Abstractions
{
	public abstract class QueueJobAsyncBase<T> : IQueueJobAsync<T>
	{
		public event EventHandler<string>? MessageReceived;
		protected abstract Task JobExecuteAsync(T payload);
		public async virtual Task<Result> RunWorkerAsync(T payload)
		{
			var validation = await ValidateAsync(payload);
			if (!validation.Result)
				return validation.ErrorResult ?? throw new NullReferenceException();

			try
			{
				await JobExecuteAsync(payload);
				return new SuccessResult();
			}
			catch (Exception ex)
			{
				return new ErrorResult("An error has been thrown", new Error(ex.Message));
			}
		}
		protected void LogEvent(string message)
		{
			MessageReceived?.Invoke(this, message);
		}
		protected async virtual Task<(bool Result, ValidationErrorResult ErrorResult)> ValidateAsync(T payload)
		{
			return await Task.Run<(bool Result, ValidationErrorResult ErrorResult)>(() => { return (true, new ValidationErrorResult("OK")); });
		}
	}

}