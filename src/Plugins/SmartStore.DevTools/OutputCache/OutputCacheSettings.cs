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
			CacheAuthenticatedRequests = true;
		}

		public bool IsEnabled { get; set; }
		public int DefaultCacheDuration { get; set; }
		//public int DefaultMaxAge { get; set; }
		public bool CacheAuthenticatedRequests { get; set; }
		public bool DebugMode { get; set; }
	}
}