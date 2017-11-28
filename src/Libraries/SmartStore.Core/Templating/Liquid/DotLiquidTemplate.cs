using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.Hosting;
using DotLiquid;
using SmartStore.ComponentModel;

namespace SmartStore.Templating.Liquid
{
	internal class DotLiquidTemplate : ITemplate
	{
		private readonly Template _template;

		public DotLiquidTemplate(Template template)
		{
			Guard.NotNull(template, nameof(template));

			_template = template;
		}

		public string Render(object data, IFormatProvider formatProvider)
		{
			Guard.NotNull(data, nameof(data));
			Guard.NotNull(formatProvider, nameof(formatProvider));
			
			var p = CreateParameters(data, formatProvider);
			return _template.Render(p);
		}

		private RenderParameters CreateParameters(object data, IFormatProvider formatProvider)
		{
			var p = new RenderParameters(formatProvider);

			var hash = new Hash();

			if (data is IDictionary<string, object> dict)
			{
				foreach (var kvp in dict)
				{
					hash[kvp.Key] = DotLiquidUtil.CreateSafeObject(kvp.Value);
				}
			}
			else
			{
				var props = FastProperty.GetProperties(data);
				foreach (var prop in props)
				{
					hash[prop.Key] = DotLiquidUtil.CreateSafeObject(prop.Value.GetValue(data));
				}
			}

			p.LocalVariables = hash;
			p.ErrorsOutputMode = HostingEnvironment.IsHosted ? ErrorsOutputMode.Display : ErrorsOutputMode.Rethrow;

			return p;
		}
	}
}
