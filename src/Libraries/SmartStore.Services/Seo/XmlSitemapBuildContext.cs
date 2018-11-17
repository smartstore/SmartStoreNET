using System;
using System.Threading;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Stores;
using SmartStore.Services.Tasks;

namespace SmartStore.Services.Seo
{
	public class XmlSitemapBuildContext
	{
		public XmlSitemapBuildContext(Store store, Language[] languages)
		{
			Guard.NotNull(store, nameof(store));
			Guard.NotEmpty(languages, nameof(languages));

			Store = store;
			Languages = languages;
			Protocol = Store.ForceSslForAllPages ? "https" : "http";
		}

		public CancellationToken CancellationToken { get; set; }
		public ProgressCallback ProgressCallback { get; set; }
		public Store Store { get; set; }
		public Language[] Languages { get; set; }

		public string Protocol { get; private set; }
	}
}
