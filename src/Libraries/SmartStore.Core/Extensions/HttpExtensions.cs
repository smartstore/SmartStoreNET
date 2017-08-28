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
using SmartStore.Core.Infrastructure;
using SmartStore.Core;
using System.Web.Mvc;

namespace SmartStore
{  
    public static class HttpExtensions
    {
        private const string HTTP_CLUSTER_VAR = "HTTP_CLUSTER_HTTPS";
		private const string HTTP_XFWDPROTO_VAR = "HTTP_X_FORWARDED_PROTO";

		/// <summary>
		/// Gets a value which indicates whether the HTTP connection uses secure sockets (HTTPS protocol). 
		/// Works with Cloud's load balancers.
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		public static bool IsSecureConnection(this HttpRequestBase request)
        {
            return (request.IsSecureConnection
				|| (request.ServerVariables[HTTP_CLUSTER_VAR] != null || request.ServerVariables[HTTP_CLUSTER_VAR] == "on")
				|| (request.ServerVariables[HTTP_XFWDPROTO_VAR] != null || request.ServerVariables[HTTP_XFWDPROTO_VAR] == "https"));
        }


		/// <summary>
		/// Returns wether the specified url is local to the host or not
		/// </summary>
		/// <param name="request"></param>
		/// <param name="url"></param>
		/// <returns></returns>
		public static bool IsAppLocalUrl(this HttpRequestBase request, string url)
		{

			if (string.IsNullOrWhiteSpace(url))
			{
				return false;
			}

			if (url.StartsWith("~/"))
			{
				return true;
			}

			if (url.StartsWith("//") || url.StartsWith("/\\"))
			{
				return false;
			}

			// at this point when the url starts with "/" it is local
			if (url.StartsWith("/"))
			{
				return true;
			}

			// at this point, check for a fully qualified url
			try
			{
				var uri = new Uri(url);
				if (uri.Authority.Equals(request.Headers["Host"], StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}

				// finally, check the base url from the settings
				var storeContext = EngineContext.Current.Resolve<IStoreContext>();
				if (storeContext != null)
				{
					var baseUrl = storeContext.CurrentStore.Url;
					if (baseUrl.HasValue())
					{
						if (uri.Authority.Equals(new Uri(baseUrl).Authority, StringComparison.OrdinalIgnoreCase))
						{
							return true;
						}
					}
				}

				return false;
			}
			catch
			{
				// mall-formed url e.g, "abcdef"
				return false;
			}
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
			CopyCookie(webRequest, httpRequest, FormsAuthentication.FormsCookieName);
		}

		[SuppressMessage("ReSharper", "PossibleNullReferenceException")]
		public static void SetAnonymousIdentCookie(this HttpWebRequest webRequest, HttpRequestBase httpRequest)
		{
			CopyCookie(webRequest, httpRequest, "SMARTSTORE.ANONYMOUS"); 
		}

		private static void CopyCookie(HttpWebRequest webRequest, HttpRequestBase sourceHttpRequest, string cookieName)
		{
			Guard.NotNull(webRequest, nameof(webRequest));
			Guard.NotNull(sourceHttpRequest, nameof(sourceHttpRequest));
			Guard.NotEmpty(cookieName, nameof(cookieName));

			var sourceCookie = sourceHttpRequest.Cookies[cookieName];
			if (sourceCookie == null)
				return;

			var sendCookie = new Cookie(sourceCookie.Name, sourceCookie.Value, sourceCookie.Path, sourceHttpRequest.Url.Host);

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

		public static T GetItem<T>(this HttpContext httpContext, string key, Func<T> factory = null, bool forceCreation = true)
		{
			if (httpContext?.Items == null)
			{
				return default(T);
			}

			return GetItem<T>(new HttpContextWrapper(httpContext), key, factory, forceCreation);
		}

		public static T GetItem<T>(this HttpContextBase httpContext, string key, Func<T> factory = null, bool forceCreation = true)
		{
			Guard.NotEmpty(key, nameof(key));

			var items = httpContext?.Items;
			if (items == null)
			{
				return default(T);
			}

			if (items.Contains(key))
			{
				return (T)items[key];
			}
			else
			{
				if (forceCreation)
				{
					var item = items[key] = (factory ?? (() => Activator.CreateInstance<T>())).Invoke();
					return (T)item;
				}
				else
				{
					return default(T);
				}
			}
		}

		public static void RemoveByPattern(this Cache cache, string pattern)
		{
			var regionName = "SmartStoreNET:";

			pattern = pattern == "*" ? regionName : pattern;

			var keys = from entry in HttpRuntime.Cache.AsParallel().Cast<DictionaryEntry>()
					   let key = entry.Key.ToString()
					   where key.StartsWith(pattern, StringComparison.OrdinalIgnoreCase)
					   select key;

			foreach (var key in keys.ToArray())
			{
				cache.Remove(key);
			}
		}

        public static ControllerContext GetMasterControllerContext(this ControllerContext controllerContext)
        {
            Guard.NotNull(controllerContext, nameof(controllerContext));

            var ctx = controllerContext;

            while (ctx.ParentActionViewContext != null)
            {
                ctx = ctx.ParentActionViewContext;
            }

            return ctx;
        }
	}

}
