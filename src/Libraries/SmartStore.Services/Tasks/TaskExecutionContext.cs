using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;

namespace SmartStore.Services.Tasks
{
	/// <summary>
	/// Provides the context for the Execute method of the <see cref="ITask"/> interface.
	/// </summary>
	public class TaskExecutionContext
	{
		/// <summary>
		/// The shared <see cref="ILifetimeScope"/> instance created
		/// before the execution of the task's background thread.
		/// </summary>
		public object LifetimeScope { get; internal set; }
	}
}
