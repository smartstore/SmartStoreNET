using System;
using System.Threading;
using Autofac;
using SmartStore.Core.Async;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Tasks;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;

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
			bool canceled = false;
            string lastError = null;
            ITask instance = null;
			string stateName = null;
			int taskId = task.Id;
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
				AsyncState.Current.SetCancelTokenSource<ScheduleTask>(cts, stateName);

				var ctx = new TaskExecutionContext(_componentContext, task)
				{
					ScheduleTask = task.Clone(),
					CancellationToken = cts.Token
				};

                instance.Execute(ctx);
            }
            catch (Exception ex)
            {
                faulted = true;
				canceled = ex is OperationCanceledException;
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
					AsyncState.Current.Remove<ScheduleTask>(stateName);
				}

				// "task" may not updatable here. solution: get fresh instance and update it
				var task2 = _scheduledTaskService.GetTaskById(taskId);
				var now = DateTime.UtcNow;

				task2.ProgressPercent = null;
				task2.ProgressMessage = null;
                task2.LastError = lastError;
                task2.LastEndUtc = now;

                if (faulted)
                {
                    if ((!canceled && task2.StopOnError) || instance == null)
                    {
                        task2.Enabled = false;
                    }
                }
                else
                {
                    task2.LastSuccessUtc = now;
                }

                if (task2.Enabled)
                {
					task2.NextRunUtc = now.AddSeconds(task2.Seconds);
                }

                _scheduledTaskService.UpdateTask(task2);
            }
        }

		private CancellationTokenSource CreateCompositeCancellationTokenSource(CancellationToken userCancellationToken)
		{
			return CancellationTokenSource.CreateLinkedTokenSource(AsyncRunner.AppShutdownCancellationToken, userCancellationToken);
		}
    }

}
