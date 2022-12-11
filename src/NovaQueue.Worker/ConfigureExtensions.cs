using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NovaQueue.Abstractions;
using NovaQueue.Core;

namespace NovaQueue.Worker
{
	public static class ConfigureExtensions
	{
		public static IServiceCollection AddNovaQueueWorker<TPayload, TJob>(this IServiceCollection services, IConfiguration configuration)
			where TPayload : class
			where TJob : class, IQueueJob<TPayload>
		{
			//Leggo le configurazioni della coda dalle settings e creo l'oggetto
			services.Configure<QueueOptions<TPayload>>(configuration);
			//Leggo le configurazioni della coda dalle settings e creo l'oggetto
			services.AddSingleton<ITransactionalQueue<TPayload>, NQueue<TPayload>>();
			//Inietto il worker specifico
			services.AddTransient<IQueueJob<TPayload>, TJob>();
			//Creo il BackgroundWorker per l'elaborazione della coda
			services.AddSingleton<IQueueWorker<TPayload>, NQueueWorker<TPayload>>();
			//Registro il servizio in background
			services.AddHostedService(provider => provider.GetService<IQueueWorker<TPayload>>()!);
			return services;
		}
		public static IServiceCollection AddNovaQueueWorkerAsync<TPayload, TJob>(this IServiceCollection services, IConfiguration configuration)
			where TPayload : class
			where TJob : class, IQueueJobAsync<TPayload>
		{
			//Leggo le configurazioni della coda dalle settings e creo l'oggetto
			services.Configure<QueueOptions<TPayload>>(configuration);
			//Leggo le configurazioni della coda dalle settings e creo l'oggetto
			services.AddSingleton<ITransactionalQueue<TPayload>, NQueue<TPayload>>();
			//Inietto il worker specifico
			services.AddTransient<IQueueJobAsync<TPayload>, TJob>();
			//Creo il BackgroundWorker per l'elaborazione della coda
			services.AddSingleton<IQueueWorker<TPayload>, NQueueWorker<TPayload>>();
			//Registro il servizio in background
			services.AddHostedService(provider => provider.GetService<IQueueWorker<TPayload>>()!);
			return services;
		}
	}
}
