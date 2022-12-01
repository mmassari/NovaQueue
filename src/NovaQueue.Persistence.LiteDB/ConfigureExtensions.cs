using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NovaQueue.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace NovaQueue.Persistence.LiteDB
{
	public static class ConfigureExtensions
	{
		public static IServiceCollection UseLiteDB(this IServiceCollection services, string connectionString)
		{

			services.AddSingleton<IQueueRepository, LiteDBRepository>((provider) =>
			{
				return new LiteDBRepository(connectionString);
			});
			return services;
		}

	}
}
