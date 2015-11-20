using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using SmartStore.ComponentModel;

namespace SmartStore.Services.DataExchange
{
	public class ColumnMapConverter : TypeConverterBase
	{
		public ColumnMapConverter()
			: base(typeof(object))
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
			if (value is string)
			{
				var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>((string)value);
				var map = new ColumnMap();

				foreach (var kvp in dict)
				{
					map.AddMapping(kvp.Key, kvp.Value);
				}

				return map;
            }

			return base.ConvertFrom(culture, value);
		}

		public override object ConvertTo(CultureInfo culture, string format, object value, Type to)
		{
			if (to == typeof(string))
			{
				if (value is ColumnMap)
				{
					return JsonConvert.SerializeObject(((ColumnMap)value).Mappings);
				}
				else
				{
					return string.Empty;
				}
			}

			return base.ConvertTo(culture, format, value, to);
		}
	}
}
