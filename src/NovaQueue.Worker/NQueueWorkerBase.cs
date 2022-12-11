using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NovaQueue.Abstractions;

namespace NovaQueue.Worker
{
	public abstract class NQueueWorkerBase<TPayload> : IQueueWorker<TPayload> where TPayload : class
	{
		protected CancellationTokenSource? _cancellationToken;
		protected readonly ILogger<NQueueWorkerBase<TPayload>> _logger;
		protected readonly ITransactionalQueue<TPayload> _queue;
		protected readonly QueueOptions<TPayload> _options;

		protected readonly List<Task> _tasks;
		protected Task? _mainLoopTask;
		public NQueueWorkerBase(
			ILogger<NQueueWorkerBase<TPayload>> logger,
			ITransactionalQueue<TPayload> queue,
			IOptions<QueueOptions<TPayload>> options)
		{
			_logger = logger;
			_queue = queue;
			_options = options.Value;
			_cancellationToken = new CancellationTokenSource();
			_tasks = new List<Task>();
		}
		protected abstract Task RunJob(QueueEntry<TPayload> item);
		public void Enqueue(TPayload payload) => _queue.Enqueue(payload);
		public async Task EnqueueAsync(TPayload payload) => await Task.Run(() => _queue.Enqueue(payload));

		public Task StartAsync(CancellationToken cancellationToken)
		{
			//Loop infinito in cui leggo la coda ed elaboro i job
			_mainLoopTask = QueueLoop(_cancellationToken!.Token);
			return Task.CompletedTask;
		}
		private async Task QueueLoop(CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				try
				{
					var items = _queue.Dequeue(_options.MaxConcurrent);
					if (items is null || items.Count == 0)
					{
						await Task.Delay(1000);
						continue;
					}

					//Avvio in thread paralleli tutti i job ed attendo
					await items.ForEachAsync(item =>
						_tasks.AddAsync(RunJob(item))
					);

					await Task.Delay(1000);
				}
				catch (OperationCanceledException)
				{
					break;
				}
				catch (Exception ex)
				{
					Console.WriteLine("Errore:" + ex.Message);
				}
			}
		}



		public async Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation($"Stop Queue {_options.Name}");
			DestroyQueue();
			await Task.WhenAny(_mainLoopTask!, Task.Delay(Timeout.Infinite, cancellationToken));
		}

		public void Dispose()
		{
			DestroyQueue();
		}

		private void DestroyQueue()
		{
			try
			{
				if (_cancellationToken != null)
				{
					_cancellationToken.Cancel();
					_cancellationToken = null;
				}
			}
			catch { }
		}

		public Task<WorkerStatus> GetStatusAsync()
		{
			throw new NotImplementedException();
		}

		public Task StopAsync()
		{
			throw new NotImplementedException();
		}

		public Task StartAsync()
		{
			throw new NotImplementedException();
		}
	}

}