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
using System.Threading;

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

			var nextRunPretty = "";
			bool isOverdue = false;
			if (dueIn.HasValue)
			{
				if (dueIn.Value.TotalSeconds > 0)
				{
					nextRunPretty = dueIn.Value.Prettify();
				}
				else
				{
					nextRunPretty = T("Common.Waiting") + "...";
					isOverdue = true;
				}
			}

			var isRunning = task.IsRunning;
			var lastStartOn = task.LastStartUtc.HasValue ? _dateTimeHelper.ConvertToUserTime(task.LastStartUtc.Value, DateTimeKind.Utc) : (DateTime?)null;
			var lastEndOn = task.LastEndUtc.HasValue ? _dateTimeHelper.ConvertToUserTime(task.LastEndUtc.Value, DateTimeKind.Utc) : (DateTime?)null;
			var lastSuccessOn = task.LastSuccessUtc.HasValue ? _dateTimeHelper.ConvertToUserTime(task.LastSuccessUtc.Value, DateTimeKind.Utc) : (DateTime?)null;
			var nextRunOn = task.NextRunUtc.HasValue ? _dateTimeHelper.ConvertToUserTime(task.NextRunUtc.Value, DateTimeKind.Utc) : (DateTime?)null;

			var model = new ScheduleTaskModel
			{
				Id = task.Id,
				Name = task.Name,
				CronExpression = task.CronExpression,
				CronDescription = CronExpression.GetFriendlyDescription(task.CronExpression),
				Enabled = task.Enabled,
				StopOnError = task.StopOnError,
				LastStart = lastStartOn,
				LastStartPretty = task.LastStartUtc.HasValue ? task.LastStartUtc.Value.RelativeFormat(true, "f") : "",
				LastEnd = lastEndOn,
				LastEndPretty = lastEndOn.HasValue ? lastEndOn.Value.ToString("G") : "",
				LastSuccess = lastSuccessOn,
				LastSuccessPretty = lastSuccessOn.HasValue ? lastSuccessOn.Value.ToString("G") : "",
				NextRun = nextRunOn,
				NextRunPretty = nextRunPretty,
				LastError = task.LastError.EmptyNull(),
				IsRunning = isRunning,
				CancelUrl = Url.Action("CancelJob", new { id = task.Id }),
				ExecuteUrl = Url.Action("RunJob", new { id = task.Id }),
				EditUrl = Url.Action("Edit", new { id = task.Id }),
				ProgressPercent = task.ProgressPercent,
				ProgressMessage = task.ProgressMessage,
				IsOverdue = isOverdue,
				Duration = ""
			};

			var span = TimeSpan.Zero;
			if (task.LastStartUtc.HasValue)
			{
				span = model.IsRunning ? now - task.LastStartUtc.Value : task.LastEndUtc.Value - task.LastStartUtc.Value;
				if (span > TimeSpan.Zero)
				{
					model.Duration = span.ToString("g");
				}
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

			var model = _scheduleTaskService.GetAllTasks(true)
				.Where(IsTaskVisible)
				.Select(PrepareScheduleTaskModel)
				.ToList();

            return View(model);
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

		[HttpPost]
		public ActionResult GetTaskRunInfo(int id /* taskId */)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageScheduleTasks))
				return new HttpUnauthorizedResult();

			var task = _scheduleTaskService.GetTaskById(id);
			if (task == null)
			{
				return HttpNotFound();
			}

			var model = PrepareScheduleTaskModel(task);

			return Json(new 
			{
				lastRunHtml = this.RenderPartialViewToString("_LastRun", model),
				nextRunHtml = this.RenderPartialViewToString("_NextRun", model)
			});
		}

		public ActionResult RunJob(int id, string returnUrl = "")
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageScheduleTasks))
				return AccessDeniedView();

			returnUrl = returnUrl.NullEmpty() ?? Request.UrlReferrer.ToString();

            _taskScheduler.RunSingleTask(id);

			// The most tasks are completed rather quickly. Wait a while...
			var start = DateTime.UtcNow;
			Thread.Sleep(200);

			// ...check and return suitable notifications
			var task = _scheduleTaskService.GetTaskById(id);
			if (task != null)
			{
				if (task.IsRunning)
				{
					NotifyInfo(T("Admin.System.ScheduleTasks.RunNow.Progress"));
				}
				else
				{
					if (task.LastError.HasValue())
					{
						NotifyError(task.LastError);
					}
					else
					{
						NotifySuccess(T("Admin.System.ScheduleTasks.RunNow.Success"));
					}
				}
				var now = DateTime.UtcNow;
			}

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

		public ActionResult Edit(int id /* taskId */, string returnUrl = null)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageScheduleTasks))
				return AccessDeniedView();
			
			var task = _scheduleTaskService.GetTaskById(id);
			if (task == null)
			{
				return HttpNotFound();
			}

			var model = PrepareScheduleTaskModel(task);
			ViewBag.ReturnUrl = returnUrl;

			return View(model);
		}

		[HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
		[ValidateAntiForgeryToken]
		public ActionResult Edit(ScheduleTaskModel model, bool continueEditing, string returnUrl = null)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageScheduleTasks))
				return AccessDeniedView();

			ViewBag.ReturnUrl = returnUrl;

			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var reloadResult = RedirectToAction("Edit", new { id = model.Id, returnUrl = returnUrl });
			var returnResult = Redirect(returnUrl.NullEmpty() ?? Url.Action("List"));

			var scheduleTask = _scheduleTaskService.GetTaskById(model.Id);
			if (scheduleTask == null)
			{
				NotifyError("Schedule task cannot be loaded");
				return reloadResult;
			}

			scheduleTask.Name = model.Name;
			scheduleTask.Enabled = model.Enabled;
			scheduleTask.StopOnError = model.StopOnError;
			scheduleTask.CronExpression = model.CronExpression;

			if (model.Enabled)
			{
				scheduleTask.NextRunUtc = _scheduleTaskService.GetNextSchedule(scheduleTask);
			}
			else
			{
				scheduleTask.NextRunUtc = null;
			}

			_scheduleTaskService.UpdateTask(scheduleTask);

			NotifySuccess(T("Admin.System.ScheduleTasks.UpdateSuccess"));

			if (continueEditing)
				return reloadResult;

			return returnResult;
		}

		[HttpPost]
		public ActionResult FutureSchedules(string expression)
		{
			try
			{
				var now = DateTime.Now;
				var model = CronExpression.GetFutureSchedules(expression, now, now.AddYears(1), 20);
				ViewBag.Description = CronExpression.GetFriendlyDescription(expression);
				return PartialView(model);
			}
			catch (Exception ex)
			{
				ViewBag.CronScheduleParseError = ex.Message;
				return PartialView(Enumerable.Empty<DateTime>());
			}
		}

		[ChildActionOnly]
		public ActionResult MinimalTask(int taskId, string returnUrl /* mandatory on purpose */, bool cancellable = true)
		{
			var task = _scheduleTaskService.GetTaskById(taskId);
			if (task == null)
			{
				return Content("");
			}

			ViewBag.HasPermission = _permissionService.Authorize(StandardPermissionProvider.ManageScheduleTasks);
			ViewBag.ReturnUrl = returnUrl;
			ViewBag.Cancellable = cancellable;

			var model = PrepareScheduleTaskModel(task);

			return PartialView(model);
		}

		[HttpPost]
		public ActionResult GetMinimalTaskWidget(int taskId, string returnUrl /* mandatory on purpose */)
		{
			var task = _scheduleTaskService.GetTaskById(taskId);
			if (task == null)
			{
				return Content("");
			}

			ViewBag.HasPermission = _permissionService.Authorize(StandardPermissionProvider.ManageScheduleTasks);
			ViewBag.ReturnUrl = returnUrl;

			var model = PrepareScheduleTaskModel(task);

			return PartialView("_MinimalTaskWidget", model);
		}

    }
}
