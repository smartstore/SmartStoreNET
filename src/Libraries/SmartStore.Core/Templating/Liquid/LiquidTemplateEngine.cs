using System;
using DotLiquid;
using DotLiquid.NamingConventions;

namespace SmartStore.Templating.Liquid
{
	public partial class LiquidTemplateEngine : ITemplateEngine
	{
		public LiquidTemplateEngine()
		{
			Template.NamingConvention = new CSharpNamingConvention();
		}

		public ITemplate Compile(string template)
		{
			Guard.NotEmpty(template, nameof(template));

			return new LiquidTemplate(Template.Parse(template))
			{
				TimeStamp = DateTime.UtcNow
			};
		}

		public string Render(string template, object data, IFormatProvider formatProvider)
		{
			Guard.NotEmpty(template, nameof(template));
			Guard.NotNull(data, nameof(data));
			Guard.NotNull(formatProvider, nameof(formatProvider));

			return Compile(template).Render(data, formatProvider);
		}
	}
}
