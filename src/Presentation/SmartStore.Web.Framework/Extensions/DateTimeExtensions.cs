using System;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Localization;
using SmartStore.Services.Helpers;
using SmartStore.Web.Framework.Localization;

namespace SmartStore.Web.Framework
{
	public static class DateTimeExtensions
	{
        /// <summary>
        /// Relative formatting of DateTime (e.g. 2 hours ago, a month ago)
        /// </summary>
        /// <param name="source">Source (UTC format)</param>
        /// <returns>Formatted date and time string</returns>
        public static string RelativeFormat(this DateTime source)
        {
            return RelativeFormat(source, string.Empty);
        }

        /// <summary>
        /// Relative formatting of DateTime (e.g. 2 hours ago, a month ago)
        /// </summary>
        /// <param name="source">Source (UTC format)</param>
        /// <param name="defaultFormat">Default format string (in case relative formatting is not applied)</param>
        /// <returns>Formatted date and time string</returns>
        public static string RelativeFormat(this DateTime source, string defaultFormat)
        {
            return RelativeFormat(source, false, defaultFormat);
        }

        /// <summary>
        /// Relative formatting of DateTime (e.g. 2 hours ago, a month ago)
        /// </summary>
        /// <param name="source">Source (UTC format)</param>
        /// <param name="convertToUserTime">A value indicating whether we should convet DateTime instance to user local time (in case relative formatting is not applied)</param>
        /// <param name="defaultFormat">Default format string (in case relative formatting is not applied)</param>
        /// <returns>Formatted date and time string</returns>
        public static string RelativeFormat(this DateTime source, bool convertToUserTime, string defaultFormat)
        {
            string result = "";
			Localizer T = EngineContext.Current.Resolve<IText>().Get;
            
            var ts = new TimeSpan(DateTime.UtcNow.Ticks - source.Ticks);
            double delta = ts.TotalSeconds;

            if (delta > 0)
            {
                if (delta < 60) // 60 (seconds)
                {
					result = ts.Seconds == 1 ? T("Time.OneSecondAgo") : T("Time.SecondsAgo", ts.Seconds);
                }
                else if (delta < 120) //2 (minutes) * 60 (seconds)
                {
					result = T("Time.OneMinuteAgo");
                }
                else if (delta < 2700) // 45 (minutes) * 60 (seconds)
                {
					result = String.Format(T("Time.MinutesAgo"), ts.Minutes);
                }
                else if (delta < 5400) // 90 (minutes) * 60 (seconds)
                {
					result = T("Time.OneHourAgo");
                }
                else if (delta < 86400) // 24 (hours) * 60 (minutes) * 60 (seconds)
                {
                    int hours = ts.Hours;
                    if (hours == 1)
                        hours = 2;
					result = T("Time.HoursAgo", hours);
                }
                else if (delta < 172800) // 48 (hours) * 60 (minutes) * 60 (seconds)
                {
					result = T("Time.Yesterday");
                }
                else if (delta < 2592000) // 30 (days) * 24 (hours) * 60 (minutes) * 60 (seconds)
                {
					result = String.Format(T("Time.DaysAgo"), ts.Days);
                }
                else if (delta < 31104000) // 12 (months) * 30 (days) * 24 (hours) * 60 (minutes) * 60 (seconds)
                {
                    int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
					result = months <= 1 ? T("Time.OneMonthAgo") : T("Time.MonthsAgo", months);
                }
                else
                {
                    int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
					result = years <= 1 ? T("Time.OneYearAgo") : T("Time.YearsAgo", years);
                }
            }
            else
            {
                DateTime tmp1 = source;
                if (convertToUserTime)
                {
                    tmp1 = EngineContext.Current.Resolve<IDateTimeHelper>().ConvertToUserTime(tmp1, DateTimeKind.Utc);
                }
                //default formatting
                if (!String.IsNullOrEmpty(defaultFormat))
                {
                    result = tmp1.ToString(defaultFormat);
                }
                else
                {
                    result = tmp1.ToString();
                }
            }

            return result.ReplaceNativeDigits();
        }

		public static string Prettify(this TimeSpan ts)
		{
			Localizer T = EngineContext.Current.Resolve<IText>().Get;
			double seconds = ts.TotalSeconds;

			try
			{
				int secsTemp = Convert.ToInt32(seconds);
				string label = T("Time.SecondsAbbr");
				int remainder = 0;
				string remainderLabel = "";

				if (secsTemp > 59)
				{
					remainder = secsTemp % 60;
					secsTemp /= 60;
					label = T("Time.MinutesAbbr");
					remainderLabel = T("Time.SecondsAbbr");
				}

				if (secsTemp > 59)
				{
					remainder = secsTemp % 60;
					secsTemp /= 60;
					label = (secsTemp == 1) ? T("Time.HourAbbr") : T("Time.HoursAbbr");
					remainderLabel = T("Time.MinutesAbbr");
				}

				string result = null;

				if (remainder == 0)
				{
					result = string.Format("{0:#,##0.#} {1}", secsTemp, label);
				}
				else
				{
					result = string.Format("{0:#,##0} {1} {2} {3}", secsTemp, label, remainder, remainderLabel);
				}

				return result.ReplaceNativeDigits();
			}
			catch
			{
				return "(-)";
			}
		}
    }
}
