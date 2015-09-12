using System;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using SmartStore.Admin.Extensions;
using SmartStore.Admin.Models.Tasks;
using SmartStore.Core.Domain.Tasks;
using SmartStore.Core.Plugins;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Security;
using SmartStore.Services.Tasks;
using SmartStore.Web.Framework.Controllers;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public partial class ScheduleTaskController : AdminControllerBase
    {
		private readonly IScheduleTaskService _scheduleTaskService;
        private readonly ITaskScheduler _taskScheduler;
        private readonly IPermissionService _permissionService;
        private readonly IDateTimeHelper _dateTimeHelper;
		private readonly ILocalizationService _localizationService;

		public ScheduleTaskController(
            IScheduleTaskService scheduleTaskService, 
            ITaskScheduler taskScheduler, 
            IPermissionService permissionService, 
            IDateTimeHelper dateTimeHelper,
			ILocalizationService localizationService)
        {
            this._scheduleTaskService = scheduleTaskService;
			this._taskScheduler = taskScheduler;
            this._permissionService = permissionService;
            this._dateTimeHelper = dateTimeHelper;
			this._localizationService = localizationService;
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
				.Select(x => x.ToScheduleTaskModel(_localizationService, _dateTimeHelper, Url))
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

			var model = task.ToScheduleTaskModel(_localizationService, _dateTimeHelper, Url);

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

			var model = task.ToScheduleTaskModel(_localizationService, _dateTimeHelper, Url);
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

			var model = task.ToScheduleTaskModel(_localizationService, _dateTimeHelper, Url);

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

			var model = task.ToScheduleTaskModel(_localizationService, _dateTimeHelper, Url);

			return PartialView("_MinimalTaskWidget", model);
		}

    }
}
