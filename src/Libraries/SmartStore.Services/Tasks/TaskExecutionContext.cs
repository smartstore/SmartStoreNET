using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Autofac;
using SmartStore.Core.Async;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Tasks;

namespace SmartStore.Services.Tasks
{
	/// <summary>
	/// Provides the context for the Execute method of the <see cref="ITask"/> interface.
	/// </summary>
	public class TaskExecutionContext
	{
		private readonly IComponentContext _componentContext;
		private readonly ScheduleTask _originalTask;

		internal TaskExecutionContext(IComponentContext componentContext, ScheduleTask originalTask)
		{
			this._componentContext = componentContext;
			this._originalTask = originalTask;
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

		/// <summary>
		/// Persists a task's progress information information to the database
		/// </summary>
		/// <param name="progress">Percentual progress. Can be <c>null</c> or a value between 0 and 100.</param>
		/// <param name="message">Progress message. Can be <c>null</c>.</param>
		/// <param name="immediately">if <c>true</c>, saves the updated task entity immediately, or lazily with the next database commit otherwise.</param>
		public void SetProgress(int? progress, string message, bool immediately =  false)
		{
			if (progress.HasValue)
				Guard.ArgumentInRange(progress.Value, 0, 100, "progress");

			// update cloned entity
			ScheduleTask.ProgressPercent = progress;
			ScheduleTask.ProgressMessage = message;

			// update attached entity
			_originalTask.ProgressPercent = progress;
			_originalTask.ProgressMessage = message;

			if (immediately)
			{
				try // dont't let this abort the task on failure
				{
					var dbContext = _componentContext.Resolve<IDbContext>();
					dbContext.ChangeState(_originalTask, System.Data.Entity.EntityState.Modified);
					dbContext.SaveChanges();
				}
				catch { }
			}
		}
	}
}
