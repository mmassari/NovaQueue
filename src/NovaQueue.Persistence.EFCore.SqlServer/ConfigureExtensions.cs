using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NovaQueue.Abstractions;

namespace NovaQueue.Persistence.EFCore.SqlServer
{
	public static class ConfigureExtensions
	{
		public static IServiceCollection AddSqlServerPersistence<TPayload>(this IServiceCollection services, string connectionString)
			where TPayload : class
		{
			services.AddOptions<PersistenceOptions>().Configure(opt =>
			{
				opt.ConnectionString = connectionString;
			});
			services.AddDbContextFactory<NovaQueueDbContext<TPayload>>();

			services.AddSingleton<IDatabaseContext<TPayload>, SqlServerContext<TPayload>>();
			services.AddSingleton<IQueueRepository<TPayload>, SqlServerQueueRepository<TPayload>>();
			services.AddSingleton<IDeadLetterRepository<TPayload>, SqlServerDeadLetterRepository<TPayload>>();
			services.AddSingleton<ICompletedRepository<TPayload>, SqlServerCompletedRepository<TPayload>>();
			services.AddSingleton<IUnitOfWork<TPayload>, SqlServerUnitOfWork<TPayload>>();
			services.AddSingleton<UpdateSortOnQueueInsertInterceptor<TPayload>>();
			services.AddSingleton<DbInitializer<TPayload>>();
			var serviceProvider = services.BuildServiceProvider();
			using (var scope = serviceProvider.CreateScope())
			{
				var provider = scope.ServiceProvider;

				var initialiser = provider.GetRequiredService<DbInitializer<TPayload>>();

				initialiser.Run();

			}
			return services;

		}
	}
}
