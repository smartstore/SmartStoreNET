using System;
using System.Globalization;
using SmartStore.Core.Email;

namespace SmartStore.ComponentModel
{
	public class EmailAddressConverter : TypeConverterBase
	{
		public EmailAddressConverter() : base(typeof(object))
		{
		}

		public override bool CanConvertFrom(Type type)
		{
			return type == typeof(string);
		}

		public override bool CanConvertTo(Type type)
		{
			return type == typeof(string);
		}

		public override object ConvertFrom(CultureInfo culture, object value)
		{
			if (value is string str && str.HasValue())
			{
				return new EmailAddress(str);
			}

			return base.ConvertFrom(culture, value);
		}

		public override object ConvertTo(CultureInfo culture, string format, object value, Type to)
		{
			if (to == typeof(string) && value is EmailAddress address)
			{
				return address.ToString();
			}

			return base.ConvertTo(culture, format, value, to);
		}
	}
}
