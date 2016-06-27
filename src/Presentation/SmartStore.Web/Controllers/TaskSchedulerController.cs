using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;
using SmartStore.Core.Domain.Tasks;
using SmartStore.Services.Tasks;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Services.Security;
using SmartStore.Services;
using SmartStore.Collections;

namespace SmartStore.Web.Controllers
{
   
	[SessionState(SessionStateBehavior.ReadOnly)]
    public class TaskSchedulerController : Controller
    {
        private readonly ITaskScheduler _taskScheduler;
        private readonly IScheduleTaskService _scheduledTaskService;
        private readonly ITaskExecutor _taskExecutor;
        private readonly ICommonServices _services;

        public TaskSchedulerController(
			ITaskScheduler taskScheduler,
            IScheduleTaskService scheduledTaskService,
            ITaskExecutor taskExecutor,
            ICommonServices services)
        {
			this._taskScheduler = taskScheduler;
            this._scheduledTaskService = scheduledTaskService;
            this._taskExecutor = taskExecutor;
            this._services = services;
        }

        [HttpPost]
        public ActionResult Sweep()
        {
            if (!_taskScheduler.VerifyAuthToken(Request.Headers["X-AUTH-TOKEN"]))
                return new HttpUnauthorizedResult();

            var pendingTasks = _scheduledTaskService.GetPendingTasks();
			var prevTaskStart = DateTime.UtcNow;
			var count = 0;

            for (var i = 0; i < pendingTasks.Count; i++)
            {
				var task = pendingTasks[i];

				if (i > 0)
				{
					// Maybe a subsequent Sweep call or another machine in a webfarm executed 
					// successive tasks already.
					// To be able to determine this, we need to reload the entity from the database.
					// The TaskExecutor will exit when the task should be in running state then.
					_services.DbContext.ReloadEntity(task);
				}

				if (task.IsPending)
				{
					prevTaskStart = DateTime.UtcNow;
					_taskExecutor.Execute(task);
					count++;
				}
            }
            
            return Content("{0} of {1} pending tasks executed".FormatInvariant(count, pendingTasks.Count));
        }

        [HttpPost]
        public ActionResult Execute(int id /* taskId */)
        {
            if (!_taskScheduler.VerifyAuthToken(Request.Headers["X-AUTH-TOKEN"]))
                return new HttpUnauthorizedResult();

            var task = _scheduledTaskService.GetTaskById(id);
            if (task == null)
                return HttpNotFound();
	
            _taskExecutor.Execute(task, QueryString.Current.ToDictionary());

            return Content("Task '{0}' executed".FormatCurrent(task.Name));
        }

		public ContentResult Noop()
		{
			return Content("noop");
		}

	}
}