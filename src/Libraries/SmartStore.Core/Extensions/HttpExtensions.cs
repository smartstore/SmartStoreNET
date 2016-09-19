using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.Security;

namespace SmartStore
{  
    public static class HttpExtensions
    {
        private const string HTTP_CLUSTER_VAR = "HTTP_CLUSTER_HTTPS";
        
        /// <summary>
        /// Gets a value which indicates whether the HTTP connection uses secure sockets (HTTPS protocol). 
        /// Works with Cloud's load balancers.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool IsSecureConnection(this HttpRequestBase request)
        {
            return (request.IsSecureConnection || (request.ServerVariables[HTTP_CLUSTER_VAR] != null || request.ServerVariables[HTTP_CLUSTER_VAR] == "on"));
        }

        /// <summary>
        /// Gets a value which indicates whether the HTTP connection uses secure sockets (HTTPS protocol). 
        /// Works with Cloud's load balancers.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool IsSecureConnection(this HttpRequest request)
        {
            return (request.IsSecureConnection || (request.ServerVariables[HTTP_CLUSTER_VAR] != null || request.ServerVariables[HTTP_CLUSTER_VAR] == "on"));
        }

	    [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
	    public static void SetFormsAuthenticationCookie(this HttpWebRequest webRequest, HttpRequestBase httpRequest)
		{
			Guard.NotNull(webRequest, nameof(webRequest));
			Guard.NotNull(httpRequest, nameof(httpRequest));

			var authCookie = httpRequest.Cookies[FormsAuthentication.FormsCookieName];
			if (authCookie == null)
				return;

			var sendCookie = new Cookie(authCookie.Name, authCookie.Value, authCookie.Path, httpRequest.Url.Host);

			if (webRequest.CookieContainer == null)
			{
				webRequest.CookieContainer = new CookieContainer();
			}

			webRequest.CookieContainer.Add(sendCookie);
		}

		public static string BuildScopedKey(this Cache cache, string key)
		{
			return key.HasValue() ? "SmartStoreNET:" + key : null;
		}

		public static T GetOrAdd<T>(this Cache cache, string key, Func<T> acquirer, TimeSpan? duration = null)
		{
			Guard.NotEmpty(key, nameof(key));
			Guard.NotNull(acquirer, nameof(acquirer));

			object obj = cache.Get(key);

			if (obj != null)
			{
				return (T)obj;
			}

			var value = acquirer();

			var absoluteExpiration = Cache.NoAbsoluteExpiration;
			if (duration.HasValue)
			{
				absoluteExpiration = DateTime.UtcNow + duration.Value;
			}

			cache.Insert(key, value, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration);

			return value;
		}

		public static void RemoveByPattern(this Cache cache, string pattern)
		{
			var regionName = "SmartStoreNET:";

			pattern = pattern == "*" ? "" : pattern;

			var keys = from entry in HttpRuntime.Cache.AsParallel().Cast<DictionaryEntry>()
					   let key = entry.Key.ToString()
					   where key.StartsWith(regionName + pattern, StringComparison.OrdinalIgnoreCase)
					   select key;

			foreach (var key in keys.ToArray())
			{
				cache.Remove(key);
			}
		}
	}

}
