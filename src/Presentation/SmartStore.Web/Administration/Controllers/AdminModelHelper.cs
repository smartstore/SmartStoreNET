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
        /// <param name="lastEntry">Last task history entry.</param>
        /// <returns>Schedule task model.</returns>
        public ScheduleTaskModel CreateScheduleTaskModel(ScheduleTask task, ScheduleTaskHistory lastEntry)
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
                RunPerMachine = task.RunPerMachine,
                StopOnError = task.StopOnError,
                NextRunPretty = nextRunPretty,
                IsRunning = lastEntry?.IsRunning ?? false,
                LastError = lastEntry?.Error.EmptyNull(),
                ProgressPercent = lastEntry?.ProgressPercent,
                ProgressMessage = lastEntry?.ProgressMessage,
                CancelUrl = _urlHelper.Action("CancelJob", "ScheduleTask", new { id = task.Id }),
                ExecuteUrl = _urlHelper.Action("RunJob", "ScheduleTask", new { id = task.Id }),
                EditUrl = _urlHelper.Action("Edit", "ScheduleTask", new { id = task.Id }),
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

            if (lastEntry != null)
            {
                model.LastStart = _dateTimeHelper.ConvertToUserTime(lastEntry.StartedOnUtc, DateTimeKind.Utc);
                model.LastStartPretty = lastEntry.StartedOnUtc.RelativeFormat(true, "f");

                if (lastEntry.FinishedOnUtc.HasValue)
                {
                    model.LastEnd = _dateTimeHelper.ConvertToUserTime(lastEntry.FinishedOnUtc.Value, DateTimeKind.Utc);
                    model.LastEndPretty = lastEntry.FinishedOnUtc.Value.ToNativeString("G");
                }
                if (lastEntry.SucceededOnUtc.HasValue)
                {
                    model.LastSuccess = _dateTimeHelper.ConvertToUserTime(lastEntry.SucceededOnUtc.Value, DateTimeKind.Utc);
                    model.LastSuccessPretty = lastEntry.SucceededOnUtc.Value.ToNativeString("G");
                }

                var span = model.IsRunning
                    ? now - lastEntry.StartedOnUtc
                    : (lastEntry.FinishedOnUtc ?? lastEntry.StartedOnUtc) - lastEntry.StartedOnUtc;
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
            var lastEntry = _scheduleTaskService.Value.GetLastHistoryEntryByTaskId(taskId);
            var task = lastEntry?.ScheduleTask ?? _scheduleTaskService.Value.GetTaskById(taskId);

            return CreateScheduleTaskModel(task, lastEntry);
        }
    }
}