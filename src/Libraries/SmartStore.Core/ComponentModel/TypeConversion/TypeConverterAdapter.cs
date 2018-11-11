using System;
using System.ComponentModel;
using System.Globalization;

namespace SmartStore.ComponentModel
{
	internal class TypeConverterAdapter : TypeConverterBase
	{
		private readonly TypeConverter _converter;

		public TypeConverterAdapter(TypeConverter converter)
			: base(typeof(object))
		{
			_converter = converter;
		}

		public override bool CanConvertFrom(Type type)
		{
			return _converter != null && _converter.CanConvertFrom(type);
		}

		public override bool CanConvertTo(Type type)
		{
			return _converter != null && _converter.CanConvertTo(type);
		}

		public override object ConvertFrom(CultureInfo culture, object value)
		{
			return _converter.ConvertFrom(null, culture, value);
		}

		public override object ConvertTo(CultureInfo culture, string format, object value, Type to)
		{
			return _converter.ConvertTo(null, culture, value, to);
		}
	}
}
