using System;
using System.Web.Mvc;
using SmartStore.Admin.Models.Tasks;
using SmartStore.Core.Domain.Tasks;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Tasks;
using SmartStore.Web.Framework;

namespace SmartStore.Admin.Extensions
{
	public static class ScheduleTaskExtensions
	{
		public static ScheduleTaskModel ToScheduleTaskModel(this ScheduleTask task, ILocalizationService localization, IDateTimeHelper dateTimeHelper, UrlHelper urlHelper)
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
					nextRunPretty = localization.GetResource("Common.Waiting") + "...";
					isOverdue = true;
				}
			}

			var isRunning = task.IsRunning;
			var lastStartOn = task.LastStartUtc.HasValue ? dateTimeHelper.ConvertToUserTime(task.LastStartUtc.Value, DateTimeKind.Utc) : (DateTime?)null;
			var lastEndOn = task.LastEndUtc.HasValue ? dateTimeHelper.ConvertToUserTime(task.LastEndUtc.Value, DateTimeKind.Utc) : (DateTime?)null;
			var lastSuccessOn = task.LastSuccessUtc.HasValue ? dateTimeHelper.ConvertToUserTime(task.LastSuccessUtc.Value, DateTimeKind.Utc) : (DateTime?)null;
			var nextRunOn = task.NextRunUtc.HasValue ? dateTimeHelper.ConvertToUserTime(task.NextRunUtc.Value, DateTimeKind.Utc) : (DateTime?)null;

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
				CancelUrl = urlHelper.Action("CancelJob", "ScheduleTask", new { id = task.Id }),
				ExecuteUrl = urlHelper.Action("RunJob", "ScheduleTask", new { id = task.Id }),
				EditUrl = urlHelper.Action("Edit", "ScheduleTask", new { id = task.Id }),
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
	}
}