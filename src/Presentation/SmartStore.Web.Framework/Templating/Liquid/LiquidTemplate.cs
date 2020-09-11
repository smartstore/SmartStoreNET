using System;
using System.Collections.Generic;
using System.Web.Hosting;
using DotLiquid;
using SmartStore.ComponentModel;

namespace SmartStore.Templating.Liquid
{
    internal class LiquidTemplate : ITemplate
    {
        public LiquidTemplate(Template template, string source)
        {
            Guard.NotNull(template, nameof(template));
            Guard.NotNull(source, nameof(source));

            Template = template;
            Source = source;
        }

        public string Source
        {
            get;
            internal set;
        }

        public Template Template
        {
            get;
            internal set;
        }

        public string Render(object model, IFormatProvider formatProvider)
        {
            Guard.NotNull(model, nameof(model));
            Guard.NotNull(formatProvider, nameof(formatProvider));

            var p = CreateParameters(model, formatProvider);
            return Template.Render(p);
        }

        private RenderParameters CreateParameters(object data, IFormatProvider formatProvider)
        {
            var p = new RenderParameters(formatProvider);

            Hash hash = null;

            if (data is ISafeObject so)
            {
                if (so.GetWrappedObject() is IDictionary<string, object> soDict)
                {
                    hash = Hash.FromDictionary(soDict);
                }
                else
                {
                    data = so.GetWrappedObject();
                }
            }

            if (hash == null)
            {
                hash = new Hash();

                if (data is IDictionary<string, object> dict)
                {
                    foreach (var kvp in dict)
                    {
                        hash[kvp.Key] = LiquidUtil.CreateSafeObject(kvp.Value);
                    }
                }
                else
                {
                    var props = FastProperty.GetProperties(data.GetType());
                    foreach (var prop in props)
                    {
                        hash[prop.Key] = LiquidUtil.CreateSafeObject(prop.Value.GetValue(data));
                    }
                }
            }

            p.LocalVariables = hash;
            p.ErrorsOutputMode = HostingEnvironment.IsHosted ? ErrorsOutputMode.Display : ErrorsOutputMode.Rethrow;

            return p;
        }
    }
}
