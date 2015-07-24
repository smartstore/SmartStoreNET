using SmartStore.Core.Data;
using SmartStore.Core.Domain.Tasks;
using SmartStore.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core.Async;
using System.Threading;
using SmartStore.Core.Plugins;
using Autofac;

namespace SmartStore.Services.Tasks
{

    public class TaskExecutor : ITaskExecutor
    {
        private readonly IScheduleTaskService _scheduledTaskService;
		private readonly IDbContext _dbContext;
        private readonly Func<Type, ITask> _taskResolver;
		private readonly IComponentContext _componentContext;

        public TaskExecutor(IScheduleTaskService scheduledTaskService, IDbContext dbContext, IComponentContext componentContext, Func<Type, ITask> taskResolver)
        {
            this._scheduledTaskService = scheduledTaskService;
			this._dbContext = dbContext;
			this._componentContext = componentContext;
            this._taskResolver = taskResolver;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void Execute(ScheduleTask task, bool throwOnError = false)
        {
			if (task.IsRunning)
                return;

			if (AsyncRunner.AppShutdownCancellationToken.IsCancellationRequested)
				return;

            bool faulted = false;
            string lastError = null;
            ITask instance = null;
			string stateName = null;

			Type taskType = null;

			try
			{
				taskType = Type.GetType(task.Type);
				if (!PluginManager.IsActivePluginAssembly(taskType.Assembly))
					return;
			}
			catch
			{
				return;
			}

            try
            {
                // create task instance
				instance = _taskResolver(taskType);
				stateName = task.Id.ToString();
                
				// prepare and save entity
                task.LastStartUtc = DateTime.UtcNow;
                task.LastEndUtc = null;
                task.NextRunUtc = null;
                _scheduledTaskService.UpdateTask(task);

				// create & set a composite CancellationTokenSource which also contains the global app shoutdown token
				var cts = CancellationTokenSource.CreateLinkedTokenSource(AsyncRunner.AppShutdownCancellationToken, new CancellationTokenSource().Token);
				AsyncState.Current.SetCancelTokenSource<TaskProgressInfo>(cts, stateName);

				// tell AsyncState about a new running task
				var taskProgressInfo = new TaskProgressInfo
				{
					ScheduleTaskId = task.Id,
					TaskType = instance.GetType()
				};
				AsyncState.Current.Set(taskProgressInfo, stateName, true);

				var ctx = new TaskExecutionContext(_componentContext)
				{
					ScheduleTask = task.Clone(),
					CancellationToken = cts.Token
					// TODO: Remove LifetimeScope
				};

                instance.Execute(ctx);
            }
            catch (Exception ex)
            {
                faulted = true;
                Logger.Error(string.Format("Error while running scheduled task '{0}'. {1}", task.Name, ex.Message), ex);
                lastError = ex.Message.Truncate(995, "...");
                if (throwOnError)
                {
                    throw;
                }
            }
            finally
            {
				// remove from AsyncState
				if (stateName.HasValue())
				{
					AsyncState.Current.Remove<TaskProgressInfo>(stateName);
				}
				
				var now = DateTime.UtcNow;
                task.LastError = lastError;
                task.LastEndUtc = now;

                if (faulted)
                {
                    if (task.StopOnError || instance == null)
                    {
                        task.Enabled = false;
                    }
                }
                else
                {
                    task.LastSuccessUtc = now;
                }

                if (task.Enabled)
                {
					task.NextRunUtc = now.AddSeconds(task.Seconds);
                }

                _scheduledTaskService.UpdateTask(task);
            }
        }

		private CancellationTokenSource CreateCompositeCancellationTokenSource(CancellationToken userCancellationToken)
		{
			return CancellationTokenSource.CreateLinkedTokenSource(AsyncRunner.AppShutdownCancellationToken, userCancellationToken);
		}
    }

}
