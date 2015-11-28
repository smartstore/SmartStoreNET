using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace SmartStore.Web.Framework.Themes
{
	public class TwoLevelViewLocationCache : IViewLocationCache
	{
		private readonly static object s_key = new object();
		private readonly IViewLocationCache _cache;

		public TwoLevelViewLocationCache(IViewLocationCache cache)
		{
			_cache = cache;
		}

		private static IDictionary<string, string> GetRequestCache(HttpContextBase httpContext)
		{
			var d = httpContext.Items[s_key] as IDictionary<string, string>;
			if (d == null)
			{
				d = new Dictionary<string, string>();
				httpContext.Items[s_key] = d;
			}
			return d;
		}

		public string GetViewLocation(HttpContextBase httpContext, string key)
		{
			var d = GetRequestCache(httpContext);
			string location;
			if (!d.TryGetValue(key, out location))
			{
				location = _cache.GetViewLocation(httpContext, key);
				d[key] = location;
			}
			return location;
		}

		public void InsertViewLocation(HttpContextBase httpContext, string key, string virtualPath)
		{
			_cache.InsertViewLocation(httpContext, key, virtualPath);
		}
	}
}
