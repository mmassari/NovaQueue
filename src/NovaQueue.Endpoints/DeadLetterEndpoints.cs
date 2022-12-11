using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using NovaQueue.Abstractions;
using NovaQueue.Worker;

namespace NovaQueue.Endpoints
{
	public class DeadLetterEndpoints<TPayload> : IEndpointDefinition
		where TPayload : class
	{
		public void DefineEndpoints(WebApplication app)
		{
			//Get all entries from the active queue
			app.MapGet("api/deadletter", GetAll)
				.WithDisplayName("Get all deadletter entries")
				.WithGroupName("DeadLetter Queue")
				.RequireAuthorization(Policies.ReadQueue.ToString());

			// Enqueue a new entry to active queue
			app.MapPost("api/deadletter/enqueue", EnqueueAll)
				.WithDisplayName("Enqueue all deadletter entries")
				.WithGroupName("DeadLetter Queue")
				.RequireAuthorization(Policies.WriteQueue.ToString());

			// Move up the entry in the active queue
			app.MapPost("/api/deadletter/{id}/enqueue", Enqueue)
				.WithDisplayName("Equeue the deadletter entry")
				.WithGroupName("DeadLetter Queue")
				.RequireAuthorization(Policies.WriteQueue.ToString());

			// Delete the DeadLetter entry
			app.MapDelete("/api/deadletter/{id}", Delete)
				.WithDisplayName("Delete the entry in the deadletter queue")
				.WithGroupName("DeadLetter Queue")
				.RequireAuthorization(Policies.WriteQueue.ToString());

			// Clear the DeadLetter queue
			app.MapDelete("/api/deadletter/{id}", Clear)
				.WithDisplayName("Clear the deadletter queue")
				.WithGroupName("DeadLetter Queue")
				.RequireAuthorization(Policies.WriteQueue.ToString());
		}

		public async Task<IEnumerable<DeadLetterEntry<TPayload>>> GetAll(ITransactionalQueue<TPayload> queue)
		{
			return await queue.DeadLetterEntriesAsync();
		}

		public async Task<IResult> EnqueueAll(ITransactionalQueue<TPayload> queue)
		{
			var entries = await queue.DeadLetterEntriesAsync();
			await queue.EnqueueAsync(entries.Select(c=>c.Payload));
			return Results.Ok();
		}
		public async Task<IResult> Enqueue(ITransactionalQueue<TPayload> queue, string id)
		{
			var entry = await queue.GetAsync(id);
			if (entry == null)
				return Results.NotFound();

			await queue.EnqueueAsync(entry.Payload);
			return Results.Ok();
		}
		public async Task<IResult> Delete(ITransactionalQueue<TPayload> queue, string id)
		{
			await queue.DeleteDeadLetterAsync(id);
			return Results.Ok();
		}
		public async Task<IResult> Clear(ITransactionalQueue<TPayload> queue)
		{
			await queue.ClearDeadLetterAsync();
			return Results.Ok();
		}

	}
}
