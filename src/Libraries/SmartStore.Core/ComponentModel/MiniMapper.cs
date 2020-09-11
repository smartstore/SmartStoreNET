using System;
using System.Globalization;
using System.Linq;
using SmartStore.Core;
using SmartStore.Utilities;

namespace SmartStore.ComponentModel
{
    /// <summary>
    /// A very simple object mapper utility which tries to map properties of the same name between two objects.
    /// If matched properties has different types, the mapper tries to convert them.
    /// If conversion fails, the property is skipped (no exception is thrown).
    /// MiniMapper cannot handle sequence and predefined types.
    /// </summary>
    public static class MiniMapper
    {
        public static TTo Map<TFrom, TTo>(TFrom from, CultureInfo culture = null)
            where TFrom : class
            where TTo : class, new()
        {
            Guard.NotNull(from, nameof(from));

            if (TryMap(from, from.GetType(), typeof(TTo), culture, out var result))
            {
                return (TTo)result;
            }

            return default(TTo);
        }

        public static object Map<TFrom>(TFrom from, Type toType, CultureInfo culture = null)
            where TFrom : class
        {
            Guard.NotNull(from, nameof(from));
            Guard.NotNull(toType, nameof(toType));
            Guard.HasDefaultConstructor(toType);

            TryMap(from, from.GetType(), toType, culture, out var result);

            return result;
        }

        public static void Map<TFrom, TTo>(TFrom from, TTo to, CultureInfo culture = null)
            where TFrom : class
            where TTo : class
        {
            Guard.NotNull(from, nameof(from));
            Guard.NotNull(to, nameof(to));

            MapComplex(from, to, from.GetType(), culture ?? CultureInfo.CurrentCulture);
        }

        private static bool TryMap(object from, Type fromType, Type toType, CultureInfo culture, out object result)
        {
            if (CommonHelper.TryConvert(from, toType, culture, out result))
            {
                return true;
            }

            if (fromType.IsPlainObjectType() && toType.IsPlainObjectType())
            {
                result = Activator.CreateInstance(toType);
                MapComplex(from, result, fromType, culture);
                return true;
            }

            return false;
        }

        private static void MapComplex(object from, object to, Type fromType, CultureInfo culture)
        {
            if (object.ReferenceEquals(from, to))
            {
                // Cannot map same instance or null source
                return;
            }

            var toType = to.GetType();
            var isEntityMapping = typeof(BaseEntity).IsAssignableFrom(toType);
            var toProps = GetFastPropertiesFor(toType).ToArray();

            foreach (var toProp in toProps)
            {
                var fromProp = FastProperty.GetProperty(fromType, toProp.Name, PropertyCachingStrategy.Cached);
                if (fromProp == null)
                {
                    continue;
                }

                var fromPropType = fromProp.Property.PropertyType;
                var toPropType = toProp.Property.PropertyType;
                var sourceValue = fromProp.GetValue(from);

                if (isEntityMapping && (fromPropType == typeof(int) || fromPropType == typeof(int?)) && toPropType == typeof(int?) && object.Equals(0, sourceValue) && toProp.Name.EndsWith("Id"))
                {
                    // TODO: This is more a hack than a proper solution. Find a more generic way to convert int FK properties...
                    // Special mapper, that avoids DbUpdate exceptions in cases where
                    // optional (nullable) int FK properties are 0 instead of "null" 
                    // after mapping model > entity.
                    toProp.SetValue(to, null);
                }
                else if (TryMap(sourceValue, fromPropType, toPropType, culture, out var targetValue))
                {
                    // Set it
                    toProp.SetValue(to, targetValue);
                }
            }
        }

        private static FastProperty[] GetFastPropertiesFor(Type type)
        {
            return FastProperty.GetProperties(type, PropertyCachingStrategy.Uncached)
                .Values
                .Where(pi => pi.IsPublicSettable)
                .ToArray();
        }
    }
}
