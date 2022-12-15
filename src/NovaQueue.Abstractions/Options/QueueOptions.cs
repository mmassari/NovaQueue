using System;
using System.IO;
using System.Text;

namespace NovaQueue.Abstractions
{
	public class QueueOptions<TPayload>
	{
		public string Name { get; set; }
		public short MaxAttempts { get; set; } = 0;
		public DeadLetterOptions DeadLetter { get; set; } = new();
		public CompletedOptions Completed { get; set; } = new();
		public OnFailurePolicy OnFailure { get; set; } = OnFailurePolicy.Retry;
		public TimeSpan WaitOnRetry { get; set; } = TimeSpan.Zero;
		public int MaxConcurrent { get; set; } = 1;
		public bool ResetOrphansOnStartup { get; set; } = true;
	}
}
