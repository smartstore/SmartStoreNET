using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.SessionState;
using SmartStore.Collections;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Stores;
using SmartStore.Services;
using SmartStore.Services.Customers;
using SmartStore.Services.Tasks;

namespace SmartStore.Web.Controllers
{
    [SessionState(SessionStateBehavior.ReadOnly)]
    public class TaskSchedulerController : Controller
    {
        private readonly ITaskScheduler _taskScheduler;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly ITaskExecutor _taskExecutor;
        private readonly ICustomerService _customerService;
        private readonly ICommonServices _services;
        //private readonly DateTime _sweepStart;

        public TaskSchedulerController(
            ITaskScheduler taskScheduler,
            IScheduleTaskService scheduleTaskService,
            ITaskExecutor taskExecutor,
            ICustomerService customerService,
            ICommonServices services)
        {
            _taskScheduler = taskScheduler;
            _scheduleTaskService = scheduleTaskService;
            _taskExecutor = taskExecutor;
            _customerService = customerService;
            _services = services;

            //// Fuzzy: substract the possible max time passed since timer trigger in ITaskScheduler
            //_sweepStart = DateTime.UtcNow.AddMilliseconds(-500);
        }

        [HttpPost]
        public async Task<ActionResult> Sweep()
        {
            if (!_taskScheduler.VerifyAuthToken(Request.Headers["X-AUTH-TOKEN"]))
            {
                return new HttpUnauthorizedResult();
            }

            var pendingTasks = await _scheduleTaskService.GetPendingTasksAsync();
            var count = 0;
            var taskParameters = QueryString.Current.ToDictionary();

            if (pendingTasks.Count > 0)
            {
                Virtualize(taskParameters);
            }

            for (var i = 0; i < pendingTasks.Count; i++)
            {
                var task = pendingTasks[i];

                if (i > 0 /*&& (DateTime.UtcNow - _sweepStart).TotalMinutes > _taskScheduler.SweepIntervalMinutes*/)
                {
                    // Maybe a subsequent Sweep call or another machine in a webfarm executed 
                    // successive tasks already.
                    // To be able to determine this, we need to reload the entity from the database.
                    // The TaskExecutor will exit when the task should be in running state then.
                    _services.DbContext.ReloadEntity(task);
                    task.LastHistoryEntry = _scheduleTaskService.GetLastHistoryEntryByTaskId(task.Id);
                }

                if (task.IsPending)
                {
                    await _taskExecutor.ExecuteAsync(task, taskParameters);
                    count++;
                }
            }

            return Content("{0} of {1} pending tasks executed".FormatInvariant(count, pendingTasks.Count));
        }

        [HttpPost]
        public async Task<ActionResult> Execute(int id /* taskId */)
        {
            if (!_taskScheduler.VerifyAuthToken(Request.Headers["X-AUTH-TOKEN"]))
            {
                return new HttpUnauthorizedResult();
            }

            var task = _scheduleTaskService.GetTaskById(id);
            if (task == null)
            {
                return HttpNotFound();
            }

            var taskParameters = QueryString.Current.ToDictionary();
            Virtualize(taskParameters);

            await _taskExecutor.ExecuteAsync(task, taskParameters);

            return Content("Task '{0}' executed".FormatCurrent(task.Name));
        }

        public ContentResult Noop()
        {
            return Content("noop");
        }

        protected virtual void Virtualize(IDictionary<string, string> taskParameters)
        {
            // Try virtualize current customer (which is necessary when user manually executes a task).
            Customer customer = null;
            if (taskParameters != null && taskParameters.ContainsKey(TaskExecutor.CurrentCustomerIdParamName))
            {
                customer = _customerService.GetCustomerById(taskParameters[TaskExecutor.CurrentCustomerIdParamName].ToInt());
            }

            if (customer == null)
            {
                // No virtualization: set background task system customer as current customer.
                customer = _customerService.GetCustomerBySystemName(SystemCustomerNames.BackgroundTask);
            }

            // Set virtual customer.
            _services.WorkContext.CurrentCustomer = customer;

            // Try virtualize current store.
            Store store = null;
            if (taskParameters != null && taskParameters.ContainsKey(TaskExecutor.CurrentStoreIdParamName))
            {
                store = _services.StoreService.GetStoreById(taskParameters[TaskExecutor.CurrentStoreIdParamName].ToInt());
                if (store != null)
                {
                    // Set virtual store.
                    _services.StoreContext.CurrentStore = store;
                }
            }
        }

    }
}