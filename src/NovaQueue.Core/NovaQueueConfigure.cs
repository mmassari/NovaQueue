using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NovaQueue.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace NovaQueue.Core
{
	public static class NovaQueueConfigure
	{
		public static IServiceCollection AddNovaQueue<TPayload>(this IServiceCollection services)
		{
			//Configuro le SatispayOptions
			services.AddOptions<QueueOptions<TPayload>>()
				.Configure(options =>
				{
					options.Name = "NovaQueue";
					options.ResetOrphansOnStartup = true;
					options.MaxConcurrent = 1;
					options.OnFailure = OnFailurePolicy.Discard;
					options.DeadLetter.IsEnabled= false;
					options.MaxAttempts = 1;
				});
			return services;
		}

		//public static IServiceCollection AddNovaQueue<TPayload>(this IServiceCollection services, IConfiguration section)
		//{
		//	services.Configure().AddOptions<QueueOptions<TPayload>>(section);
		//	return services;


		//}
		public static IServiceCollection AddNovaQueue<TPayload>(this IServiceCollection services, Action<QueueOptions<TPayload>> opt)
		{
			//Configuro le SatispayOptions
			services.Configure(opt);

			return services;
		}
	}

}
