using Microsoft.EntityFrameworkCore;
using NovaQueue.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace NovaQueue.Persistence.EFCore.SqlServer
{
	public abstract class SqlServerRepositoryBase<TPayload>
		where TPayload : class
	{
		protected SqlServerContext<TPayload> context { get; }
		protected string tableName { get; set; }
		public SqlServerRepositoryBase(IDatabaseContext<TPayload> context)
		{
			this.context = context as SqlServerContext<TPayload>;
		}
	}
}
