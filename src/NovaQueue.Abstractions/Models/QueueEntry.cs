/* Copyright 2018 by Nomadeon Software LLC. Licensed uinder MIT: https://opensource.org/licenses/MIT */
using NovaQueue.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NovaQueue.Abstractions
{
	public enum ErrorType
	{
		Unhandled,
		Validation,
		JobExecution,
	}
	public class AttemptLogs
	{
		public int Attempt { get; set; }
		public string Log { get; set; }
	}
	public class AttemptErrors
	{
		public int Attempt { get; set; }
		public ErrorType Type { get; set; }
		public Error[] Errors { get; set; }
	}
	public interface IEntity<TPayload>
	{
		string Id { get; set; }
		TPayload Payload { get; set; }
	}
	public class QueueEntry<T> : IEntity<T>
	{
		public string Id { get; set; } = Guid.NewGuid().ToString();
		public int Sort { get; set; } = 0;
		public T Payload { get; set; }
		public bool IsCheckedOut { get; set; } = false;
		public DateTime DateCreated { get; } = DateTime.Now;
		public DateTime? LastAttempt { get; set; } = null;
		public short Attempts { get; set; } = 0;
		public List<AttemptLogs> Logs { get; set; } = new();
		public List<AttemptErrors> Errors { get; } = new();
		public QueueEntry()
		{

		}

		public QueueEntry(T payload)
		{
			Payload = payload;
		}
	}
}
