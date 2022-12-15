using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NovaQueue.Abstractions
{
	public interface IDatabaseContext<TPayload>
	{
		//void Initialize(string name);
		bool DeadLetterQueueEnabled { get; }
		bool CompletedQueueEnabled { get;  }
		string CollectionName { get; }
		string DeadLetterCollectionName { get; }
		string CompletedCollectionName { get; }
		void BeginTransaction();
		void CommitTransaction();
		void RollbackTransaction();
	}
}
