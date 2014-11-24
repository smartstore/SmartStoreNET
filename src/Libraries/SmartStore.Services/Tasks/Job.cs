using System;
using SmartStore.Core.Domain.Tasks;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Logging;
using Autofac;
using SmartStore.Core.Data;
using System.Threading;

namespace SmartStore.Services.Tasks
{
    /// <summary>
    /// Task
    /// </summary>
	/// <remarks>
	/// Formerly <c>Task</c>. Had to rename to <c>Job</c> in order to prevent naming conflicts with <c>System.Threading.Tasks.Task</c>
	/// </remarks>
    public partial class Job
    {
		
		/// <summary>
        /// Ctor for Task
        /// </summary>
        private Job()
        {
            this.Enabled = true;
        }

        /// <summary>
        /// Ctor for Task
        /// </summary>
        /// <param name="task">Task </param>
        public Job(ScheduleTask task)
        {
			this.Type = task.Type;
            this.Enabled = task.Enabled;
            this.StopOnError = task.StopOnError;
            this.Name = task.Name;
			this.LastError = task.LastError;
			this.IsRunning = task.IsRunning;		//task.LastStartUtc.GetValueOrDefault() > task.LastEndUtc.GetValueOrDefault();
        }

        private ITask CreateTask(ILifetimeScope scope)
        {
            ITask task = null;
            if (this.Enabled)
            {
                var type2 = System.Type.GetType(this.Type);
                if (type2 != null)
                {
					object instance;
					if (!EngineContext.Current.ContainerManager.TryResolve(type2, scope, out instance))
                    {
                        //not resolved
						instance = EngineContext.Current.ContainerManager.ResolveUnregistered(type2, scope);
                    }
                    task = instance as ITask;
                }
            }
            return task;
        }

        /// <summary>
        /// Executes the task
        /// </summary>
        public void Execute(ILifetimeScope scope = null, bool throwOnError = false)
        {
            this.IsRunning = true;

			scope = scope ?? EngineContext.Current.ContainerManager.Scope();
			var faulted = false;

			try
			{
				var task = this.CreateTask(scope);
				if (task != null)
				{
					this.LastStartUtc = DateTime.UtcNow;

					var scheduleTaskService = scope.Resolve<IScheduleTaskService>();
					var scheduleTask = scheduleTaskService.GetTaskByType(this.Type);

					if (scheduleTask != null)
					{
						//update appropriate datetime properties
						scheduleTask.LastStartUtc = this.LastStartUtc;
						scheduleTaskService.UpdateTask(scheduleTask);
					}

					//execute task
					var ctx = new TaskExecutionContext()
					{
						LifetimeScope = scope,
						CancellationToken = new CancellationTokenSource(TimeSpan.FromHours(6.0))	// TODO: make that somewhat configurable (AdminAreaSettings?)
					};

					task.Execute(ctx);
					this.LastEndUtc = this.LastSuccessUtc = DateTime.UtcNow;
					this.LastError = null;
				}
				else 
				{
					faulted = true;
					this.LastError = "Could not create task instance";
				}
			}
			catch (Exception ex)
			{
				faulted = true;
				this.Enabled = !this.StopOnError;
				this.LastEndUtc = DateTime.UtcNow;
				this.LastError = ex.Message.Truncate(995, "...");

				//log error
				var logger = scope.Resolve<ILogger>();
				logger.Error(string.Format("Error while running the '{0}' schedule task. {1}", this.Name, ex.Message), ex);
				if (throwOnError)
				{
					throw;
				}
			}
			finally
			{
				var scheduleTaskService = scope.Resolve<IScheduleTaskService>();
				var scheduleTask = scheduleTaskService.GetTaskByType(this.Type);

				if (scheduleTask != null)
				{
					// update appropriate properties
					scheduleTask.LastError = this.LastError;
					scheduleTask.LastEndUtc = this.LastEndUtc;
					if (!faulted)
					{
						scheduleTask.LastSuccessUtc = this.LastSuccessUtc;
					}

					scheduleTaskService.UpdateTask(scheduleTask);
				}

				scope.Dispose();

				this.IsRunning = false;
			}
        }

        /// <summary>
        /// A value indicating whether a task is running
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Datetime of the last start
        /// </summary>
        public DateTime? LastStartUtc { get; private set; }

        /// <summary>
        /// Datetime of the last end
        /// </summary>
        public DateTime? LastEndUtc { get; private set; }

        /// <summary>
        /// Datetime of the last success
        /// </summary>
        public DateTime? LastSuccessUtc { get; private set; }

		/// <summary>
		/// Message of the last error
		/// </summary>
		public string LastError { get; private set; }

        /// <summary>
        /// A value indicating type of the task
        /// </summary>
        public string Type { get; private set; }

        /// <summary>
        /// A value indicating whether to stop task on error
        /// </summary>
        public bool StopOnError { get; private set; }

        /// <summary>
        /// Get the task name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// A value indicating whether the task is enabled
        /// </summary>
        public bool Enabled { get; set; }
    }
}
