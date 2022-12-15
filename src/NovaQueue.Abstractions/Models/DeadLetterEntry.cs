/* Copyright 2018 by Nomadeon Software LLC. Licensed uinder MIT: https://opensource.org/licenses/MIT */
using System;
using System.Collections.Generic;

namespace NovaQueue.Abstractions;

public class DeadLetterEntry<T> : IEntity<T>
{
	public string Id { get; set; }
	public T Payload { get; set; }
	public List<AttemptLogs> Logs { get; set; }
	public List<AttemptErrors> Errors { get; set; }
	public DateTime DateCreated { get; set; }
	public DateTime DateDeadLettered { get; set; } = DateTime.Now;
	public DeadLetterEntry() { }
	public DeadLetterEntry(QueueEntry<T> queueEntry)
	{
		Id = queueEntry.Id;
		Payload = queueEntry.Payload;
		Errors = queueEntry.Errors;
		Logs = queueEntry.Logs;
		DateCreated = queueEntry.DateCreated;
	}
}
