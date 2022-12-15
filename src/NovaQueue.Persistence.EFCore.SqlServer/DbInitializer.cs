using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using NovaQueue.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace NovaQueue.Persistence.EFCore.SqlServer
{
	public class DbInitializer<TPayload>
	{
		private readonly SqlServerContext<TPayload> _context;

		public DbInitializer(IDatabaseContext<TPayload> context)
		{
			_context = context as SqlServerContext<TPayload>;
		}

		public void Run()
		{
			var ctx = _context.CreateDbContext();
			//Se il db non esiste
			if(!(ctx.Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator).Exists())
			{
				ctx.Database.EnsureDeleted();
				ctx.Database.EnsureCreated();
			}

		}
	}
}
