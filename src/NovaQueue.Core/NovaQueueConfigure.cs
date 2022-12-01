using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace NovaQueueLib
{
	public static class NovaQueueConfigure
	{
		public static IServiceCollection AddNovaQueue(this IServiceCollection services)
		{
			//Configuro le SatispayOptions
			services.AddOptions<NovaQueueOptions>()
				.Configure(options =>
				{
					options.MaxConcurrent = 1;
					options.Transactional = false;
					options.OnFailure = OnFailurePolicy.Discard;
					options.DeadLetter.IsEnabled= false;
					options.MaxAttempts = 1;
				});

			RegisterServices(ref services);
			return services;
		}

		public static IServiceCollection AddNovaQueue(this IServiceCollection services, IConfiguration section)
		{
			services.Configure<NovaQueueOptions>(section);
			RegisterServices(ref services);
			return services;


		}
		public static IServiceCollection AddNovaQueue(this IServiceCollection services, Action<KlarnaOptions> opt)
		{
			//Configuro le SatispayOptions
			services.Configure(opt);

			RegisterMappings();
			RegisterServices(ref services);
			return services;
		}
		public static IServiceCollection AddNovaQueue(this IServiceCollection services, KlarnaOptions opt)
		{
			//Configuro le SatispayOptions
			services.AddOptions<KlarnaOptions>()
				.Configure(options =>
				{
					options.BaseUrls = opt.BaseUrls;
					options.WebhookUrls = opt.WebhookUrls;
					options.WebhookAddSessionId = opt.WebhookAddSessionId;
					options.TimeoutMillis = opt.TimeoutMillis;
					options.Env = opt.Env;
				});

			RegisterMappings();
			RegisterServices(ref services);
			return services;
		}
		/// <summary>
		/// Configura i servizi di Klarna
		/// </summary>
		/// <param name="services"></param>
		private static void RegisterServices(ref IServiceCollection services)
		{
			services.AddScoped<IKlarnaService, KlarnaService>();
			services.AddScoped<IKlarnaShopDataStore, KlarnaShopDataStore>();
			services.AddScoped<IKlarnaPaymentDataStore, KlarnaPaymentDataStore>();
			services.AddScoped<IOperatorDataStore, OperatorDataStore>();
			services.AddScoped<IValidator<KlarnaTicket>, KlarnaTicketValidator>();
			services.AddScoped<IValidator<KlarnaRefundTicket>, KlarnaRefundTicketValidator>();
			services.AddScoped<IValidator<ChangeStatusPayload>, ChangeStatusPayloadValidator>();
			services.AddHttpClient<IKlarnaGateway, KlarnaGateway>((provider, httpClient) =>
			{
				var o = provider.GetRequiredService<IOptions<KlarnaOptions>>();
				httpClient.Timeout = o.Value.Timeout;
				httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				httpClient.DefaultRequestHeaders.Add("User-Agent", "TST.TIE.KlarnaGateway");

			}).AddPolicyHandler(new ClientPolicy().HttpRetryPolicy);

		}
	}

}
