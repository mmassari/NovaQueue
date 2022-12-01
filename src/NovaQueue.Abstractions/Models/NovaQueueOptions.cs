using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NovaQueue.Abstractions
{
	public class NovaQueueOptions<TPayload>
	{
		public string Name { get; set; }
		public short MaxAttempts { get; set; } = 0;
		public DeadLetterOptions DeadLetter { get; set; } = new();
		public CompletedOptions Completed { get; set; } = new();
		public OnFailurePolicy OnFailure { get; set; } = OnFailurePolicy.Retry;
		public int MaxConcurrent { get; set; } = 1;
		public bool ResetOrphansOnStartup { get; set; } = true;
	}
	public class CompletedOptions
	{
		public bool IsEnabled { get; set; } = false;
		public TimeSpan DeleteAfter { get; set; }
	}
	public class DeadLetterOptions
	{
		public bool IsEnabled { get; set; } = false;
		public List<string> AlertMailRecipients { get; set; } = new();
		public TimeSpan AlertCheckEvery { get; set; } = TimeSpan.Zero;
	}
}
