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
                Priority = task.Priority,
                RunPerMachine = task.RunPerMachine,
                StopOnError = task.StopOnError,
                NextRunPretty = nextRunPretty,
                CancelUrl = _urlHelper.Action("CancelJob", "ScheduleTask", new { id = task.Id }),
                ExecuteUrl = _urlHelper.Action("RunJob", "ScheduleTask", new { id = task.Id }),
                EditUrl = _urlHelper.Action("Edit", "ScheduleTask", new { id = task.Id }),
                IsOverdue = isOverdue
            };

            if (task.NextRunUtc.HasValue)
            {
                model.NextRun = _dateTimeHelper.ConvertToUserTime(task.NextRunUtc.Value, DateTimeKind.Utc);
            }

            model.LastHistoryEntry = CreateScheduleTaskHistoryModel(lastEntry);

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

        /// <summary>
        /// Creates and prepares a schedule task history view model.
        /// </summary>
        /// <param name="historyEntry">Schedule task history.</param>
        /// <returns>Schedule task history model.</returns>
        public ScheduleTaskHistoryModel CreateScheduleTaskHistoryModel(ScheduleTaskHistory historyEntry)
        {
            if (historyEntry == null)
            {
                return new ScheduleTaskHistoryModel();
            }

            var model = new ScheduleTaskHistoryModel
            {
                Id = historyEntry.Id,
                ScheduleTaskId = historyEntry.ScheduleTaskId,
                IsRunning = historyEntry.IsRunning,
                Error = historyEntry.Error.EmptyNull(),
                ProgressPercent = historyEntry.ProgressPercent,
                ProgressMessage = historyEntry.ProgressMessage,
                StartedOn = _dateTimeHelper.ConvertToUserTime(historyEntry.StartedOnUtc, DateTimeKind.Utc),
                StartedOnPretty = historyEntry.StartedOnUtc.RelativeFormat(true, "f"),
                MachineName = historyEntry.MachineName
            };

            model.StartedOnString = model.StartedOn.ToString("g");

            if (historyEntry.FinishedOnUtc.HasValue)
            {
                model.FinishedOn = _dateTimeHelper.ConvertToUserTime(historyEntry.FinishedOnUtc.Value, DateTimeKind.Utc);
                model.FinishedOnString = model.FinishedOn.Value.ToString("g");
                model.FinishedOnPretty = historyEntry.FinishedOnUtc.Value.RelativeFormat(true, "f");
            }

            if (historyEntry.SucceededOnUtc.HasValue)
            {
                model.SucceededOn = _dateTimeHelper.ConvertToUserTime(historyEntry.SucceededOnUtc.Value, DateTimeKind.Utc);
                model.SucceededOnPretty = historyEntry.SucceededOnUtc.Value.ToNativeString("G");
            }

            var span = model.IsRunning
                ? DateTime.UtcNow - historyEntry.StartedOnUtc
                : (historyEntry.FinishedOnUtc ?? historyEntry.StartedOnUtc) - historyEntry.StartedOnUtc;

            if (span > TimeSpan.Zero)
            {
                model.Duration = span.ToString("g");
            }

            return model;
        }
    }
}