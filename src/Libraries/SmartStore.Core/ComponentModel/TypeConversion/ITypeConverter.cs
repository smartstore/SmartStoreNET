using System;
using System.Globalization;

namespace SmartStore.ComponentModel
{
    /// <summary>
    /// Converts objects.
    /// </summary>
    public interface ITypeConverter
    {
        /// <summary>
        /// Returns whether this converter can convert an object of the given type to the type of this converter.
        /// </summary>
        /// <param name="type">A Type that represents the type you want to convert from. </param>
        /// <returns>true if this converter can perform the conversion; otherwise, false.</returns>
        bool CanConvertFrom(Type type);

        /// <summary>
        /// Returns whether this converter can convert the object to the specified type.
        /// </summary>
        /// <param name="type">A Type that represents the type you want to convert to. </param>
        /// <returns>true if this converter can perform the conversion; otherwise, false.</returns>
        bool CanConvertTo(Type type);

        /// <summary>
        /// Converts the given value to the type of this converter.
        /// </summary>
        /// <param name="culture">The <see cref="CultureInfo"/> to use as the current culture. If null is passed, the invariant culture is assumed.</param>
        /// <param name="value">The object to convert.</param>
        /// <returns>An object that represents the converted value.</returns>
        object ConvertFrom(CultureInfo culture, object value);

        /// <summary>
        /// Converts the given value object to the specified type, using the arguments.
        /// </summary>
        /// <param name="culture">The <see cref="CultureInfo"/> to use as the current culture. If null is passed, the invariant culture is assumed.</param>
        /// <param name="format">A standard or custom format expression.</param>
        /// <param name="value">The object to convert.</param>
        /// <param name="to">The type to convert the value parameter to.</param>
        /// <returns>An Object that represents the converted value.</returns>
        object ConvertTo(CultureInfo culture, string format, object value, Type to);
    }

    public static class ITypeConverterExtensions
    {
        public static object ConvertFrom(this ITypeConverter converter, object value)
        {
            return converter.ConvertFrom(CultureInfo.InvariantCulture, value);
        }

        public static object ConvertTo(this ITypeConverter converter, object value, Type to)
        {
            return converter.ConvertTo(CultureInfo.InvariantCulture, null, value, to);
        }

        public static object SafeConvert(this ITypeConverter converter, string value)
        {
            try
            {
                if (converter != null && value.HasValue() && converter.CanConvertFrom(typeof(string)))
                {
                    return converter.ConvertFrom(value);
                }
            }
            catch (Exception exc)
            {
                exc.Dump();
            }

            return null;
        }

        public static bool IsEqual(this ITypeConverter converter, string value, object compareWith)
        {
            object convertedObject = converter.SafeConvert(value);

            if (convertedObject != null && compareWith != null)
                return convertedObject.Equals(compareWith);

            return false;
        }
    }
}
