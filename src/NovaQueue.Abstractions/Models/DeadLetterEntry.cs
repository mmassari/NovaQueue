/* Copyright 2018 by Nomadeon Software LLC. Licensed uinder MIT: https://opensource.org/licenses/MIT */
using System;
using System.Collections.Generic;

namespace NovaQueue.Abstractions;

public class DeadLetterEntry<T>
{
	public string Id { get; }
	public T Payload { get; }
	public List<AttemptLogs> Logs { get; set; }
	public List<AttemptErrors> Errors { get; }
	public DateTime DateCreated { get; }
	public DateTime DateDeadLettered { get; } = DateTime.Now;
	public DeadLetterEntry(QueueEntry<T> queueEntry)
	{
		Id = queueEntry.Id;
		Payload = queueEntry.Payload;
		Errors = queueEntry.Errors;
		Logs = queueEntry.Logs;
		DateCreated = queueEntry.DateCreated;
	}
}
