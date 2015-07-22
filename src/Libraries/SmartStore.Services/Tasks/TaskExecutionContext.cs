using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Autofac;
using SmartStore.Core.Domain.Tasks;

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

        /// <summary>
        /// A cancellation token for the running task.
        /// You can use ThrowIfCancellationRequested() for a hard or IsCancellationRequested for a soft break.
        /// </summary>
        public CancellationToken CancellationToken { get; internal set; }

        public ScheduleTask ScheduleTask { get; set; }
	}
}
