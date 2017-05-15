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
		private readonly ICustomerService _customerService;
		private readonly IWorkContext _workContext;
        private readonly Func<Type, ITask> _taskResolver;
		private readonly IComponentContext _componentContext;
		private readonly IAsyncState _asyncState;

		public const string CurrentCustomerIdParamName = "CurrentCustomerId";

        public TaskExecutor(
			IScheduleTaskService scheduledTaskService, 
			IDbContext dbContext,
 			ICustomerService customerService,
			IWorkContext workContext,
			IComponentContext componentContext,
			IAsyncState asyncState,
			Func<Type, ITask> taskResolver)
        {
            this._scheduledTaskService = scheduledTaskService;
			this._dbContext = dbContext;
			this._customerService = customerService;
			this._workContext = workContext;
			this._componentContext = componentContext;
			this._asyncState = asyncState;
            this._taskResolver = taskResolver;

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
                Customer customer = null;
                
                // try virtualize current customer (which is necessary when user manually executes a task)
                if (taskParameters != null && taskParameters.ContainsKey(CurrentCustomerIdParamName))
                {
                    customer = _customerService.GetCustomerById(taskParameters[CurrentCustomerIdParamName].ToInt());
                }
                
                if (customer == null)
                {
                    // no virtualization: set background task system customer as current customer
                    customer = _customerService.GetCustomerBySystemName(SystemCustomerNames.BackgroundTask);
                }
				
				_workContext.CurrentCustomer = customer;

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
				// remove from AsyncState
				if (stateName.HasValue())
				{
					_asyncState.Remove<ScheduleTask>(stateName);
				}

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

				_scheduledTaskService.UpdateTask(task);
            }
        }

		private CancellationTokenSource CreateCompositeCancellationTokenSource(CancellationToken userCancellationToken)
		{
			return CancellationTokenSource.CreateLinkedTokenSource(AsyncRunner.AppShutdownCancellationToken, userCancellationToken);
		}
    }
}
