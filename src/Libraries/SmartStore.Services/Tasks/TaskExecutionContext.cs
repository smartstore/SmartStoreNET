using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Autofac;
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
			_componentContext = componentContext;
			_originalTask = originalTask;
			Parameters = new Dictionary<string, string>();
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
        /// You can use ThrowIfCancellationRequested() for a hard, or IsCancellationRequested for a soft break.
        /// </summary>
        public CancellationToken CancellationToken { get; internal set; }

        public ScheduleTask ScheduleTask { get; set; }

		public IDictionary<string, string> Parameters { get; set; }

		/// <summary>
		/// Persists a task's progress information to the database
		/// </summary>
		/// <param name="value">Progress value (numerator)</param>
		/// <param name="maximum">Progress maximum (denominator)</param>
		/// <param name="message">Progress message. Can be <c>null</c>.</param>
		/// <param name="immediately">if <c>true</c>, saves the updated task entity immediately, or lazily with the next database commit otherwise.</param>
		public void SetProgress(int value, int maximum, string message, bool immediately = false)
		{
			if (value == 0 && maximum == 0)
			{
				SetProgress(null, message, immediately);
			}
			else
			{
				float fraction = (float)value / (float)Math.Max(maximum, 1f);
				int percentage = (int)Math.Round(fraction * 100f, 0);

				SetProgress(Math.Min(Math.Max(percentage, 0), 100), message, immediately);
			}
		}

		/// <summary>
		/// Persists a task's progress information to the database
		/// </summary>
		/// <param name="progress">Percentual progress. Can be <c>null</c> or a value between 0 and 100.</param>
		/// <param name="message">Progress message. Can be <c>null</c>.</param>
		/// <param name="immediately">if <c>true</c>, saves the updated task entity immediately, or lazily with the next database commit otherwise.</param>
		public virtual void SetProgress(int? progress, string message, bool immediately =  false)
		{
			if (progress.HasValue)
				Guard.InRange(progress.Value, 0, 100, nameof(progress));

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
					//dbContext.ChangeState(_originalTask, System.Data.Entity.EntityState.Modified);
					dbContext.SaveChanges();
				}
				catch { }
			}
		}

		/// <summary>
		/// Creates a transient context, which can be passed to methods requiring a <see cref="TaskExecutionContext"/> instance. 
		/// Such methods are usually executed by the task executor in the background. If a manual invocation of such
		/// methods is required, this is the way to go.
		/// </summary>
		/// <param name="componentContext">The component context</param>
		/// <returns>A transient context</returns>
		public static TaskExecutionContext CreateTransientContext(IComponentContext componentContext)
		{
			return CreateTransientContext(componentContext, CancellationToken.None);
		}

		/// <summary>
		/// Creates a transient context, which can be passed to methods requiring a <see cref="TaskExecutionContext"/> instance. 
		/// Such methods are usually executed by the task executor in the background. If a manual invocation of such
		/// methods is required, this is the way to go.
		/// </summary>
		/// <param name="componentContext">The component context</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns>A transient context</returns>
		public static TaskExecutionContext CreateTransientContext(IComponentContext componentContext, CancellationToken cancellationToken)
		{
			var originalTask = new ScheduleTask { Name = "Transient", IsHidden = true, Enabled = true };
			var context = new TransientTaskExecutionContext(componentContext, originalTask);

			context.CancellationToken = cancellationToken;
			context.ScheduleTask = originalTask.Clone();

			return context;
		}
	}

	internal class TransientTaskExecutionContext : TaskExecutionContext
	{
		public TransientTaskExecutionContext(IComponentContext componentContext, ScheduleTask originalTask)
			: base(componentContext, originalTask)
		{
		}

		public override void SetProgress(int? progress, string message, bool immediately = false)
		{
			base.SetProgress(progress, message, false /* skip committing to DB */);
		}
	}
}
