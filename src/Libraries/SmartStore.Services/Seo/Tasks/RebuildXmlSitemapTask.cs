using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core.Domain.Seo;
using SmartStore.Services.Localization;
using SmartStore.Services.Stores;
using SmartStore.Services.Tasks;

namespace SmartStore.Services.Seo
{
	public class RebuildXmlSitemapTask : ITask
	{
		private readonly IStoreService _storeService;
		private readonly ILanguageService _languageService;
		private readonly IXmlSitemapGenerator _generator;
		private readonly SeoSettings _seoSettings;

		public RebuildXmlSitemapTask(
			IStoreService storeService,
			ILanguageService languageService,
			IXmlSitemapGenerator generator, 
			SeoSettings seoSettings)
		{
			_storeService = storeService;
			_languageService = languageService;
			_generator = generator;
			_seoSettings = seoSettings;
		}

		public void Execute(TaskExecutionContext ctx)
		{
			var stores = _storeService.GetAllStores();

			foreach (var store in stores)
			{
				var languages = _languageService.GetAllLanguages(false, store.Id);
				var buildContext = new XmlSitemapBuildContext(store, languages.ToArray())
				{
					CancellationToken = ctx.CancellationToken,
					ProgressCallback = OnProgress
				};

				_generator.Rebuild(buildContext);
			}

			void OnProgress(int value, int max, string msg)
			{
				ctx.SetProgress(value, max, msg, true);
			}
		}
	}
}
