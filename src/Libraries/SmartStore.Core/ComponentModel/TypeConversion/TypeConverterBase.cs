using System;
using System.ComponentModel;
using System.Globalization;

namespace SmartStore.ComponentModel
{
	public abstract class TypeConverterBase : ITypeConverter
	{
		private readonly Lazy<TypeConverter> _systemConverter;
		private readonly Type _type; 

		protected TypeConverterBase(Type type)
		{
			Guard.NotNull(type, nameof(type));

			_type = type;
			_systemConverter = new Lazy<TypeConverter>(() => TypeDescriptor.GetConverter(type), true);
		}

		public TypeConverter SystemConverter
		{
			get
			{
				if (_type == typeof(object))
				{
					return null;
				}

				return _systemConverter.Value;
			}
		}

		public virtual bool CanConvertFrom(Type type)
		{
			return SystemConverter != null && SystemConverter.CanConvertFrom(type);
		}

		public virtual bool CanConvertTo(Type type)
		{
			return type == typeof(string) || (SystemConverter != null && SystemConverter.CanConvertTo(type));
		}

		public virtual object ConvertFrom(CultureInfo culture, object value)
		{
			if (SystemConverter != null)
			{
				return SystemConverter.ConvertFrom(null, culture, value);
			}

			throw Error.InvalidCast(value.GetType(), _type);
		}

		public virtual object ConvertTo(CultureInfo culture, string format, object value, Type to)
		{
			if (SystemConverter != null)
			{
				return SystemConverter.ConvertTo(null, culture, value, to);
			}

			if (value == null)
			{
				return string.Empty;
			}

			return value.ToString();
		}
	}
}
