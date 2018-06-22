using System;
using System.Web.Mvc;
using SmartStore.Admin.Models.Tasks;
using SmartStore.Core.Domain.Tasks;
using SmartStore.Core.Localization;
using SmartStore.Services.Helpers;
using SmartStore.Services.Tasks;
using SmartStore.Web.Framework;

namespace SmartStore.Admin.Controllers
{
    public partial class AdminModelHelper
    {
        protected readonly Lazy<IScheduleTaskService> _scheduleTaskService;
        protected readonly IDateTimeHelper _dateTimeHelper;
        protected readonly UrlHelper _urlHelper;

        public AdminModelHelper(
            Lazy<IScheduleTaskService> scheduleTaskService,
            IDateTimeHelper dateTimeHelper,
            UrlHelper urlHelper)
        {
            _scheduleTaskService = scheduleTaskService;
            _dateTimeHelper = dateTimeHelper;
            _urlHelper = urlHelper;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        /// <summary>
        /// Creates and prepares a schedule task view model.
        /// </summary>
        /// <param name="task">Schedule task.</param>
        /// <param name="runningEntry">Running task history entry.</param>
        /// <returns>Schedule task model.</returns>
        public ScheduleTaskModel CreateScheduleTaskModel(ScheduleTask task, ScheduleTaskHistory runningEntry)
        {
            if (task == null)
            {
                return null;
            }

            var now = DateTime.UtcNow;
            var nextRunPretty = string.Empty;
            var isOverdue = false;

            TimeSpan? dueIn = task.NextRunUtc.HasValue
                ? task.NextRunUtc.Value - now
                : (TimeSpan?)null;

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

            var model = new ScheduleTaskModel
            {
                Id = task.Id,
                Name = task.Name,
                CronExpression = task.CronExpression,
                CronDescription = CronExpression.GetFriendlyDescription(task.CronExpression),
                Enabled = task.Enabled,
                StopOnError = task.StopOnError,
                NextRunPretty = nextRunPretty,
                LastError = runningEntry?.Error.EmptyNull(),
                IsRunning = runningEntry != null,
                CancelUrl = _urlHelper.Action("CancelJob", "ScheduleTask", new { id = task.Id }),
                ExecuteUrl = _urlHelper.Action("RunJob", "ScheduleTask", new { id = task.Id }),
                EditUrl = _urlHelper.Action("Edit", "ScheduleTask", new { id = task.Id }),
                ProgressPercent = runningEntry?.ProgressPercent,
                ProgressMessage = runningEntry?.ProgressMessage,
                IsOverdue = isOverdue,
                Duration = string.Empty,
                LastStartPretty = string.Empty,
                LastEndPretty = string.Empty,
                LastSuccessPretty = string.Empty
            };

            if (task.NextRunUtc.HasValue)
            {
                model.NextRun = _dateTimeHelper.ConvertToUserTime(task.NextRunUtc.Value, DateTimeKind.Utc);
            }

            if (runningEntry != null)
            {
                model.LastStart = _dateTimeHelper.ConvertToUserTime(runningEntry.StartedOnUtc, DateTimeKind.Utc);
                model.LastStartPretty = runningEntry.StartedOnUtc.RelativeFormat(true, "f");

                if (runningEntry.FinishedOnUtc.HasValue)
                {
                    model.LastEnd = _dateTimeHelper.ConvertToUserTime(runningEntry.FinishedOnUtc.Value, DateTimeKind.Utc);
                    model.LastEndPretty = runningEntry.FinishedOnUtc.Value.ToNativeString("G");
                }
                if (runningEntry.SucceededOnUtc.HasValue)
                {
                    model.LastSuccess = _dateTimeHelper.ConvertToUserTime(runningEntry.SucceededOnUtc.Value, DateTimeKind.Utc);
                    model.LastSuccessPretty = runningEntry.SucceededOnUtc.Value.ToNativeString("G");
                }

                var span = model.IsRunning
                    ? now - runningEntry.StartedOnUtc
                    : (runningEntry.FinishedOnUtc ?? runningEntry.StartedOnUtc) - runningEntry.StartedOnUtc;
                if (span > TimeSpan.Zero)
                {
                    model.Duration = span.ToString("g");
                }
            }

            return model;
        }

        /// <summary>
        /// Creates and prepares a schedule task view model.
        /// </summary>
        /// <param name="taskId">Schedule task identifier.</param>
        /// <returns>Schedule task model.</returns>
        public ScheduleTaskModel CreateScheduleTaskModel(int taskId)
        {
            var runningEntry = _scheduleTaskService.Value.GetRunningHistoryEntryByTaskId(taskId);
            var task = runningEntry?.ScheduleTask ?? _scheduleTaskService.Value.GetTaskById(taskId);

            return CreateScheduleTaskModel(task, runningEntry);
        }
    }
}