using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using NovaQueue.Abstractions;
using NovaQueue.Worker;

namespace NovaQueue.Endpoints
{
	public interface IEndpointDefinition
	{
		void DefineEndpoints(WebApplication app);
	}

	public class QueueEndpoints<TPayload> : IEndpointDefinition
		where TPayload : class
	{
		public void DefineEndpoints(WebApplication app)
		{
			//Get all entries from the active queue
			app.MapGet("api/queue", GetAll)
				.WithDisplayName("Get all queue entries")
				.WithGroupName("Active Queue");
				//.RequireAuthorization(Policies.ReadQueue.ToString());

			// Enqueue a new entry to active queue
			app.MapPost("api/queue", Add)
				.WithDisplayName("Add entry to active queue")
				.WithGroupName("Active Queue");

			// Move up the entry in the active queue
			app.MapPost("/api/queue/{id}/up", MoveUp)
				.WithDisplayName("Move up the entry in the queue")
				.WithGroupName("Active Queue");

			// Run the entry 
			app.MapPost("/api/queue/{id}/run", Run)
				.WithDisplayName("Run the entry now")
				.WithGroupName("Active Queue");

			// Delete the entry
			app.MapDelete("/api/queue/{id}", Delete)
				.WithDisplayName("Delete the entry in the queue")
				.WithGroupName("Active Queue");
		}

		internal async Task<IResult> GetAll(ITransactionalQueue<TPayload> queue)
		{
			return Results.Ok(await queue.QueuedEntriesAsync());
		}

		internal async Task<IResult> Add(TPayload payload, ITransactionalQueue<TPayload> queue)
		{
			await queue.EnqueueAsync(payload);
			return Results.Ok();
		}
		internal async Task<IResult> MoveUp(string id, ITransactionalQueue<TPayload> queue)
		{
			var entry = await queue.GetAsync(id);
			await queue.MoveUpAsync(entry);
			return Results.Ok();
		}
		internal async Task<IResult> Run(string id, ITransactionalQueue<TPayload> queue, IQueueWorker<TPayload> worker)
		{
			var entry = await queue.DequeueAsync();
			await worker.RunAsync(entry);
			return Results.Ok();
		}
		internal async Task<IResult> Delete(string id, ITransactionalQueue<TPayload> queue)
		{
			await queue.DeleteAsync(id);
			return Results.Ok();
		}
	}
}
