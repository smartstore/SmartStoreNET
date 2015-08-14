using System;
using System.Linq;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Reflection;
using SmartStore.Admin.Models.Directory;
using SmartStore.Admin.Models.Tasks;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Tasks;
using SmartStore.Core.Localization;
using SmartStore.Services.Configuration;
using SmartStore.Services.Directory;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Security;
using SmartStore.Services.Tasks;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using Telerik.Web.Mvc;
using SmartStore.Core.Async;
using Autofac;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using System.Collections.Generic;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public partial class ScheduleTaskController : AdminControllerBase
    {
		private readonly IScheduleTaskService _scheduleTaskService;
        private readonly ITaskScheduler _taskScheduler;
        private readonly IPermissionService _permissionService;
        private readonly IDateTimeHelper _dateTimeHelper;

		public ScheduleTaskController(
            IScheduleTaskService scheduleTaskService, 
            ITaskScheduler taskScheduler, 
            IPermissionService permissionService, 
            IDateTimeHelper dateTimeHelper)
        {
            this._scheduleTaskService = scheduleTaskService;
			this._taskScheduler = taskScheduler;
            this._permissionService = permissionService;
            this._dateTimeHelper = dateTimeHelper;
        }

		private bool IsTaskVisible(ScheduleTask task)
		{
			if (task.IsHidden)
				return false;

			var type = Type.GetType(task.Type);
			if (type != null)
			{
				return PluginManager.IsActivePluginAssembly(type.Assembly);
			}
			return false;
		}

        [NonAction]
        protected ScheduleTaskModel PrepareScheduleTaskModel(ScheduleTask task)
        {
			var now = DateTime.UtcNow;
			
			TimeSpan? dueIn = null;
			if (task.NextRunUtc.HasValue)
			{
				dueIn = task.NextRunUtc.Value - now;
			}

			var nextRunStr = "";
			bool isOverdue = false;
			if (dueIn.HasValue)
			{
				if (dueIn.Value.TotalSeconds > 0)
				{
					nextRunStr = "<span class='muted'>{0}</span>".FormatCurrent(dueIn.Value.Prettify());
				}
				else
				{
					nextRunStr = "<span class='text-success'><strong>{0}</strong></span>".FormatCurrent(T("Common.Waiting") + "...");
					isOverdue = true;
				}
			}

			var isRunning = task.IsRunning;

			var model = new ScheduleTaskModel
			{
				Id = task.Id,
				Name = task.Name,
				CronExpression = task.CronExpression,
				CronDescription = CronExpression.GetFriendlyDescription(task.CronExpression),
				Enabled = task.Enabled,
				StopOnError = task.StopOnError,
				LastStartUtc = task.LastStartUtc.HasValue ? task.LastStartUtc.Value.RelativeFormat(true, "f") : "",
				LastEndUtc = task.LastEndUtc.HasValue ? _dateTimeHelper.ConvertToUserTime(task.LastEndUtc.Value, DateTimeKind.Utc).ToString("G") : "",
				LastSuccessUtc = task.LastSuccessUtc.HasValue ? _dateTimeHelper.ConvertToUserTime(task.LastSuccessUtc.Value, DateTimeKind.Utc).ToString("G") : "",
				NextRunUtc = nextRunStr,
				LastError = task.LastError.EmptyNull(),
				IsRunning = isRunning,
				CancelUrl = isRunning ? Url.Action("CancelJob", new { id = task.Id }) : "",
				EditUrl = Url.Action("EditPopup", new { id = task.Id }),
				ProgressPercent = task.ProgressPercent,
				ProgressMessage = task.ProgressMessage,
				IsOverdue = isOverdue,
				Duration = ""
			};

			var span = TimeSpan.Zero;
			if (task.LastStartUtc.HasValue)
			{
				span = model.IsRunning ? now - task.LastStartUtc.Value : task.LastEndUtc.Value - task.LastStartUtc.Value;
				model.Duration = span.ToString("g");
			}

            return model;
        }

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageScheduleTasks))
                return AccessDeniedView();

            return View();
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult List(GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageScheduleTasks))
                return AccessDeniedView();
			
            var models = _scheduleTaskService.GetAllTasks(true)
				.Where(IsTaskVisible)
                .Select(PrepareScheduleTaskModel)
				//.OrderByDescending(x => x.IsRunning)
				//.ThenByDescending(x => x.IsOverdue)
				//.ThenBy(x => x.Seconds)
                .ToList();

            var model = new GridModel<ScheduleTaskModel>
            {
                Data = models,
                Total = models.Count
            };

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult TaskUpdate(ScheduleTaskModel model, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageScheduleTasks))
                return AccessDeniedView();

            if (!ModelState.IsValid)
            {
                //display the first model error
                var modelStateErrors = this.ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
				NotifyError(modelStateErrors.FirstOrDefault());
                return Content(modelStateErrors.FirstOrDefault());
            }

            var scheduleTask = _scheduleTaskService.GetTaskById(model.Id);
			if (scheduleTask == null)
			{
				NotifyError("Schedule task cannot be loaded");
				return Content("");
			}

			if (scheduleTask.IsRunning)
			{
				NotifyError(T("Admin.System.ScheduleTasks.UpdateLocked"));
				return Content("");
			}

            scheduleTask.Name = model.Name;
            scheduleTask.Enabled = model.Enabled;
            scheduleTask.StopOnError = model.StopOnError;

			if (model.Enabled)
			{
				scheduleTask.NextRunUtc = _scheduleTaskService.GetNextSchedule(scheduleTask);
			}
			else
			{
				scheduleTask.NextRunUtc = null;
			}

            _scheduleTaskService.UpdateTask(scheduleTask);

            return List(command);
        }

		[HttpPost]
		public ActionResult GetRunningTasks()
		{
			if (!_scheduleTaskService.HasRunningTasks())
				return Json(null);

			var runningTasks = from t in _scheduleTaskService.GetRunningTasks()
							   select new 
							   {
 								   id = t.Id,
								   percent = t.ProgressPercent,
								   message = t.ProgressMessage,
							   };

			return Json(runningTasks.ToArray());
		}

		public ActionResult RunJob(int id, string returnUrl = "")
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageScheduleTasks))
				return AccessDeniedView();

			returnUrl = returnUrl.NullEmpty() ?? Request.UrlReferrer.ToString();

            _taskScheduler.RunSingleTask(id);

            NotifyInfo(T("Admin.System.ScheduleTasks.RunNow.Progress"));
			return Redirect(returnUrl);
		}

		public ActionResult CancelJob(int id /* scheduleTaskId */, string returnUrl = "")
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageScheduleTasks))
				return AccessDeniedView();

			var cts = _taskScheduler.GetCancelTokenSourceFor(id);
			if (cts != null)
			{
				cts.Cancel();
				NotifyWarning(T("Admin.System.ScheduleTasks.CancellationRequested"));
			}

			returnUrl = returnUrl.NullEmpty() ?? Request.UrlReferrer.ToString();
			return Redirect(returnUrl);
		}

		public ActionResult EditPopup(int id /* taskId */)
		{
			var task = _scheduleTaskService.GetTaskById(id);
			var model = PrepareScheduleTaskModel(task);

			return PartialView(model);
		}

		[HttpPost]
		public ActionResult CronScheduleNextOccurrences(string expression)
		{
			try
			{
				var now = DateTime.Now;
				var model = CronExpression.GetFutureSchedules(expression, now, now.AddYears(1));
				ViewBag.Description = CronExpression.GetFriendlyDescription(expression);
				return PartialView(model);
			}
			catch (Exception ex)
			{
				ViewBag.CronScheduleParseError = ex.Message;
				return PartialView(Enumerable.Empty<DateTime>());
			}
		}

    }
}
