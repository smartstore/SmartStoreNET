using System;
using System.Xml;
using System.Globalization;
using TimeZone = System.TimeZoneInfo;
using System.Text.RegularExpressions;

namespace SmartStore
{
    public static class DateTimeExtensions
    {
		public static readonly DateTime BeginOfEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Converts a nullable date/time value to UTC.
        /// </summary>
        /// <param name="value">The nullable date/time</param>
        /// <returns>The nullable date/time in UTC</returns>
        public static DateTime? ToUniversalTime(this DateTime? value)
        {
            return value.HasValue ? value.Value.ToUniversalTime() : (DateTime?)null;
        }

        /// <summary>
        /// Converts a nullable UTC date/time value to local time.
        /// </summary>
        /// <param name="value">The nullable UTC date/time</param>
        /// <returns>The nullable UTC date/time as local time</returns>
        public static DateTime? ToLocalTime(this DateTime? value)
        {
            return value.HasValue ? value.Value.ToLocalTime() : (DateTime?)null;
        }


        /// <summary>
        /// Returns a date that is rounded to the next even hour above the given
        /// date.
        /// <p>
        /// For example an input date with a time of 08:13:54 would result in a date
        /// with the time of 09:00:00. If the date's time is in the 23rd hour, the
        /// date's 'day' will be promoted, and the time will be set to 00:00:00.
        /// </p>
        /// </summary>
        /// <param name="value">the Date to round, if <see langword="null" /> the current time will
        /// be used</param>
        /// <returns>the new rounded date</returns>
        public static DateTime GetEvenHourDate(this DateTime? value)
        {
            if (!value.HasValue)
            {
                value = DateTime.UtcNow;
            }
            DateTime d = value.Value.AddHours(1);
            return new DateTime(d.Year, d.Month, d.Day, d.Hour, 0, 0);
        }

        /// <summary>
        /// Returns a date that is rounded to the next even minute above the given
        /// date.
        /// <p>
        /// For example an input date with a time of 08:13:54 would result in a date
        /// with the time of 08:14:00. If the date's time is in the 59th minute,
        /// then the hour (and possibly the day) will be promoted.
        /// </p>
        /// </summary>
        /// <param name="value">The Date to round, if <see langword="null" /> the current time will  be used</param>
        /// <returns>The new rounded date</returns>
        public static DateTime GetEvenMinuteDate(this DateTime? value)
        {
            if (!value.HasValue)
            {
                value = DateTime.UtcNow;
            }

            DateTime d = value.Value;
            d = d.AddMinutes(1);
            return new DateTime(d.Year, d.Month, d.Day, d.Hour, d.Minute, 0);
        }

        /// <summary>
        /// Returns a date that is rounded to the previous even minute below the
        /// given date.
        /// <p>
        /// For example an input date with a time of 08:13:54 would result in a date
        /// with the time of 08:13:00.
        /// </p>
        /// </summary>
        /// <param name="value">the Date to round, if <see langword="null" /> the current time will
        /// be used</param>
        /// <returns>the new rounded date</returns>
        public static DateTime GetEvenMinuteDateBefore(this DateTime? value)
        {
            if (!value.HasValue)
            {
                value = DateTime.UtcNow;
            }

            DateTime d = value.Value;
            return new DateTime(d.Year, d.Month, d.Day, d.Hour, d.Minute, 0);
        }

        public static long ToJavaScriptTicks(this DateTime value)
        {
            DateTimeOffset utcDateTime = value.ToUniversalTime();
			long javaScriptTicks = (utcDateTime.Ticks - BeginOfEpoch.Ticks) / (long)10000;
            return javaScriptTicks;
        }

        /// <summary>
        /// Get the first day of the month for
        /// any full date submitted
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DateTime GetFirstDayOfMonth(this DateTime value)
        {
            DateTime dtFrom = value;
            dtFrom = dtFrom.AddDays(-(dtFrom.Day - 1));
            return dtFrom;
        }

        /// <summary>
        /// Get the last day of the month for any
        /// full date
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DateTime GetLastDayOfMonth(this DateTime value)
        {
            DateTime dtTo = value;
            dtTo = dtTo.AddMonths(1);
            dtTo = dtTo.AddDays(-(dtTo.Day));
            return dtTo;
        }

		public static DateTime ToEndOfTheDay(this DateTime value)
		{
			if (value != null)
				return new DateTime(value.Year, value.Month, value.Day, 23, 59, 59);
			return value;
		}

		public static DateTime? ToEndOfTheDay(this DateTime? value)
		{
			return (value.HasValue ? value.Value.ToEndOfTheDay() : value);
		}

		/// <summary>
		/// Epoch time. Number of seconds since midnight (UTC) on 1st January 1970.
		/// </summary>
		public static long ToUnixTime(this DateTime value)
		{
			return Convert.ToInt64((value.ToUniversalTime() - BeginOfEpoch).TotalSeconds);
		}

		/// <summary>
		/// UTC date based on number of seconds since midnight (UTC) on 1st January 1970.
		/// </summary>
		public static DateTime FromUnixTime(this long unixTime)
		{
			return BeginOfEpoch.AddSeconds(unixTime);
		}

		/// <summary>
		/// Converts a DateTime to a string with native digits
		/// </summary>
		public static string ToNativeString(this DateTime value)
		{
			return value.ToNativeString(null, null);
		}

		/// <summary>
		/// Converts a DateTime to a string with native digits
		/// </summary>
		public static string ToNativeString(this DateTime value, IFormatProvider provider)
		{
			return value.ToNativeString(null, provider);
		}

		/// <summary>
		/// Converts a DateTime to a string with native digits
		/// </summary>
		public static string ToNativeString(this DateTime value, string format)
		{
			return value.ToNativeString(format, null);
		}

		/// <summary>
		/// Converts a DateTime to a string with native digits
		/// </summary>
		public static string ToNativeString(this DateTime value, string format, IFormatProvider provider)
		{
			provider = provider ?? CultureInfo.CurrentCulture;
			return value.ToString(format, provider).ReplaceNativeDigits(provider);
		}
    }

}
