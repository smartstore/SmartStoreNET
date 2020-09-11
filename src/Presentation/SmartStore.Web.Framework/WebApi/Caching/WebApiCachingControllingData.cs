using System.Web;
using System.Web.Caching;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Plugins;

namespace SmartStore.Web.Framework.WebApi.Caching
{
    public static class WebApiCachingControllingData
    {
        private static readonly object _lock = new object();

        public static string Key { get; } = "WebApiControllingData";

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
                            NoRequestTimestampValidation = settings.NoRequestTimestampValidation,
                            AllowEmptyMd5Hash = settings.AllowEmptyMd5Hash,
                            LogUnauthorized = settings.LogUnauthorized,
                            ApiUnavailable = (plugin == null || !plugin.Installed),
                            PluginVersion = (plugin == null ? "1.0" : plugin.Version.ToString()),
                            MaxTop = settings.MaxTop,
                            MaxExpansionDepth = settings.MaxExpansionDepth
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
        public bool NoRequestTimestampValidation { get; set; }
        public bool AllowEmptyMd5Hash { get; set; }
        public bool LogUnauthorized { get; set; }
        public string PluginVersion { get; set; }
        public int MaxTop { get; set; }
        public int MaxExpansionDepth { get; set; }

        public string Version => "{0} {1}".FormatInvariant(WebApiGlobal.MaxApiVersion, PluginVersion);
    }
}
