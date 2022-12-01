using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NovaQueue.Abstractions
{
	public enum PersistenceType
	{
		None,
		Sqlite,
		LiteDB,
		SqlServer
	}
	public class NovaQueuePersistenceOptions
	{
		public PersistenceType Type { get; set; } = PersistenceType.None;
		public string ConnectionString { get; set; } = "Filename=novaqueue.db;connection=direct";
	}
}
