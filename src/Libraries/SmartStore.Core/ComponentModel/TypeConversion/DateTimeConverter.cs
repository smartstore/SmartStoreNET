using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.ComponentModel
{
	[SuppressMessage("ReSharper", "CanBeReplacedWithTryCastAndCheckForNull")]
	public class DateTimeConverter : TypeConverterBase
	{
		public DateTimeConverter()
			: base(typeof(DateTime))
		{
		}

		public override bool CanConvertFrom(Type type)
		{
			return type == typeof(string)
				|| type == typeof(long)
				|| type == typeof(double)
				|| type == typeof(TimeSpan)
				|| base.CanConvertFrom(type);
		}

		public override bool CanConvertTo(Type type)
		{
			return type == typeof(string)
				|| type == typeof(long)
				|| type == typeof(double)
				|| type == typeof(DateTimeOffset) 
				|| type == typeof(TimeSpan)
				|| base.CanConvertTo(type);
		}

		public override object ConvertFrom(CultureInfo culture, object value)
		{
			if (value is TimeSpan)
			{
				var span = (TimeSpan)value;
				return new DateTime(span.Ticks);
			}

			if (value is string)
			{
				var str = (string)value;

				DateTime time;
				if (DateTime.TryParse(str, culture, DateTimeStyles.None, out time))
				{
					return time;
				}

				long lng;
				if (long.TryParse(str, NumberStyles.None, culture, out lng))
				{
					return lng.FromUnixTime();
				}

				double dbl;
				if (double.TryParse(str, NumberStyles.AllowDecimalPoint, culture, out dbl))
				{
					return DateTime.FromOADate(dbl);
				}
            }

			if (value is long)
			{
				return ((long)value).FromUnixTime();
			}

			if (value is double)
			{
				return DateTime.FromOADate((double)value);
			}

			return base.ConvertFrom(culture, value);
		}

		public override object ConvertTo(CultureInfo culture, string format, object value, Type to)
		{
			var time = (DateTime)value;

			if (to == typeof(DateTimeOffset))
			{
				return new DateTimeOffset(time);
			}

			if (to == typeof(TimeSpan))
			{
				return new TimeSpan(time.Ticks);
			}

			if (to == typeof(double))
			{
				return time.ToOADate();
			}

			if (to == typeof(long))
			{
				return time.ToUnixTime();
			}

			return base.ConvertTo(culture, format, value, to);
        }
	}
}
