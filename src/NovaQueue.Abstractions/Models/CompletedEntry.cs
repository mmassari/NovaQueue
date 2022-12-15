/* Copyright 2018 by Nomadeon Software LLC. Licensed uinder MIT: https://opensource.org/licenses/MIT */
using System;
using System.Collections.Generic;

namespace NovaQueue.Abstractions
{
	public class CompletedEntry<T> : IEntity<T>
	{
		public string Id { get; set; }
		public T Payload { get; set; }
		public List<AttemptLogs> Logs { get; set; } = new();
		public List<AttemptErrors> Errors { get; set; } = new();
		public DateTime DateCreated { get; set; }
		public DateTime DateCompleted { get; set; } = DateTime.Now;

		public CompletedEntry()
		{
		}
		public CompletedEntry(QueueEntry<T> queueEntry)
		{
			Id = queueEntry.Id;
			Payload = queueEntry.Payload;
			Logs = queueEntry.Logs;
			Errors = queueEntry.Errors;
			DateCreated = queueEntry.DateCreated;
			DateCompleted = DateTime.Now;
		}
	}
}
