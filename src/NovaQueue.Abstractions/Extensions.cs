using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace NovaQueue.Abstractions
{
	public static class Extensions
	{
		public static IOptions<QueueOptions<T>> ToIOptions<T>(this QueueOptions<T> options) =>
			new NovaOptions<QueueOptions<T>>(options);
		
	}
}
