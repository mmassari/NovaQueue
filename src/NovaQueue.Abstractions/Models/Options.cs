using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace NovaQueue.Abstractions
{
	public class NovaOptions<T> : IOptions<T> 
		where T : class
	{
		private readonly T options;

		public NovaOptions(T options)
		{
			this.options = options;
		}
		public T Value => options;
	}
}
