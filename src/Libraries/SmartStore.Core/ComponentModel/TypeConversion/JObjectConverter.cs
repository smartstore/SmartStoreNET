using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SmartStore.ComponentModel
{
    [SuppressMessage("ReSharper", "CanBeReplacedWithTryCastAndCheckForNull")]
    public class JObjectConverter : DefaultTypeConverter
    {
        public JObjectConverter()
            : base(typeof(JObject))
        {
        }

        public override bool CanConvertFrom(Type type)
        {
            return type == typeof(string)
                || type.IsPlainObjectType()
                || type.IsAnonymous();

        }

        public override bool CanConvertTo(Type type)
        {
            return type == typeof(string);
        }

        public override object ConvertFrom(CultureInfo culture, object value)
        {
            if (value is string json)
            {
                return JObject.Parse(json);
            }

            if (value != null)
            {
                return JObject.FromObject(value);
            }

            return base.ConvertFrom(culture, value);
        }

        public override object ConvertTo(CultureInfo culture, string format, object value, Type to)
        {
            if (value is JObject jobj && to == typeof(string))
            {
                return jobj.ToString(Formatting.Indented);
            }

            return base.ConvertTo(culture, format, value, to);
        }
    }
}
