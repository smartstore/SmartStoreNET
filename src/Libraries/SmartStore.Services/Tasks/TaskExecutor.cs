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
using SmartStore.Services.Customers;
using SmartStore.Core;
using SmartStore.Core.Domain.Customers;

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

        public TaskExecutor(
			IScheduleTaskService scheduledTaskService, 
			IDbContext dbContext,
 			ICustomerService customerService,
			IWorkContext workContext,
			IComponentContext componentContext, 
			Func<Type, ITask> taskResolver)
        {
            this._scheduledTaskService = scheduledTaskService;
			this._dbContext = dbContext;
			this._customerService = customerService;
			this._workContext = workContext;
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
                // set background task system customer as current customer
				var customer = _customerService.GetCustomerBySystemName(SystemCustomerNames.BackgroundTask);
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
