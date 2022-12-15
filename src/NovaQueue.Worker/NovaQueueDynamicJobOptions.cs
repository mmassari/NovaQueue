using NovaQueue.Abstractions;
using System;
using System.Linq;
using System.Reflection;

namespace NovaQueue.Worker
{
	public class NovaQueueDynamicJobOptions
	{
		public string AssemblyFile { get; set; } = "";
		public string JobClass { get; set; } = "";

		public IQueueJobAsync<TPayload> GetJob<TPayload>()
		{
			Assembly assembly;
			var jobInterface = typeof(IQueueJobAsync<TPayload>);
			if (string.IsNullOrWhiteSpace(AssemblyFile))
				assembly = Assembly.GetExecutingAssembly();
			else
				assembly = Assembly.LoadFile(AssemblyFile);

			var jobs = assembly.GetTypesWithInterface(typeof(IQueueJobAsync<TPayload>));
			if (jobs is null || jobs.Count() == 0)
				throw new NullReferenceException();

			return (IQueueJobAsync<TPayload>)Activator.CreateInstance(jobs.First())!;
		}
	}
}