using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NovaQueue.Abstractions;
using NovaQueue.Core;
using NovaQueue.Persistence.LiteDB;
using System;
using System.Collections.Generic;

namespace SampleConsole
{
	class Program
	{
		static void Main(string[] args)
		{
			var options = new QueueOptions<string>()
			{
				Name="logs",
				MaxAttempts = 5,
				MaxConcurrent = 1,
				OnFailure = OnFailurePolicy.MoveLast,
				ResetOrphansOnStartup = true,
				Completed = new CompletedOptions
				{
					IsEnabled = true,
					DeleteAfter = TimeSpan.FromDays(2),
				},
				DeadLetter = new DeadLetterOptions
				{
					IsEnabled = true,
					AlertMailRecipients = new List<string> { "mmassari@titantex.sm" },
					AlertCheckEvery = TimeSpan.FromMinutes(2),
				}
			};

			// NovaQueue depends on LiteDB. You can save other things to same database.
			using (var db = new LiteDBRepository("Queue.db"))
			{
				// Creates a "logs" collection in LiteDB. You can also pass a user defined object.
				var logs = new NQueue<string>(db, options.ToIOptions());

				// Recommended on startup to reset anything that was checked out but not committed or aborted. 
				// Or call CurrentCheckouts to inspect them and abort yourself. See github page for
				// notes regarding duplicate messages.
				logs.ResetOrphans();

				// Adds record to queue
				logs.Enqueue("Test");

				// Get next item from queue. Marks it as checked out such that other threads that 
				// call Checkout will not see it - but does not remove it from the queue.
				var record = logs.Dequeue();

				try
				{
					// Do something that may potentially fail, i.e. a network call
					// ...

					// Removes record from queue
					logs.Commit(record);
				}
				catch
				{
					// Returns the record to the queue
					logs.Abort(record);
				}
			}

			Console.WriteLine("Done");
			Console.ReadLine();
		}
	}
}