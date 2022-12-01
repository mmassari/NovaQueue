/* Copyright 2018 by Nomadeon Software LLC. Licensed uinder MIT: https://opensource.org/licenses/MIT */
using System;
using System.Collections.Generic;
using System.Text;

namespace NovaQueue.Abstractions
{
	public class QueueEntry<T>
	{
		public long Id { get; set; }
		public T Payload { get; set; }
		public bool IsCheckedOut { get; set; } = false;
		public DateTime DateCreated { get; } = DateTime.Now;
		public DateTime? LastAttempt { get; set; } = null;
		public short Attempts { get; set; } = 0;
		public string Logs { get; set; }
		public List<string> Errors { get; } = new();
		public QueueEntry()
		{

		}

		public QueueEntry(T payload)
		{
			Payload = payload;
		}
	}
}
