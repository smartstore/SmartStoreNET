using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core.Domain.Seo;
using SmartStore.Services.Tasks;

namespace SmartStore.Services.Seo
{
	public class RebuildXmlSitemapTask : ITask
	{
		private readonly IXmlSitemapGenerator _generator;
		private readonly SeoSettings _seoSettings;

		public RebuildXmlSitemapTask(IXmlSitemapGenerator generator, SeoSettings seoSettings)
		{
			_generator = generator;
			_seoSettings = seoSettings;
		}

		public void Execute(TaskExecutionContext ctx)
		{
			if (_generator.IsGenerated)
			{
				_generator.Invalidate();
			}

			// enforces refresh
			_generator.GetSitemap(0);
		}
	}
}
