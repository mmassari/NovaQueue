using NovaQueue.Abstractions;
using NovaQueue.Abstractions.Models;

namespace SampleWebApi;
public record DelayPayload(string title, DateTime dt, int counter);

public class DelayJob : IQueueJobAsync<DelayPayload>
{
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
}
