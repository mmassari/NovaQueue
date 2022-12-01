using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace NovaQueue.Abstractions
{
	public static class Extensions
	{
		public static IOptions<NovaQueueOptions<T>> ToIOptions<T>(this NovaQueueOptions<T> options) =>
			new NovaOptions<NovaQueueOptions<T>>(options);
		
	}
}
