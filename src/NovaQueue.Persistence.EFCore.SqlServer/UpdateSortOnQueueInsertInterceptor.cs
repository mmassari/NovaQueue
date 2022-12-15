using Microsoft.EntityFrameworkCore.Diagnostics;
using NovaQueue.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NovaQueue.Persistence.EFCore.SqlServer
{
	public class UpdateSortOnQueueInsertInterceptor<TPayload> : SaveChangesInterceptor
	{
		public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
			DbContextEventData eventData, 
			InterceptionResult<int> result, 
			CancellationToken cancellationToken = default)
		{
			var dbContext = eventData.Context as NovaQueueDbContext<TPayload>;
			if(dbContext == null )
				return base.SavingChangesAsync(eventData, result, cancellationToken);

			var entries = dbContext.ChangeTracker.Entries<QueueEntry<TPayload>>();
			foreach (var entry in entries)
			{
				if(entry.State == Microsoft.EntityFrameworkCore.EntityState.Added)
				{
					var max = 0;
					try { max = dbContext.QueueEntries.Max(c => c.Sort); } catch { }
					entry.Property(c=>c.Sort).CurrentValue=  max+ 1;
				}
			}
			return base.SavingChangesAsync(eventData, result, cancellationToken);
		}
	}
}
