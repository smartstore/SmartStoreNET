using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using SmartStore.Core.Domain.Shipping;

namespace SmartStore.ComponentModel
{
    [SuppressMessage("ReSharper", "TryCastAlwaysSucceeds")]
    public class ShippingOptionConverter : DefaultTypeConverter
    {
        private readonly bool _forList;

        public ShippingOptionConverter(bool forList)
            : base(typeof(object))
        {
            _forList = forList;
        }

        public override bool CanConvertFrom(Type type)
        {
            return type == typeof(string);
        }

        public override bool CanConvertTo(Type type)
        {
            return type == typeof(string);
        }

        public override object ConvertFrom(CultureInfo culture, object value)
        {
            if (value is string str)
            {
                object result = null;
                if (!String.IsNullOrEmpty(str))
                {
                    try
                    {
                        using (var tr = new StringReader(str))
                        {
                            var serializer = new XmlSerializer(_forList ? typeof(List<ShippingOption>) : typeof(ShippingOption));
                            result = serializer.Deserialize(tr);
                        }
                    }
                    catch
                    {
                        // xml error
                    }
                }

                return result;
            }

            return base.ConvertFrom(culture, value);
        }

        public override object ConvertTo(CultureInfo culture, string format, object value, Type to)
        {
            if (to == typeof(string))
            {
                if (value != null && (value is ShippingOption || value is IList<ShippingOption>))
                {
                    var sb = new StringBuilder();
                    using (var tw = new StringWriter(sb))
                    {
                        var serializer = new XmlSerializer(_forList ? typeof(List<ShippingOption>) : typeof(ShippingOption));
                        serializer.Serialize(tw, value);
                        return sb.ToString();
                    }
                }
                else
                {
                    return string.Empty;
                }
            }

            return base.ConvertTo(culture, format, value, to);
        }
    }
}
