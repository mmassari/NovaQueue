using NovaQueue.Abstractions;
using NovaQueue.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NovaQueue.Worker
{
	public static class ReflectionExtensions
	{
		public static void HandleResult<T>(this ITransactionalQueue<T> queue, QueueEntry<T> item, Result result)
		{
			item.LastAttempt = DateTime.Now;
			item.Attempts++;
			item.IsCheckedOut = false;

			if (result.Success) 
			{
				queue.Commit(item);
			}			
			else
			{
				if (result is ValidationErrorResult validationResult)
					queue.Abort(item, ErrorType.Validation, validationResult.Errors.ToArray());
				else if (result is ErrorResult errorResult)
					queue.Abort(item, ErrorType.JobExecution, errorResult.Errors.Select(c=>c).ToArray());
				else
					queue.Abort(item, ErrorType.Unhandled, new Error("Unexpected error. ErrorResult is null"));
			};
		}
		public static Task AddAsync(this List<Task> sequence, Task item)
		{
			sequence.Add(item);
			return item;
		}
		public static Task ForEachAsync<T>(this IEnumerable<T> sequence, Func<T, Task> action)
		{
			return Task.WhenAll(sequence.Select(action));
		}
		public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
		{
			if (assembly == null) throw new ArgumentNullException("assembly");
			try
			{
				return assembly.GetTypes().Where(c => !c.IsInterface);
			}
			catch (ReflectionTypeLoadException e)
			{
				return e.Types.Where(t => t != null);
			}
		}

		public static IEnumerable<Type> GetTypesWithInterface(this Assembly asm, Type interfaceType)
		{
			return asm.GetLoadableTypes().Where(interfaceType.IsAssignableFrom).ToList();
		}
	}
}
