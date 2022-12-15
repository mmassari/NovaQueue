using IOptionsWriter;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NovaQueue.Abstractions;
using NovaQueue.Worker;

namespace NovaQueue.Endpoints
{
	public enum Policies
	{
		ReadQueue,
		WriteQueue,
		ReadSettings,
		WriteSettings,
		Admin
	}

	public static class EndpointsServiceConfigureExtensions
	{		
		public static IServiceCollection AddNovaQueueEndpoints<TPayload>(this IServiceCollection services, Action<SettingsOptions> options)
			where TPayload: class
		{
			var endpoints = new List<IEndpointDefinition>
			{
				new QueueEndpoints<TPayload>(),
				new DeadLetterEndpoints<TPayload>(),
				new CompletedEndpoints<TPayload>(),
				new SettingsEndpoints<TPayload>(),
				new WorkerEndpoints<TPayload>()
			};

			services.AddSingleton(endpoints as IReadOnlyCollection<IEndpointDefinition>);
			var settings = new SettingsOptions();
			options.Invoke(settings);
			services.AddOptions().ConfigureWritable<QueueOptions<TPayload>>(
					settings.SectionName, 
					settings.SettingsFile, 
					settings.ReloadAfterWrite);

			return services;
		}

		public static WebApplication UseNovaQueueEndpoints<T>(this WebApplication app)
			where T : class
		{
			var definitions = app.Services.GetRequiredService<IReadOnlyCollection<IEndpointDefinition>>();
			foreach (var def in definitions)
			{
				def.DefineEndpoints(app);
			}
			return app;
		}
	}
}