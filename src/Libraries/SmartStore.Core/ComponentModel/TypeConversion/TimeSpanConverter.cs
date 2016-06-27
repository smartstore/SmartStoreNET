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
	public class TimeSpanConverter : TypeConverterBase
	{
		public TimeSpanConverter()
			: base(typeof(TimeSpan))
		{
		}

		public override bool CanConvertFrom(Type type)
		{
			return type == typeof(string)
				|| type == typeof(DateTime)
				|| base.CanConvertFrom(type);
		}

		public override object ConvertFrom(CultureInfo culture, object value)
		{
			if (value is DateTime)
			{
				var time = (DateTime)value;
				return new TimeSpan(time.Ticks);
			}

			if (value is string)
			{
				var str = (string)value;

				TimeSpan span;
				if (TimeSpan.TryParse(str, culture, out span))
				{
					return span;
				}

				long lng;
				if (long.TryParse(str, NumberStyles.None, culture, out lng))
				{
					return new TimeSpan(lng.FromUnixTime().Ticks);
				}

				double dbl;
				if (double.TryParse(str, NumberStyles.None, culture, out dbl))
				{
					return new TimeSpan(DateTime.FromOADate(dbl).Ticks);
				}
			}

			try
			{
				return (TimeSpan)System.Convert.ChangeType(value, typeof(TimeSpan), culture);
			}
			catch { }

			return base.ConvertFrom(culture, value);
		}
	}
}
