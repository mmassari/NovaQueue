using IOptionsWriter;
using Mapster;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NovaQueue.Abstractions;
using NovaQueue.Worker;

namespace NovaQueue.Endpoints
{
	public class SettingsEndpoints<TPayload> : IEndpointDefinition
		where TPayload : class
	{
		public void DefineEndpoints(WebApplication app)
		{
			//Get all entries from the active queue
			app.MapGet("api/settings", Get)
				.WithDisplayName("Get all queue entries")
				.WithGroupName("Active Queue")
				.RequireAuthorization(Policies.ReadQueue.ToString());

			// Enqueue a new entry to active queue
			app.MapPost("api/settings", Update)
				.WithDisplayName("Add entry to active queue")
				.WithGroupName("Active Queue")
				.RequireAuthorization(Policies.WriteQueue.ToString());
		}

		public QueueOptions<TPayload> Get(IOptions<QueueOptions<TPayload>> options)
		{
			return options.Value;
		}

		public IResult Update(IOptionsWritable<QueueOptions<TPayload>> writeOptions, QueueOptions<TPayload> newOptions)
		{
			writeOptions.Update(options =>
			{
				newOptions.Adapt(options, typeof(QueueOptions<TPayload>), typeof(QueueOptions<TPayload>));
			});
			return Results.Ok();
		}
	}
}
