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

			if (HostingEnvironment.IsHosted)
			{
				if (vpp != null)
				{
					Template.FileSystem = new LiquidFileSystem(vpp);
				}

				Template.RegisterTag<T>("T");
			}
		}

		public ITemplate Compile(string source)
		{
			Guard.NotEmpty(source, nameof(source));

			return new LiquidTemplate(Template.Parse(source), source);
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
