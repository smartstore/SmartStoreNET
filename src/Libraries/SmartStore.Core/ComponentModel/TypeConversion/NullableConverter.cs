using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace SmartStore.ComponentModel
{
    [SuppressMessage("ReSharper", "TryCastAlwaysSucceeds")]
    public class NullableConverter : DefaultTypeConverter
    {
        private readonly bool _elementTypeIsConvertible;

        internal NullableConverter(Type type, Type elementType)
            : base(type)
        {
            NullableType = type;
            ElementType = elementType ?? Nullable.GetUnderlyingType(type);

            if (ElementType == null)
            {
                throw Error.Argument("type", "Type is not a nullable type.");
            }

            _elementTypeIsConvertible = typeof(IConvertible).IsAssignableFrom(ElementType) && !ElementType.IsEnum;
            ElementTypeConverter = TypeConverterFactory.GetConverter(ElementType);
        }

        public Type NullableType
        {
            get;
            private set;
        }

        public Type ElementType
        {
            get;
            private set;
        }

        public ITypeConverter ElementTypeConverter
        {
            get;
            private set;
        }

        public override bool CanConvertFrom(Type type)
        {
            if (type == this.ElementType)
            {
                return true;
            }

            if (ElementTypeConverter.CanConvertFrom(type))
            {
                return true;
            }

            if (_elementTypeIsConvertible && type != typeof(string) && typeof(IConvertible).IsAssignableFrom(type))
            {
                return true;
            }

            return false;
        }

        public override bool CanConvertTo(Type type)
        {
            //Console.WriteLine("NullableConverter can convert to {0}: {1}".FormatInvariant(type.Name, UnderlyingTypeConverter.CanConvertTo(type)));

            if (type == this.ElementType)
            {
                return true;
            }

            return ElementTypeConverter.CanConvertTo(type);
        }

        public override object ConvertFrom(CultureInfo culture, object value)
        {
            if ((value == null) || (value.GetType() == this.ElementType))
            {
                return value;
            }

            if ((value is string) && string.IsNullOrEmpty(value as string))
            {
                return null;
            }

            if (_elementTypeIsConvertible && !(value is string) && value is IConvertible)
            {
                // num > num?
                return Convert.ChangeType(value, ElementType, culture);
            }

            return ElementTypeConverter.ConvertFrom(culture, value);
        }

        public override object ConvertTo(CultureInfo culture, string format, object value, Type to)
        {
            if ((to == this.ElementType) && this.NullableType.IsInstanceOfType(value))
            {
                return value;
            }

            if (value == null)
            {
                if (to == typeof(string))
                {
                    return string.Empty;
                }
            }

            return ElementTypeConverter.ConvertTo(culture, format, value, to);
        }
    }
}
