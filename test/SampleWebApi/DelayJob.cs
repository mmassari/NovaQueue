using NovaQueue.Abstractions;
using NovaQueue.Abstractions.Models;
using System;
using System.Threading.Tasks;

namespace SampleWebApi;
public class DelayPayload { 
	public string title { get; set; }
	public DateTime dt { get; set; }
	public int counter { get; set; }

}
public class DelayJob : IQueueJobAsync<DelayPayload>
{
	public event JobLogEventHandler<DelayPayload> LogMessageReceived;
	public async Task<Result<QueueEntryLog>> RunWorkerAsync(DelayPayload payload)
	{
		Console.WriteLine($"RunWorkerAsync - Payload: {payload}");
		await Task.Delay(3000);
		if (payload.title == "Daniele")
		{
			return new ErrorResult<QueueEntryLog>("Impossibile aspettare Daniele");
		}
		return new SuccessResult<QueueEntryLog>(QueueEntryLog.Empty);
	}

	public Task<Result> RunWorkerAsync(QueueEntry<DelayPayload> payload)
	{
		throw new NotImplementedException();
	}
}
