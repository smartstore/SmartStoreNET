using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using SmartStore.Admin.Models.Tasks;
using SmartStore.Core.Async;
using SmartStore.Core.Domain.Tasks;
using SmartStore.Services.Security;
using SmartStore.Services.Tasks;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Security;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public partial class ScheduleTaskController : AdminControllerBase
    {
		private readonly IScheduleTaskService _scheduleTaskService;
        private readonly ITaskScheduler _taskScheduler;
		private readonly IAsyncState _asyncState;
        private readonly AdminModelHelper _adminModelHelper;

        public ScheduleTaskController(
            IScheduleTaskService scheduleTaskService, 
            ITaskScheduler taskScheduler, 
			IAsyncState asyncState,
            AdminModelHelper adminModelHelper)
        {
            _scheduleTaskService = scheduleTaskService;
			_taskScheduler = taskScheduler;
			_asyncState = asyncState;
            _adminModelHelper = adminModelHelper;
        }

		private string GetTaskMessage(ScheduleTask task, string resourceKey)
		{
			string message = null;

			var taskClassName = task.Type
				.SplitSafe(",")
				.SafeGet(0)
				.SplitSafe(".")
				.LastOrDefault();

			if (taskClassName.HasValue())
			{
				message = Services.Localization.GetResource(string.Concat(resourceKey, ".", taskClassName), logIfNotFound: false, returnEmptyIfNotFound: true);
			}

			if (message.IsEmpty())
			{
				message = T(resourceKey);
			}

			return message;
		}

		public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageScheduleTasks))
                return AccessDeniedView();

            var models = new List<ScheduleTaskModel>();
            var tasks = _scheduleTaskService.GetAllTasks(true);
            var runningHistoryEntries = _scheduleTaskService.GetRunningHistoryEntries().ToDictionarySafe(x => x.ScheduleTaskId);

            foreach (var task in tasks.Where(x => x.IsVisible()))
            {
                runningHistoryEntries.TryGetValue(task.Id, out var runningEntry);
                var model = _adminModelHelper.CreateScheduleTaskModel(task, runningEntry);
                if (model != null)
                {
                    models.Add(model);
                }
            }

            return View(models);
        }

		[HttpPost]
		public ActionResult GetRunningTasks()
		{
            var runningHistoryEntries = _scheduleTaskService.GetRunningHistoryEntries();
            if (!runningHistoryEntries.Any())
            {
                return Json(new EmptyResult());
            }

            var models = runningHistoryEntries
                .Select(x => new
                {
                    id = x.ScheduleTaskId,
                    percent = x.ProgressPercent,
                    message = x.ProgressMessage
                })
                .ToArray();

            return Json(models);
        }

        [HttpPost]
		public ActionResult GetTaskRunInfo(int id /* taskId */)
		{
			if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageScheduleTasks))
				return new HttpUnauthorizedResult();

            var model = _adminModelHelper.CreateScheduleTaskModel(id);
            if (model == null)
            {
                return HttpNotFound();
            }

			return Json(new 
			{
				lastRunHtml = this.RenderPartialViewToString("~/Administration/Views/ScheduleTask/_LastRun.cshtml", model),
				nextRunHtml = this.RenderPartialViewToString("~/Administration/Views/ScheduleTask/_NextRun.cshtml", model)
			});
		}

		public ActionResult RunJob(int id, string returnUrl = "")
		{
			if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageScheduleTasks))
				return AccessDeniedView();

			var taskParams = new Dictionary<string, string>
			{
				{ TaskExecutor.CurrentCustomerIdParamName, Services.WorkContext.CurrentCustomer.Id.ToString() },
				{ TaskExecutor.CurrentStoreIdParamName, Services.StoreContext.CurrentStore.Id.ToString() }
			};

			_taskScheduler.RunSingleTask(id, taskParams);

			// The most tasks are completed rather quickly. Wait a while...
			Thread.Sleep(200);

            // ...check and return suitable notifications
            var runningEntry = _scheduleTaskService.GetRunningHistoryEntryByTaskId(id);
            if (runningEntry != null)
            {
                NotifyInfo(GetTaskMessage(runningEntry.ScheduleTask, "Admin.System.ScheduleTasks.RunNow.Progress"));
            }
            else
            {
                if (runningEntry.Error.HasValue())
                {
                    NotifyError(runningEntry.Error);
                }
                else
                {
                    NotifySuccess(GetTaskMessage(runningEntry.ScheduleTask , "Admin.System.ScheduleTasks.RunNow.Success"));
                }
            }

			return RedirectToReferrer(returnUrl);
		}

		public ActionResult CancelJob(int id /* scheduleTaskId */, string returnUrl = "")
		{
			if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageScheduleTasks))
				return AccessDeniedView();
	
			if (_asyncState.Cancel<ScheduleTask>(id.ToString()))
			{
				NotifyWarning(T("Admin.System.ScheduleTasks.CancellationRequested"));
			}

			return RedirectToReferrer(returnUrl);
		}

		public ActionResult Edit(int id /* taskId */, string returnUrl = null)
		{
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageScheduleTasks))
                return AccessDeniedView();

            var model = _adminModelHelper.CreateScheduleTaskModel(id);
            if (model == null)
            {
                return HttpNotFound();
            }

            ViewBag.ReturnUrl = returnUrl;

            return View(model);
		}

		[HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
		[ValidateAntiForgeryToken]
		public ActionResult Edit(ScheduleTaskModel model, bool continueEditing, string returnUrl = null)
		{
			if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageScheduleTasks))
				return AccessDeniedView();

			ViewBag.ReturnUrl = returnUrl;

			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var reloadResult = RedirectToAction("Edit", new { id = model.Id, returnUrl = returnUrl });
			var returnResult = returnUrl.HasValue() ? (ActionResult)Redirect(returnUrl) : (ActionResult)RedirectToAction("List");

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
			scheduleTask.NextRunUtc = model.Enabled 
				? _scheduleTaskService.GetNextSchedule(scheduleTask) 
				: null;

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
		public ActionResult MinimalTask(int taskId, string returnUrl /* mandatory on purpose */, bool cancellable = true, bool reloadPage = false)
		{
            var model = _adminModelHelper.CreateScheduleTaskModel(taskId);
            if (model == null)
            {
                return new EmptyResult();
            }

            ViewBag.HasPermission = Services.Permissions.Authorize(StandardPermissionProvider.ManageScheduleTasks);
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.Cancellable = cancellable;
            ViewBag.ReloadPage = reloadPage;

            return PartialView(model);
		}

		[HttpPost]
		public ActionResult GetMinimalTaskWidget(int taskId, string returnUrl /* mandatory on purpose */)
		{
            var model = _adminModelHelper.CreateScheduleTaskModel(taskId);
            if (model == null)
            {
                return new EmptyResult();
            }

            ViewBag.HasPermission = Services.Permissions.Authorize(StandardPermissionProvider.ManageScheduleTasks);
            ViewBag.ReturnUrl = returnUrl;

            return PartialView("_MinimalTaskWidget", model);
		}
    }
}
