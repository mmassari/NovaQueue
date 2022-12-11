using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using NovaQueue.Abstractions;
using NovaQueue.Worker;

namespace NovaQueue.Endpoints
{
	public class WorkerEndpoints<TPayload> : IEndpointDefinition
		where TPayload : class
	{
		public void DefineEndpoints(WebApplication app)
		{
			app.MapGet("/api/worker/status", GetStatus)
				.WithDisplayName("Get queue worker status")
				.WithGroupName("Worker service")
				.RequireAuthorization(Policies.Admin.ToString());

			app.MapPost("/api/worker/start", Start)
				.WithDisplayName("Start queue worker")
				.WithGroupName("Worker service")
				.RequireAuthorization(Policies.Admin.ToString());

			app.MapPost("/api/worker/stop", Stop)
				.WithDisplayName("Stop queue worker")
				.WithGroupName("Worker service")
				.RequireAuthorization(Policies.Admin.ToString());
		}

		internal async Task<IResult> GetStatus(IQueueWorker<TPayload> worker)
		{
			return Results.Ok(await worker.GetStatusAsync());
		}

		internal async Task<IResult> Stop(IQueueWorker<TPayload> worker)
		{
			await worker.StopAsync();
			return Results.Ok();
		}
		internal async Task<IResult> Start(IQueueWorker<TPayload> worker)
		{
			await worker.StartAsync();
			return Results.Ok();
		}
	}
}