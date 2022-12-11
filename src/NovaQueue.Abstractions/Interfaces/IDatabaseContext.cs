using System;
using System.Collections.Generic;
using System.Text;

namespace NovaQueue.Abstractions
{
	public interface IDatabaseContext<TPayload>
	{
		//void Initialize(string name);
		void BeginTransaction();
		void CommitTransaction();
		void RollbackTransaction();

	}
}
