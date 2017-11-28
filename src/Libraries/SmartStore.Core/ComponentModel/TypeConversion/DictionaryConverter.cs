using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Web.Routing;
using SmartStore.Utilities;

namespace SmartStore.ComponentModel
{
	public class DictionaryTypeConverter<T> : TypeConverterBase where T : IDictionary<string, object>
	{
		public DictionaryTypeConverter()
			: base(typeof(object))
		{
		}

		public override bool CanConvertFrom(Type type)
		{
			return !type.IsPredefinedType() && type.IsClass;
		}

		public override bool CanConvertTo(Type type)
		{
			return !type.IsPredefinedType() && DictionaryConverter.CanCreateType(type);
		}

		public override object ConvertFrom(CultureInfo culture, object value)
		{
			// Obj > Dict
			var dict = CommonHelper.ObjectToDictionary(value);
			var to = typeof(T);

			if (to == typeof(RouteValueDictionary))
			{
				return new RouteValueDictionary(dict);
			}
			if (to == typeof(Dictionary<string, object>))
			{
				return (Dictionary<string, object>)dict;
			}
			else if (to == typeof(ExpandoObject))
			{
				var expando = new ExpandoObject();
				expando.Merge(dict);
				return expando;
			}
			else
			{
				return dict;
			}
		}

		public override object ConvertTo(CultureInfo culture, string format, object value, Type to)
		{
			// Dict > Obj
			if (value is IDictionary<string, object>  dict)
			{
				return DictionaryConverter.CreateAndPopulate(to, dict, out var problems);
			}

			return base.ConvertTo(culture, format, value, to);
		}
	}
}
