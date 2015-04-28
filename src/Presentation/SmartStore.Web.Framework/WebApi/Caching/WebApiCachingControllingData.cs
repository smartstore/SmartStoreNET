﻿using System.Web;
using System.Web.Caching;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Plugins;

namespace SmartStore.Web.Framework.WebApi.Caching
{
	public static class WebApiCachingControllingData
	{
		private static object _lock = new object();

		public static string Key { get { return "WebApiControllingData"; } }

		public static void Remove()
		{
			try
			{
				HttpRuntime.Cache.Remove(Key);
			}
			catch { }
		}

		public static WebApiControllingCacheData Data()
		{
			var data = HttpRuntime.Cache[Key] as WebApiControllingCacheData;
			if (data == null)
			{
				lock (_lock)
				{
					data = HttpRuntime.Cache[Key] as WebApiControllingCacheData;

					if (data == null)
					{
						var engine = EngineContext.Current;
						var plugin = engine.Resolve<IPluginFinder>().GetPluginDescriptorBySystemName(WebApiGlobal.PluginSystemName);
						var settings = engine.Resolve<WebApiSettings>();

						data = new WebApiControllingCacheData
						{
							ValidMinutePeriod = settings.ValidMinutePeriod,
							LogUnauthorized = settings.LogUnauthorized,
							ApiUnavailable = (plugin == null || !plugin.Installed),
							PluginVersion = (plugin == null ? "1.0" : plugin.Version.ToString())
						};

						HttpRuntime.Cache.Add(Key, data, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);
					}
				}
			}
			return data;
		}
	}

	public partial class WebApiControllingCacheData
	{
		public bool ApiUnavailable { get; set; }
		public int ValidMinutePeriod { get; set; }
		public bool LogUnauthorized { get; set; }
		public string PluginVersion { get; set; }

		public string Version
		{
			get
			{
				return "{0} {1}".FormatWith(WebApiGlobal.MaxApiVersion, PluginVersion);
			}
		}
	}
}
