/* Copyright 2018 by Nomadeon Software LLC. Licensed uinder MIT: https://opensource.org/licenses/MIT */
using System;
using System.Collections.Generic;

namespace NovaQueue.Abstractions;

public class DeadLetterEntry<T>
{
	public Guid Id { get; } = Guid.NewGuid();
	public long OriginalId { get; }
	public T Payload { get; }
	public IEnumerable<string> Errors { get; }
	public DateTime DateCreated { get; } = DateTime.Now;
	public DateTime? LastMailSent { get; set; } = null;

	public DeadLetterEntry(QueueEntry<T> queueEntry)
	{
		OriginalId = queueEntry.Id;
		Payload = queueEntry.Payload;
		Errors = queueEntry.Errors;
		DateCreated = queueEntry.DateCreated;
	}
}
