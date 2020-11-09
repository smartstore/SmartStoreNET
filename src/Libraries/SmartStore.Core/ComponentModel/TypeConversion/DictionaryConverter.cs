using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Web.Routing;
using SmartStore.Utilities;

namespace SmartStore.ComponentModel
{
    public class DictionaryTypeConverter<T> : DefaultTypeConverter where T : IDictionary<string, object>
    {
        public DictionaryTypeConverter()
            : base(typeof(object))
        {
        }

        public override bool CanConvertFrom(Type type)
        {
            return type.IsPlainObjectType() || type.IsAnonymous();
        }

        public override bool CanConvertTo(Type type)
        {
            return DictionaryConverter.CanCreateType(type);
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
            else if (to == typeof(Dictionary<string, object>))
            {
                return (Dictionary<string, object>)dict;
            }
            else if (to == typeof(ExpandoObject))
            {
                var expando = new ExpandoObject();
                expando.Merge(dict);
                return expando;
            }
            else if (to == typeof(HybridExpando))
            {
                var expando = new HybridExpando();
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
            if (value is IDictionary<string, object> dict)
            {
                return DictionaryConverter.CreateAndPopulate(to, dict, out _);
            }

            return base.ConvertTo(culture, format, value, to);
        }
    }
}
