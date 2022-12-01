using NovaQueue.Worker;
using NovaQueue.Persistence.LiteDB;
using TestApi;
using NovaQueue.Abstractions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.UseLiteDB(builder.Configuration.GetConnectionString("Default"));
builder.Services.AddNovaQueueWorker<DelayPayload, DelayJob>(
	builder.Configuration.GetSection("NovaQueue:DelayWorker"));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
	"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/queue/completed", (ITransactionalQueue<DelayPayload> queue) =>
{
	Results.Ok(queue.CompletedEntries());
});
app.MapGet("/queue/deadletter", (ITransactionalQueue<DelayPayload> queue) =>
{
	Results.Ok(queue.DeadLetterEntries());
});
app.MapGet("/queue/entries", (ITransactionalQueue<DelayPayload> queue) =>
{
	Results.Ok(queue.QueuedEntries());
});
app.MapPost("/queue", async (
	IQueueWorker<DelayPayload> worker,
	DelayPayload payload) =>
{
	await worker.EnqueueAsync(payload);
	return Results.Ok();
}).WithName("EnqueuePayload");

app.MapGet("/weatherforecast", () =>
{
	var forecast = Enumerable.Range(1, 5).Select(index =>
		new WeatherForecast
		(
			DateTime.Now.AddDays(index),
			Random.Shared.Next(-20, 55),
			summaries[Random.Shared.Next(summaries.Length)]
		))
		.ToArray();
	return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

internal record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
	public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}