using NovaQueue.Worker;
using NovaQueue.Persistence.LiteDB;
using TestApi;
using NovaQueue.Abstractions;
using Microsoft.Extensions.Options;
using NovaQueue.Endpoints;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddLiteDbPersistence<DelayPayload>(builder.Configuration.GetConnectionString("Default"));
builder.Services.AddNovaQueueWorker<DelayPayload, DelayJob>(builder.Configuration.GetSection("NovaQueue:DelayWorker"));
builder.Services.AddNovaQueueEndpoints<DelayPayload>(options =>
{
	options.SectionName = "NovaQueue:DelayWorker";
	options.SettingsFile = "";
	options.ReloadAfterWrite = true;
});

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

app.UseNovaQueueEndpoints<DelayPayload>();

app.MapGet("/fillqueue", ([FromServices]IQueueWorker<DelayPayload> worker) =>
{
	var gen = new Random();
	DateTime start = new DateTime(2020, 1, 1);
	int range = (DateTime.Today - start).Days;

	for (var x = 0; x < 100; x++)
	{
		var dt = start.AddDays(gen.Next(range));
		worker.Enqueue(new DelayPayload($"Job {x}", dt, x));
	}
})
.WithName("FillQueue");	

app.Run();

internal record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
	public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}