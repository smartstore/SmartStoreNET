using System;
using System.Web.Mvc;
using SmartStore.Admin.Models.Tasks;
using SmartStore.Core;
using SmartStore.Core.Domain.Tasks;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Tasks;
using SmartStore.Web.Framework;

namespace SmartStore.Admin.Extensions
{
    public static class ScheduleTaskExtensions
	{
        public static ScheduleTaskModel ToScheduleTaskModel(
            this ScheduleTask task,
            ILocalizationService localization,
            IDateTimeHelper dateTimeHelper,
            IApplicationEnvironment env,
            UrlHelper urlHelper)
		{
            Guard.NotNull(task, nameof(task));
            Guard.NotNull(localization, nameof(localization));
            Guard.NotNull(dateTimeHelper, nameof(dateTimeHelper));
            Guard.NotNull(env, nameof(env));
            Guard.NotNull(urlHelper, nameof(urlHelper));

            var now = DateTime.UtcNow;
            var nextRunPretty = string.Empty;
			var isOverdue = false;
            var runningEntry = task.GetRunningHistoryEntry(env.MachineName);

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
					nextRunPretty = localization.GetResource("Common.Waiting") + "...";
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
				CancelUrl = urlHelper.Action("CancelJob", "ScheduleTask", new { id = task.Id }),
				ExecuteUrl = urlHelper.Action("RunJob", "ScheduleTask", new { id = task.Id }),
				EditUrl = urlHelper.Action("Edit", "ScheduleTask", new { id = task.Id }),
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
                model.NextRun = dateTimeHelper.ConvertToUserTime(task.NextRunUtc.Value, DateTimeKind.Utc);
            }

            if (runningEntry != null)
            {
                model.LastStart = dateTimeHelper.ConvertToUserTime(runningEntry.StartedOnUtc, DateTimeKind.Utc);
                model.LastStartPretty = runningEntry.StartedOnUtc.RelativeFormat(true, "f");

                if (runningEntry.FinishedOnUtc.HasValue)
                {
                    model.LastEnd = dateTimeHelper.ConvertToUserTime(runningEntry.FinishedOnUtc.Value, DateTimeKind.Utc);
                    model.LastEndPretty = runningEntry.FinishedOnUtc.Value.ToNativeString("G");
                }
                if (runningEntry.SucceededOnUtc.HasValue)
                {
                    model.LastSuccess = dateTimeHelper.ConvertToUserTime(runningEntry.SucceededOnUtc.Value, DateTimeKind.Utc);
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
	}
}