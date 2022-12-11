using System;
using System.Collections.Generic;

namespace NovaQueue.Abstractions
{
	public class DeadLetterOptions
	{
		public bool IsEnabled { get; set; } = false;
		public List<string> AlertMailRecipients { get; set; } = new();
		public TimeSpan AlertCheckEvery { get; set; } = TimeSpan.Zero;
	}
}
