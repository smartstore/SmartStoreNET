using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CronExpressionDescriptor;
using NCrontab;

namespace SmartStore.Services.Tasks
{
	public static class CronExpression
	{
		public static bool IsValid(string expression)
		{
			try
			{
				CrontabSchedule.Parse(expression);
				return true;
			}
			catch { }

			return false;
		}

		public static DateTime GetNextSchedule(string expression, DateTime baseTime)
		{
			return GetFutureSchedules(expression, baseTime, 1).FirstOrDefault();
		}

		public static DateTime GetNextSchedule(string expression, DateTime baseTime, DateTime endTime)
		{
			return GetFutureSchedules(expression, baseTime, endTime, 1).FirstOrDefault();
		}

		public static IEnumerable<DateTime> GetFutureSchedules(string expression, DateTime baseTime, int max = 10)
		{
			return GetFutureSchedules(expression, baseTime, DateTime.MaxValue);
		}

		public static IEnumerable<DateTime> GetFutureSchedules(string expression, DateTime baseTime, DateTime endTime, int max = 10)
		{
			Guard.NotEmpty(expression, nameof(expression));

			var schedule = CrontabSchedule.Parse(expression);
			return schedule.GetNextOccurrences(baseTime, endTime).Take(max);
		}

		public static string GetFriendlyDescription(string expression)
		{
			var options = new Options 
			{ 
				DayOfWeekStartIndexZero = true, 
				ThrowExceptionOnParseError = true, 
				Verbose = false,
				Use24HourTimeFormat = Thread.CurrentThread.CurrentUICulture.DateTimeFormat.AMDesignator.IsEmpty()
			};

			if (expression.HasValue())
			{
				try
				{
					return ExpressionDescriptor.GetDescription(expression, options);
				}
				catch { }
			}

			return "?";
		}

	}

}
