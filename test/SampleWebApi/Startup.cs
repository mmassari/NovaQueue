using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NovaQueue.Worker;
using NovaQueue.Persistence.LiteDB;
using SampleWebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication31
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddLiteDbPersistence<DelayPayload>(Configuration.GetConnectionString("Default"));
			services.AddNovaQueueWorkerAsync<DelayPayload, DelayJob>(Configuration.GetSection("NovaQueue:DelayWorker"));
			services.AddControllers();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}



/*
 

using NovaQueue.Worker;
using SampleWebApi;
using NovaQueue.Persistence.LiteDB;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddLiteDbPersistence<DelayPayload>(builder.Configuration.GetConnectionString("Default"));
builder.Services.AddNovaQueueWorkerAsync<DelayPayload, DelayJob>(
	builder.Configuration.GetSection("NovaQueue:DelayWorker"));
//	builder.Configuration.GetSection("NovaQueue:MailWorker"));

builder.Services.AddHostedService(provider => provider.GetService<IQueueWorker<DelayPayload>>()!);
//builder.Services.AddHostedService(provider => provider.GetService<IQueueWorker<MailPayload>>());

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGet("/", () => Results.Ok);
//app.MapPost("/queue", async (
//	[FromServices]IQueueWorker<DelayPayload> worker,
//	[FromBody]DelayPayload payload) =>
//{
//	await worker.EnqueueAsync(payload);
//	return Results.Ok();
//}).WithName("Enqueue Payload");

app.Run();
 
 */