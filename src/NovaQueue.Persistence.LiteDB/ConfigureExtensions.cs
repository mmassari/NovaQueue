using Microsoft.Extensions.DependencyInjection;
using NovaQueue.Abstractions;
using NovaQueue.Persistence.LiteDB.Repositories;

namespace NovaQueue.Persistence.LiteDB
{
	public static class ConfigureExtensions
	{
		public static IServiceCollection AddLiteDbPersistence<TPayload>(this IServiceCollection services, string connectionString)
			where TPayload : class
		{
			services.AddOptions<PersistenceOptions>().Configure(opt =>
			{
				opt.ConnectionString = connectionString;
			});
			services.AddSingleton<IDatabaseContext<TPayload>, LiteDBContext<TPayload>>();
			services.AddSingleton<IQueueRepository<TPayload>, LiteDbQueueRepository<TPayload>>();
			services.AddSingleton<IDeadLetterRepository<TPayload>, LiteDbDeadLetterRepository<TPayload>>();
			services.AddSingleton<ICompletedRepository<TPayload>, LiteDbCompletedRepository<TPayload>>();
			return services;
		}

	}
}
