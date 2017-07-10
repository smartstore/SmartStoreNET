using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Autofac;
using SmartStore.Core;
using SmartStore.Core.Async;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Tasks;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.Services.Customers;

namespace SmartStore.Services.Tasks
{
	public class TaskExecutor : ITaskExecutor
    {
        private readonly IScheduleTaskService _scheduledTaskService;
		private readonly IDbContext _dbContext;
		private readonly IWorkContext _workContext;
        private readonly Func<Type, ITask> _taskResolver;
		private readonly IComponentContext _componentContext;
		private readonly IAsyncState _asyncState;

		public const string CurrentCustomerIdParamName = "CurrentCustomerId";
		public const string CurrentStoreIdParamName = "CurrentStoreId";

		public TaskExecutor(
			IScheduleTaskService scheduledTaskService, 
			IDbContext dbContext,
 			ICustomerService customerService,
			IWorkContext workContext,
			IComponentContext componentContext,
			IAsyncState asyncState,
			Func<Type, ITask> taskResolver)
        {
            _scheduledTaskService = scheduledTaskService;
			_dbContext = dbContext;
			_workContext = workContext;
			_componentContext = componentContext;
			_asyncState = asyncState;
            _taskResolver = taskResolver;

            Logger = NullLogger.Instance;
			T = NullLocalizer.Instance;
		}

        public ILogger Logger { get; set; }
		public Localizer T { get; set; }

		public void Execute(
			ScheduleTask task,
			IDictionary<string, string> taskParameters = null,
            bool throwOnError = false)
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

			Type taskType = null;

			try
			{
				taskType = Type.GetType(task.Type);

				if (taskType == null)
				{
					Logger.DebugFormat("Invalid scheduled task type: {0}", task.Type.NaIfEmpty());
				}

				if (taskType == null)
					return;

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
				task.ProgressPercent = null;
				task.ProgressMessage = null;

                _scheduledTaskService.UpdateTask(task);

				// create & set a composite CancellationTokenSource which also contains the global app shoutdown token
				var cts = CancellationTokenSource.CreateLinkedTokenSource(AsyncRunner.AppShutdownCancellationToken, new CancellationTokenSource().Token);
				_asyncState.SetCancelTokenSource<ScheduleTask>(cts, stateName);

				var ctx = new TaskExecutionContext(_componentContext, task)
				{
					ScheduleTask = task.Clone(),
					CancellationToken = cts.Token,
					Parameters = taskParameters ?? new Dictionary<string, string>()
				};

				Logger.DebugFormat("Executing scheduled task: {0}", task.Type);
				instance.Execute(ctx);
            }
            catch (Exception exception)
            {
                faulted = true;
				canceled = exception is OperationCanceledException;
				lastError = exception.Message.Truncate(995, "...");

				if (canceled)
					Logger.Warn(exception, T("Admin.System.ScheduleTasks.Cancellation", task.Name));
				else
					Logger.Error(exception, string.Concat(T("Admin.System.ScheduleTasks.RunningError", task.Name), ": ", exception.Message));

                if (throwOnError)
                {
                    throw;
                }
            }
            finally
            {
				task.ProgressPercent = null;
				task.ProgressMessage = null;

				var now = DateTime.UtcNow;
				task.LastError = lastError;
				task.LastEndUtc = now;
				
				if (faulted)
				{
					if ((!canceled && task.StopOnError) || instance == null)
					{
						task.Enabled = false;
					}
				}
				else
				{
					task.LastSuccessUtc = now;
				}

				Logger.DebugFormat("Executed scheduled task: {0}. Elapsed: {1} ms.", task.Type, (now - task.LastStartUtc.Value).TotalMilliseconds);

				if (task.Enabled)
				{
					task.NextRunUtc = _scheduledTaskService.GetNextSchedule(task);
				}

				// remove from AsyncState
				if (stateName.HasValue())
				{
					_asyncState.Remove<ScheduleTask>(stateName);
				}

				_scheduledTaskService.UpdateTask(task);
            }
        }
    }
}
