using System;
using System.Collections.Generic;
using System.Threading;
using Autofac;
using SmartStore.Core;
using SmartStore.Core.Async;
using SmartStore.Core.Domain.Tasks;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.Core.Utilities;

namespace SmartStore.Services.Tasks
{
    public class TaskExecutor : ITaskExecutor
    {
        private readonly IScheduleTaskService _scheduledTaskService;
        private readonly Func<Type, ITask> _taskResolver;
		private readonly IComponentContext _componentContext;
		private readonly IAsyncState _asyncState;
        private readonly IApplicationEnvironment _env;

        public const string CurrentCustomerIdParamName = "CurrentCustomerId";
		public const string CurrentStoreIdParamName = "CurrentStoreId";

		public TaskExecutor(
			IScheduleTaskService scheduledTaskService, 
			IComponentContext componentContext,
			IAsyncState asyncState,
			Func<Type, ITask> taskResolver,
            IApplicationEnvironment env)
        {
            _scheduledTaskService = scheduledTaskService;
			_componentContext = componentContext;
			_asyncState = asyncState;
            _taskResolver = taskResolver;
            _env = env;

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
            if (AsyncRunner.AppShutdownCancellationToken.IsCancellationRequested)
            {
                return;
            }

            if (task.LastHistoryEntry == null)
            {
                // The task was started manually.
                task.LastHistoryEntry = _scheduledTaskService.GetLastHistoryEntryByTaskId(task.Id);
            }

            if (task?.LastHistoryEntry?.IsRunning == true)
            {
                return;
            }

            bool faulted = false;
			bool canceled = false;
            string lastError = null;
            ITask instance = null;
			string stateName = null;
			Type taskType = null;
            Exception exception = null;

            var historyEntry = new ScheduleTaskHistory
            {
                ScheduleTaskId = task.Id,
                IsRunning = true,
                MachineName = _env.MachineName.EmptyNull(),
                StartedOnUtc = DateTime.UtcNow
            };

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

                task.ScheduleTaskHistory.Add(historyEntry);
                _scheduledTaskService.UpdateTask(task);
            }
            catch
			{
				return;
			}

            try
            {
                // Task history entry has been successfully added, now we execute the task.
				// Create task instance.
				instance = _taskResolver(taskType);
				stateName = task.Id.ToString();

				// Create & set a composite CancellationTokenSource which also contains the global app shoutdown token.
				var cts = CancellationTokenSource.CreateLinkedTokenSource(AsyncRunner.AppShutdownCancellationToken, new CancellationTokenSource().Token);
				_asyncState.SetCancelTokenSource<ScheduleTask>(cts, stateName);

				var ctx = new TaskExecutionContext(_componentContext, historyEntry)
				{
                    ScheduleTaskHistory = historyEntry.Clone(),
					CancellationToken = cts.Token,
					Parameters = taskParameters ?? new Dictionary<string, string>()
				};

				Logger.DebugFormat("Executing scheduled task: {0}", task.Type);
				instance.Execute(ctx);
            }
            catch (Exception ex)
            {
                exception = ex;
                faulted = true;
				canceled = ex is OperationCanceledException;
				lastError = ex.Message.Truncate(995, "...");

                if (canceled)
                {
                    Logger.Warn(ex, T("Admin.System.ScheduleTasks.Cancellation", task.Name));
                }
                else
                {
                    Logger.Error(ex, string.Concat(T("Admin.System.ScheduleTasks.RunningError", task.Name), ": ", ex.Message));
                }
            }
            finally
            {
                var now = DateTime.UtcNow;
                var updateTask = false;

                historyEntry.IsRunning = false;
                historyEntry.ProgressPercent = null;
                historyEntry.ProgressMessage = null;
                historyEntry.Error = lastError;
                historyEntry.FinishedOnUtc = now;
				
				if (faulted)
				{
					if ((!canceled && task.StopOnError) || instance == null)
					{
						task.Enabled = false;
                        updateTask = true;
					}
				}
				else
				{
                    historyEntry.SucceededOnUtc = now;
				}

                try
                {
                    Logger.DebugFormat("Executed scheduled task: {0}. Elapsed: {1} ms.", task.Type, (now - historyEntry.StartedOnUtc).TotalMilliseconds);

                    // Remove from AsyncState.
                    if (stateName.HasValue())
                    {
                        _asyncState.Remove<ScheduleTask>(stateName);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }

                if (task.Enabled)
				{
					task.NextRunUtc = _scheduledTaskService.GetNextSchedule(task);
                    updateTask = true;
                }

                _scheduledTaskService.UpdateHistoryEntry(historyEntry);

                if (updateTask)
                {
                    _scheduledTaskService.UpdateTask(task);
                }

                Throttle.Check("Delete old schedule task history entries", TimeSpan.FromDays(1), () => _scheduledTaskService.DeleteHistoryEntries() > 0);
            }

            if (throwOnError && exception != null)
            {
                throw exception;
            }
        }
    }
}
