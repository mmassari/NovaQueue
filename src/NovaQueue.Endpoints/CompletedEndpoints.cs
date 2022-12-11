using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using NovaQueue.Abstractions;
using NovaQueue.Worker;

namespace NovaQueue.Endpoints
{
	public class CompletedEndpoints<TPayload> : IEndpointDefinition
		where TPayload : class
	{
		public void DefineEndpoints(WebApplication app)
		{
			//Get all entries from the active queue
			app.MapGet("api/completed", GetAll)
				.WithDisplayName("Get all completed entries")
				.WithGroupName("Completed Queue")
				.RequireAuthorization(Policies.ReadQueue.ToString());

			// Enqueue a new entry to active queue
			app.MapPost("api/completed/{id}/enqueue", Enqueue)
				.WithDisplayName("Move completed entry to active queue")
				.WithGroupName("Completed Queue")
				.RequireAuthorization(Policies.WriteQueue.ToString());

			// Delete the entry
			app.MapDelete("/api/completed/{id}", Delete)
				.WithDisplayName("Delete the entry in the completed queue")
				.WithGroupName("Comleted Queue")
				.RequireAuthorization(Policies.WriteQueue.ToString());

			// Delete the entry
			app.MapDelete("/api/completed", Clear)
				.WithDisplayName("Delete all entries in the completed queue")
				.WithGroupName("Comleted Queue")
				.RequireAuthorization(Policies.WriteQueue.ToString());
		}

		public async Task<IResult> GetAll(ITransactionalQueue<TPayload> queue)
		{
			return Results.Ok(await queue.CompletedEntriesAsync());
		}

		public async Task<IResult> Enqueue(ITransactionalQueue<TPayload> queue, string id)
		{
			var entry = await queue.GetAsync(id);			
			await queue.EnqueueAsync(entry.Payload);
			return Results.Ok();
		}
		public async Task<IResult> Delete(ITransactionalQueue<TPayload> queue, string id)
		{
			await queue.DeleteCompletedAsync(id);
			return Results.Ok();
		}
		public async Task<IResult> Clear(ITransactionalQueue<TPayload> queue)
		{
			await queue.ClearCompletedAsync();
			return Results.Ok();
		}
	}
}
