using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartStore.DevTools.OutputCache
{
	public class OutputCacheSettings
	{
		public OutputCacheSettings()
		{
			IsEnabled = true; // true;
			DefaultCacheDuration = 300;
			AutomaticInvalidationEnabled = true;
			CacheAuthenticatedRequests = true;
			IgnoreNoCache = true;
			DebugMode = true;

			CacheableRoutes = new List<CacheableRoute>
			{
				new CacheableRoute { Route = "Home/Index" },
				new CacheableRoute { Route = "Home/SitemapSeo" },
				new CacheableRoute { Route = "Catalog/Category" },
				new CacheableRoute { Route = "Catalog/Manufacturer" },
				new CacheableRoute { Route = "Catalog/ProductsByTag" },
				new CacheableRoute { Route = "Product/ProductDetails", Duration = 3600 },
				new CacheableRoute { Route = "Topic/TopicDetails", Duration = 3600 },
				new CacheableRoute { Route = "Common/Header" },
				new CacheableRoute { Route = "Common/Footer" },
			};
		}

		public bool IsEnabled { get; set; }
		public int DefaultCacheDuration { get; set; }
		public bool AutomaticInvalidationEnabled { get; set; }
		public bool CacheAuthenticatedRequests { get; set; }
		public bool IgnoreNoCache { get; set; }
		public bool DebugMode { get; set; }

		public IList<CacheableRoute> CacheableRoutes { get; set; }
		public IList<CacheableRoute> CacheableChildActions { get; set; }
	}

	public class CacheableRoute
	{
		public string Route { get; set; }
		public int? Duration { get; set; }
	}
}