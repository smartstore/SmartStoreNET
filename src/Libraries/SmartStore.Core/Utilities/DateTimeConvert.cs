using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace SmartStore.Utilities
{
    /// <summary>
    /// Converts between <see cref="DateTime"/>s and <see langword="string"/>s.
    /// </summary>
    /// <remarks>Accepted formats for parsing are "dd MMM yyyy HH:mm:ss.ff", "yyyy-MM-ddTHH:mm:ss", "dd MMM yyyy hh:mm tt", "dd MMM yyyy hh:mm:ss tt", "dd MMM yyyy HH:mm:ss", "dd MMM yyyy HH:mm" and "dd MMM yyyy".</remarks>
    public static class DateTimeConvert
    {
        #region Properties

        /// <summary>
        /// The default format used by <see cref="ToString(DateTime)"/> and <see cref="ToString(Nullable{DateTime})"/>.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1802:UseLiteralsWhereAppropriate")]
        public static readonly string DateTimeFormat = "dd MMM yyyy HH:mm:ss.ff";

        /// <summary>
        /// The supported formats used to parse strings.
        /// </summary>
        private static readonly string[] parseFormats = new string[] { DateTimeFormat, "s", "dd MMM yyyy hh:mm tt", "dd MMM yyyy hh:mm:ss tt", "dd MMM yyyy HH:mm:ss", "dd MMM yyyy HH:mm", "dd MMM yyyy" };

        #endregion

        #region Methods

        /// <summary>
        /// Converts the specified string representation of a date and time to its <see cref="DateTime"/> equivalent. 
        /// </summary>
        /// <remarks>
        /// Accepted formats for parsing are "dd MMM yyyy HH:mm:ss.ff", "yyyy-MM-ddTHH:mm:ss", "dd MMM yyyy hh:mm tt", "dd MMM yyyy hh:mm:ss tt", "dd MMM yyyy HH:mm:ss", "dd MMM yyyy HH:mm" and "dd MMM yyyy". <see cref="DateTime.ParseExact(string,string[],IFormatProvider,DateTimeStyles)"/> is used to attempt to parse <paramref name="s"/>.
        /// </remarks>
        /// <param name="s">A string containing a date and (optionally) time to convert.</param>
        /// <returns>A <see cref="DateTime"/> equivalent to the date and time contained in <paramref name="s"/>.</returns>
        /// <exception cref="FormatException"><paramref name="s"/> cannot be parsed.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="s"/> is a null reference.</exception>
        /// <exception cref="ArgumentException"><paramref name="s"/> is <see cref="string.Empty"/>.</exception>
        public static DateTime Parse(string date)
        {
            try
            {
                return DateTime.ParseExact(date, parseFormats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
            }
            catch (FormatException formatException)
            {
                throw new FormatException(string.Format("{0}. Value = {1}", formatException.Message, date), formatException);
            }
        }


        /// <summary>
        /// Converts the specified string representation of a date and time to its <see cref="Nullable"/> <see cref="DateTime"/> equivalent. 
        /// </summary>
        /// <param name="s">A string containing a date and (optionally) time to convert.</param>
        /// <returns><see langword="null"/> if <paramref name="s"/> is <see cref="string.IsNullOrEmpty"/>; otherwise the value returned from <see cref="Parse"/>.</returns>
        public static DateTime? ParseNullable(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }
            else
            {
                return Parse(s);
            }
        }


        /// <summary>
        /// Converts a <see cref="DateTime"/> to its equivalent string representation. 
        /// </summary>
        /// <param name="dateTime">The <see cref="DateTime"/> to convert.</param>
        /// <returns>A string representation <paramref name="dateTime"/>.</returns>
        public static string ToString(DateTime dateTime)
        {
            return dateTime.ToString(DateTimeFormat);
        }


        /// <summary>
        /// Converts a <see cref="Nullable"/> <see cref="DateTime"/> to its equivalent string representation. 
        /// </summary>
        /// <param name="dateTime">The <see cref="Nullable"/> <see cref="DateTime"/> to convert.</param>
        /// <returns><see langword="null"/> if A string representation <paramref name="dateTime"/>.</returns>
        public static string ToString(DateTime? dateTime)
        {
            if (dateTime.HasValue)
            {
                return ToString(dateTime.Value);
            }
            else
            {
                return string.Empty;
            }
        }

        #endregion
    }
}