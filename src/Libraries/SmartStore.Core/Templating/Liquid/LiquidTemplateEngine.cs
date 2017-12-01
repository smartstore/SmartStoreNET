using System;
using System.Web.Hosting;
using DotLiquid;
using DotLiquid.NamingConventions;
using SmartStore.Core;
using SmartStore.Core.IO;

namespace SmartStore.Templating.Liquid
{
	public partial class LiquidTemplateEngine : ITemplateEngine
	{
		public LiquidTemplateEngine(IVirtualPathProvider vpp)
		{
			Template.NamingConvention = new CSharpNamingConvention();

			if (vpp != null && HostingEnvironment.IsHosted)
			{
				Template.FileSystem = new LiquidFileSystem(vpp);
			}		
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

		public object CreateTestModelFor(BaseEntity entity, string modelPrefix)
		{
			Guard.NotNull(entity, nameof(entity));

			return new TestDrop(entity, modelPrefix);
		}
	}
}
