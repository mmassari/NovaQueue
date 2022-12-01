using NovaQueue.Worker;
using SampleWebApi;
using NovaQueue.Persistence.LiteDB;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.UseLiteDB(builder.Configuration.GetConnectionString("Default"));
builder.Services.AddNovaQueueWorkerAsync<DelayPayload, DelayJob>(
	builder.Configuration.GetSection("NovaQueue:DelayWorker"));

//builder.Services.AddNovaQueueWorker<MailPayload, MailerJob>(
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