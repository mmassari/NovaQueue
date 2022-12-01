/* Copyright 2018 by Nomadeon Software LLC. Licensed uinder MIT: https://opensource.org/licenses/MIT */
using System;

namespace NovaQueue.Abstractions
{
	public class CompletedEntry<T>
	{
		public Guid Id { get; } = Guid.NewGuid();
		public long OriginalId { get; }
		public T Payload { get; }
		public string Logs { get; }
		public DateTime DateCreated { get; }
		public DateTime DateCompleted { get; } = DateTime.Now;
		public DateTime? LastMailSent { get; set; } = null;

		public CompletedEntry(QueueEntry<T> queueEntry)
		{
			OriginalId = queueEntry.Id;
			Payload = queueEntry.Payload;

			Logs = queueEntry.Errors.Count > 0 ? "Errors:\n" + String.Join("\n", queueEntry.Errors) : "";
			if (!string.IsNullOrWhiteSpace(queueEntry.Logs))
			{
				if (Logs.Length > 0) Logs += "\n";
				Logs += $"Logs:\n{queueEntry.Logs}";
			}
				
			DateCreated = queueEntry.DateCreated;
			DateCompleted = DateTime.Now;
		}
	}
}
