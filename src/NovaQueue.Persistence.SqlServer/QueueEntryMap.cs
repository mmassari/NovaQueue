using Dapper.FluentMap.Dommel.Mapping;
using NovaQueue.Abstractions;

namespace NovaQueue.Persistence.SqlServer
{
	public class QueueEntryMap: DommelEntityMap<QueueEntry<object>>
	{
		private string _tableName;

		public QueueEntryMap(string tableName)
		{
			_tableName = tableName;
			ToTable(tableName);
			Map(p => p.Id).IsIdentity().IsKey();
			Map(p => p.Payload);
		}

		public string GetExistsSql()
		{
			return $"EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{_tableName}')";
		}
		public string GetCreateTableSql()
		{
			return $"CREATE TABLE {_tableName}" +
					  "ADD COLUMN Id int IDENTITY(1,1) NOT NULL,";
		}
	}
	public class DeadLetterEntryMap : DommelEntityMap<DeadLetterEntry<object>>
	{
		public DeadLetterEntryMap(string tableName)
		{
			ToTable(tableName);
			Map(p => p.Id).IsKey();
			Map(p => p.Payload);


		}
	}
	public class CompletedEntryMap : DommelEntityMap<CompletedEntry<object>>
	{
		public CompletedEntryMap(string tableName)
		{
			ToTable(tableName);
			Map(p => p.Id).IsIdentity().IsKey();
			Map(p => p.Payload);


		}
	}
}
