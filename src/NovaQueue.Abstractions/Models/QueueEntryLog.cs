using System.Collections.Generic;

namespace NovaQueue.Abstractions
{
	public class QueueEntryLog
	{
		private readonly List<string> _logs;
		public QueueEntryLog()
		{
			_logs = new List<string>();
		}
		public QueueEntryLog(string log) : this()
		{
			Add(log);
		}
		public string Logs => string.Join("\n", _logs);
		public void Add(string log) { _logs.Add(log); }
		public static QueueEntryLog Create(string log) => new QueueEntryLog(log);
		public static QueueEntryLog Empty => new QueueEntryLog();
	}

}