/* Copyright 2018 by Nomadeon Software LLC. Licensed uinder MIT: https://opensource.org/licenses/MIT */
using System;
using System.Collections.Generic;

namespace NovaQueue.Abstractions
{
	public class CompletedEntry<T>
	{
		public string Id { get; }
		public T Payload { get; }
		public List<AttemptLogs> Logs { get; set; } = new();
		public List<AttemptErrors> Errors { get; } = new();
		public DateTime DateCreated { get; }
		public DateTime DateCompleted { get; } = DateTime.Now;

		public CompletedEntry(QueueEntry<T> queueEntry)
		{
			Id = queueEntry.Id;
			Payload = queueEntry.Payload;
			Logs= queueEntry.Logs;
			Errors= queueEntry.Errors;
			DateCreated = queueEntry.DateCreated;
			DateCompleted = DateTime.Now;
		}
	}
}
