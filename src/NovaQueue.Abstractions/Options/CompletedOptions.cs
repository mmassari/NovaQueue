using System;

namespace NovaQueue.Abstractions
{
	public class CompletedOptions
	{
		public bool IsEnabled { get; set; } = false;
		public TimeSpan DeleteAfter { get; set; }
	}
}
