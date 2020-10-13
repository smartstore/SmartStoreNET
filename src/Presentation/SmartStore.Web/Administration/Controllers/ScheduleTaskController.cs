using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using SmartStore.Admin.Models.Tasks;
using SmartStore.Core.Async;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Tasks;
using SmartStore.Core.Security;
using SmartStore.Services.Tasks;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public partial class ScheduleTaskController : AdminControllerBase
    {
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly ITaskScheduler _taskScheduler;
        private readonly IAsyncState _asyncState;
        private readonly AdminModelHelper _adminModelHelper;
        private readonly AdminAreaSettings _adminAreaSettings;
        private readonly CommonSettings _commonSettings;

        public ScheduleTaskController(
            IScheduleTaskService scheduleTaskService,
            ITaskScheduler taskScheduler,
            IAsyncState asyncState,
            AdminModelHelper adminModelHelper,
            AdminAreaSettings adminAreaSettings,
            CommonSettings commonSettings)
        {
            _scheduleTaskService = scheduleTaskService;
            _taskScheduler = taskScheduler;
            _asyncState = asyncState;
            _adminModelHelper = adminModelHelper;
            _adminAreaSettings = adminAreaSettings;
            _commonSettings = commonSettings;
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

        [Permission(Permissions.System.ScheduleTask.Read)]
        public ActionResult List()
        {
            var models = new List<ScheduleTaskModel>();
            var tasks = _scheduleTaskService.GetAllTasks(true);
            var lastHistoryEntries = _scheduleTaskService.GetHistoryEntries(0, int.MaxValue, 0, true, true).ToDictionarySafe(x => x.ScheduleTaskId);

            foreach (var task in tasks.Where(x => x.IsVisible()))
            {
                lastHistoryEntries.TryGetValue(task.Id, out var lastEntry);
                var model = _adminModelHelper.CreateScheduleTaskModel(task, lastEntry);
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
            // We better not check permission here.
            var runningHistoryEntries = _scheduleTaskService.GetHistoryEntries(0, int.MaxValue, 0, true, true, true);
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
            // We better not check permission here.
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

        [Permission(Permissions.System.ScheduleTask.Execute)]
        public ActionResult RunJob(int id, string returnUrl = "")
        {
            var taskParams = new Dictionary<string, string>
            {
                { TaskExecutor.CurrentCustomerIdParamName, Services.WorkContext.CurrentCustomer.Id.ToString() },
                { TaskExecutor.CurrentStoreIdParamName, Services.StoreContext.CurrentStore.Id.ToString() }
            };

            _taskScheduler.RunSingleTask(id, taskParams);

            // The most tasks are completed rather quickly. Wait a while...
            Thread.Sleep(200);

            // ...check and return suitable notifications
            var lastEntry = _scheduleTaskService.GetLastHistoryEntryByTaskId(id);
            if (lastEntry != null)
            {
                if (lastEntry.IsRunning)
                {
                    NotifyInfo(GetTaskMessage(lastEntry.ScheduleTask, "Admin.System.ScheduleTasks.RunNow.Progress"));
                }
                else
                {
                    if (lastEntry.Error.HasValue())
                    {
                        NotifyError(lastEntry.Error);
                    }
                    else
                    {
                        NotifySuccess(GetTaskMessage(lastEntry.ScheduleTask, "Admin.System.ScheduleTasks.RunNow.Success"));
                    }
                }
            }

            return RedirectToReferrer(returnUrl);
        }

        [Permission(Permissions.System.ScheduleTask.Execute)]
        public ActionResult CancelJob(int id /* scheduleTaskId */, string returnUrl = "")
        {
            if (_asyncState.Cancel<ScheduleTask>(id.ToString()))
            {
                NotifyWarning(T("Admin.System.ScheduleTasks.CancellationRequested"));
            }

            return RedirectToReferrer(returnUrl);
        }

        [Permission(Permissions.System.ScheduleTask.Read)]
        public ActionResult Edit(int id /* taskId */, string returnUrl = null)
        {
            var model = _adminModelHelper.CreateScheduleTaskModel(id);
            if (model == null)
            {
                return HttpNotFound();
            }

            ViewBag.ReturnUrl = returnUrl;
            ViewBag.GridPageSize = _adminAreaSettings.GridPageSize;
            ViewBag.HistoryCleanupNote = T("Admin.System.ScheduleTasks.HistoryCleanupNote",
                _commonSettings.MaxNumberOfScheduleHistoryEntries,
                _commonSettings.MaxScheduleHistoryAgeInDays).Text;

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.System.ScheduleTask.Update)]
        public ActionResult Edit(ScheduleTaskModel model, bool continueEditing, string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var scheduleTask = _scheduleTaskService.GetTaskById(model.Id);
            if (scheduleTask == null)
            {
                NotifyError("Schedule task cannot be loaded");
                return RedirectToAction("Edit", new { id = model.Id, returnUrl });
            }

            scheduleTask.Name = model.Name;
            scheduleTask.Enabled = model.Enabled;
            scheduleTask.StopOnError = model.StopOnError;
            scheduleTask.CronExpression = model.CronExpression;
            scheduleTask.Priority = model.Priority;
            scheduleTask.NextRunUtc = model.Enabled
                ? _scheduleTaskService.GetNextSchedule(scheduleTask)
                : null;

            _scheduleTaskService.UpdateTask(scheduleTask);

            NotifySuccess(T("Admin.System.ScheduleTasks.UpdateSuccess"));

            if (continueEditing)
            {
                return RedirectToAction("Edit", new { id = model.Id, returnUrl });
            }
            else if (returnUrl.HasValue())
            {
                return RedirectToReferrer(returnUrl, () => RedirectToAction("List"));
            }

            return RedirectToAction("List");
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.System.ScheduleTask.Read)]
        public ActionResult HistoryList(GridCommand command, int taskId)
        {
            var gridModel = new GridModel<ScheduleTaskHistoryModel>();
            var history = _scheduleTaskService.GetHistoryEntries(command.Page - 1, command.PageSize, taskId);

            gridModel.Total = history.TotalCount;
            gridModel.Data = history.Select(x => _adminModelHelper.CreateScheduleTaskHistoryModel(x)).ToList();

            return new JsonResult
            {
                Data = gridModel
            };
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.System.ScheduleTask.Delete)]
        public ActionResult DeleteHistoryEntry(int id, GridCommand command)
        {
            var historyEntry = _scheduleTaskService.GetHistoryEntryById(id);
            if (historyEntry != null)
            {
                _scheduleTaskService.DeleteHistoryEntry(historyEntry);
            }

            return HistoryList(command, historyEntry.ScheduleTaskId);
        }

        [HttpPost]
        [Permission(Permissions.System.ScheduleTask.Read)]
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

            ViewBag.CanRead = Services.Permissions.Authorize(Permissions.System.ScheduleTask.Read);
            ViewBag.CanExecute = Services.Permissions.Authorize(Permissions.System.ScheduleTask.Execute);
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

            ViewBag.CanRead = Services.Permissions.Authorize(Permissions.System.ScheduleTask.Read);
            ViewBag.CanExecute = Services.Permissions.Authorize(Permissions.System.ScheduleTask.Execute);
            ViewBag.ReturnUrl = returnUrl;

            return PartialView("_MinimalTaskWidget", model);
        }
    }
}
