using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Web.Routing;
using Newtonsoft.Json.Linq;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Email;

namespace SmartStore.ComponentModel
{
    public static class TypeConverterFactory
    {
        private static readonly ConcurrentDictionary<Type, ITypeConverter> _typeConverters = new ConcurrentDictionary<Type, ITypeConverter>();

        static TypeConverterFactory()
        {
            CreateDefaultConverters();
        }

        private static void CreateDefaultConverters()
        {
            _typeConverters.TryAdd(typeof(DateTime), new DateTimeConverter());
            _typeConverters.TryAdd(typeof(TimeSpan), new TimeSpanConverter());
            _typeConverters.TryAdd(typeof(bool), new BooleanConverter(
                new[] { "yes", "y", "on", "wahr" },
                new[] { "no", "n", "off", "falsch" }));

            ITypeConverter converter = new ShippingOptionConverter(true);
            _typeConverters.TryAdd(typeof(IList<ShippingOption>), converter);
            _typeConverters.TryAdd(typeof(List<ShippingOption>), converter);
            _typeConverters.TryAdd(typeof(ShippingOption), new ShippingOptionConverter(false));

            converter = new ProductBundleDataConverter(true);
            _typeConverters.TryAdd(typeof(IList<ProductBundleItemOrderData>), converter);
            _typeConverters.TryAdd(typeof(List<ProductBundleItemOrderData>), converter);
            _typeConverters.TryAdd(typeof(ProductBundleItemOrderData), new ProductBundleDataConverter(false));

            converter = new DictionaryTypeConverter<IDictionary<string, object>>();
            _typeConverters.TryAdd(typeof(IDictionary<string, object>), converter);
            _typeConverters.TryAdd(typeof(Dictionary<string, object>), converter);
            _typeConverters.TryAdd(typeof(RouteValueDictionary), new DictionaryTypeConverter<RouteValueDictionary>());
            _typeConverters.TryAdd(typeof(ExpandoObject), new DictionaryTypeConverter<ExpandoObject>());
            _typeConverters.TryAdd(typeof(HybridExpando), new DictionaryTypeConverter<HybridExpando>());

            _typeConverters.TryAdd(typeof(EmailAddress), new EmailAddressConverter());
            _typeConverters.TryAdd(typeof(JObject), new JObjectConverter());
        }

        public static IReadOnlyCollection<ITypeConverter> Converters => _typeConverters.Values.AsReadOnly();

        public static void RegisterConverter<T>(ITypeConverter typeConverter)
        {
            RegisterConverter(typeof(T), typeConverter);
        }

        public static void RegisterConverter(Type type, ITypeConverter typeConverter)
        {
            Guard.NotNull(type, nameof(type));
            Guard.NotNull(typeConverter, nameof(typeConverter));

            _typeConverters.TryAdd(type, typeConverter);
        }

        public static ITypeConverter RemoveConverter<T>(ITypeConverter typeConverter)
        {
            return RemoveConverter(typeof(T));
        }

        public static ITypeConverter RemoveConverter(Type type)
        {
            Guard.NotNull(type, nameof(type));

            _typeConverters.TryRemove(type, out var converter);
            return converter;
        }

        public static ITypeConverter GetConverter<T>()
        {
            return GetConverter(typeof(T));
        }

        public static ITypeConverter GetConverter(object component)
        {
            Guard.NotNull(component, nameof(component));

            return GetConverter(component.GetType());
        }

        public static ITypeConverter GetConverter(Type type)
        {
            Guard.NotNull(type, nameof(type));

            return _typeConverters.GetOrAdd(type, Get);

            ITypeConverter Get(Type t)
            {
                // Nullable types
                if (type.IsNullable(out Type elementType))
                {
                    return new NullableConverter(type, elementType);
                }

                // Sequence types
                if (type.IsSequenceType(out elementType))
                {
                    var converter = (ITypeConverter)Activator.CreateInstance(typeof(EnumerableConverter<>).MakeGenericType(elementType), type);
                    return converter;
                }

                // Default fallback
                return new DefaultTypeConverter(type);
            }
        }
    }
}
