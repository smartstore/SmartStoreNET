using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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

        public async Task ExecuteAsync(
            ScheduleTask entity,
            IDictionary<string, string> taskParameters = null,
            bool throwOnError = false)
        {
            if (AsyncRunner.AppShutdownCancellationToken.IsCancellationRequested)
            {
                return;
            }

            if (entity.LastHistoryEntry == null)
            {
                // The task was started manually.
                entity.LastHistoryEntry = _scheduledTaskService.GetLastHistoryEntryByTaskId(entity.Id);
            }

            if (entity?.LastHistoryEntry?.IsRunning == true)
            {
                return;
            }

            bool faulted = false;
            bool canceled = false;
            string lastError = null;
            ITask task = null;
            string stateName = null;
            Type taskType = null;
            Exception exception = null;

            var historyEntry = new ScheduleTaskHistory
            {
                ScheduleTaskId = entity.Id,
                IsRunning = true,
                MachineName = _env.MachineName.EmptyNull(),
                StartedOnUtc = DateTime.UtcNow
            };

            try
            {
                taskType = Type.GetType(entity.Type);
                if (taskType == null)
                {
                    Logger.DebugFormat("Invalid scheduled task type: {0}", entity.Type.NaIfEmpty());
                }

                if (taskType == null)
                    return;

                if (!PluginManager.IsActivePluginAssembly(taskType.Assembly))
                    return;

                entity.ScheduleTaskHistory.Add(historyEntry);
                _scheduledTaskService.UpdateTask(entity);
            }
            catch
            {
                return;
            }

            try
            {
                // Task history entry has been successfully added, now we execute the task.
                // Create task instance.
                task = _taskResolver(taskType);
                stateName = entity.Id.ToString();

                // Create & set a composite CancellationTokenSource which also contains the global app shoutdown token.
                var cts = CancellationTokenSource.CreateLinkedTokenSource(AsyncRunner.AppShutdownCancellationToken, new CancellationTokenSource().Token);
                _asyncState.SetCancelTokenSource<ScheduleTask>(cts, stateName);

                var ctx = new TaskExecutionContext(_componentContext, historyEntry)
                {
                    ScheduleTaskHistory = historyEntry.Clone(),
                    CancellationToken = cts.Token,
                    Parameters = taskParameters ?? new Dictionary<string, string>()
                };

                Logger.DebugFormat("Executing scheduled task: {0}", entity.Type);

                if (task is IAsyncTask asyncTask)
                {
                    await asyncTask.ExecuteAsync(ctx);
                }
                else
                {
                    task.Execute(ctx);
                }
            }
            catch (Exception ex)
            {
                exception = ex;
                faulted = true;
                canceled = ex is OperationCanceledException;
                lastError = ex.ToAllMessages(true);

                if (canceled)
                {
                    Logger.Warn(ex, T("Admin.System.ScheduleTasks.Cancellation", entity.Name));
                }
                else
                {
                    Logger.Error(ex, string.Concat(T("Admin.System.ScheduleTasks.RunningError", entity.Name), ": ", ex.Message));
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
                    if ((!canceled && entity.StopOnError) || task == null)
                    {
                        entity.Enabled = false;
                        updateTask = true;
                    }
                }
                else
                {
                    historyEntry.SucceededOnUtc = now;
                }

                try
                {
                    Logger.DebugFormat("Executed scheduled task: {0}. Elapsed: {1} ms.", entity.Type, (now - historyEntry.StartedOnUtc).TotalMilliseconds);

                    // Remove from AsyncState.
                    if (stateName.HasValue())
                    {
                        // We don't just remove the cancellation token, but the whole state (along with the token)
                        // for the case that a state was registered during task execution.
                        _asyncState.Remove<ScheduleTask>(stateName);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }

                if (entity.Enabled)
                {
                    entity.NextRunUtc = _scheduledTaskService.GetNextSchedule(entity);
                    updateTask = true;
                }

                _scheduledTaskService.UpdateHistoryEntry(historyEntry);

                if (updateTask)
                {
                    _scheduledTaskService.UpdateTask(entity);
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
