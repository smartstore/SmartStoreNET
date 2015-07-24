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
		private readonly IComponentContext _componentContext;

		internal TaskExecutionContext(IComponentContext componentContext)
		{
			this._componentContext = componentContext;
		}

		public T Resolve<T>(object key = null) where T : class
		{
			if (key == null)
			{
				return _componentContext.Resolve<T>();
			}
			return _componentContext.ResolveKeyed<T>(key);
		}

		public T ResolveNamed<T>(string name) where T : class
		{
			return _componentContext.ResolveNamed<T>(name);
		}

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
