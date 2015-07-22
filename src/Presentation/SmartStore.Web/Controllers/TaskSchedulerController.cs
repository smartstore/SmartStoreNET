using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SmartStore.Core.Domain.Tasks;
using SmartStore.Services.Tasks;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Services.Security;
using SmartStore.Services;

namespace SmartStore.Web.Controllers
{
    
    public class TaskSchedulerController : Controller
    {
        private readonly ITaskSweeper _taskSweeper;
        private readonly IScheduleTaskService _scheduledTaskService;
        private readonly ITaskExecutor _taskExecutor;
        private readonly ICommonServices _services;

        public TaskSchedulerController(
            ITaskSweeper taskSweeper,
            IScheduleTaskService scheduledTaskService,
            ITaskExecutor taskExecutor,
            ICommonServices services)
        {
            this._taskSweeper = taskSweeper;
            this._scheduledTaskService = scheduledTaskService;
            this._taskExecutor = taskExecutor;
            this._services = services;
        }

        [HttpPost]
        public ActionResult Sweep()
        {
            if (!_taskSweeper.VerifyAuthToken(Request.Headers["X-AUTH-TOKEN"]))
                return new HttpUnauthorizedResult();

            var pendingTasks = _scheduledTaskService.GetPendingTasks();

            foreach (var task in pendingTasks)
            {
                _taskExecutor.Execute(task);
            }
            
            return Content("{0} tasks executed".FormatInvariant(pendingTasks.Count));
        }

        [HttpPost]
        public ActionResult Execute(int id /* taskId */)
        {
            if (!_taskSweeper.VerifyAuthToken(Request.Headers["X-AUTH-TOKEN"]))
                return new HttpUnauthorizedResult();

            var task = _scheduledTaskService.GetTaskById(id);
            if (task == null)
                return HttpNotFound();

            _taskExecutor.Execute(task);

            return Content("Task '{0}'  executed".FormatCurrent(task.Name));
        }

    }
}