using System;
using System.ComponentModel;
using System.Globalization;

namespace SmartStore.ComponentModel
{
    public class DefaultTypeConverter : ITypeConverter
    {
        private readonly Lazy<TypeConverter> _systemConverter;
        private readonly Type _type;
        private readonly bool _typeIsConvertible;
        private readonly bool _typeIsEnum;

        public DefaultTypeConverter(Type type)
        {
            Guard.NotNull(type, nameof(type));

            _type = type;
            _typeIsConvertible = typeof(IConvertible).IsAssignableFrom(type);
            _typeIsEnum = type.IsEnum;
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
            // Use Convert.ChangeType if both types are IConvertible
            if (_typeIsConvertible && typeof(IConvertible).IsAssignableFrom(type))
            {
                return true;
            }

            if (SystemConverter != null)
            {
                return SystemConverter.CanConvertFrom(type);
            }

            return false;
        }

        public virtual bool CanConvertTo(Type type)
        {
            // Use Convert.ChangeType if both types are IConvertible
            if (_typeIsConvertible && typeof(IConvertible).IsAssignableFrom(type))
            {
                return true;
            }

            if (type == typeof(string))
            {
                return true;
            }

            if (SystemConverter != null)
            {
                return SystemConverter.CanConvertTo(type);
            }

            return false;
        }

        public virtual object ConvertFrom(CultureInfo culture, object value)
        {
            // Use Convert.ChangeType if both types are IConvertible
            if (!_typeIsEnum && _typeIsConvertible && value is IConvertible)
            {
                return Convert.ChangeType(value, _type, culture);
            }

            if (SystemConverter != null)
            {
                return SystemConverter.ConvertFrom(null, culture, value);
            }

            throw Error.InvalidCast(value.GetType(), _type);
        }

        public virtual object ConvertTo(CultureInfo culture, string format, object value, Type to)
        {
            // Use Convert.ChangeType if both types are IConvertible
            if (!_typeIsEnum && _typeIsConvertible && value != null && typeof(IConvertible).IsAssignableFrom(to))
            {
                return Convert.ChangeType(value, to, culture);
            }

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
