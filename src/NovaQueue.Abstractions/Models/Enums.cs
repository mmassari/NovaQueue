using System;
using System.Collections.Generic;
using System.Text;

namespace NovaQueue.Abstractions
{
	public enum OnFailurePolicy
	{
		MoveLast,
		Retry,
		Discard
	}
	[Flags]
	public enum CollectionType
	{
		Queue = 1,
		DeadLetter = 2,
		Completed = 4
	}
}
