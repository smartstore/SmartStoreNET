using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Autofac;
using SmartStore.Core.Async;
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

		public void SetProgress(float? progress, string message)
		{
			if (progress.HasValue)
				Guard.ArgumentInRange(progress.Value, 0, 100, "progress");

			var stateName = ScheduleTask.Id.ToString();

			AsyncState.Current.Update<TaskProgressInfo>(x => 
			{ 
				x.Progress = progress; 
				x.Message = message; 
			}, stateName);
		}
	}
}
