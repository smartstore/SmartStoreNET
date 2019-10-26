﻿using System;
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
using SmartStore.Core.Fakes;
using System.Runtime.CompilerServices;

namespace SmartStore
{
	public static class HttpExtensions
	{
		private const string CACHE_REGION_NAME = "SmartStoreNET:";

		private static readonly List<Tuple<string, string>> _sslHeaders = new List<Tuple<string, string>>
		{
			new Tuple<string, string>("HTTP_CLUSTER_HTTPS", "on"),
            new Tuple<string, string>("HTTP_X_FORWARDED_PROTO", "https"),
            new Tuple<string, string>("X-Forwarded-Proto", "https"),
			new Tuple<string, string>("x-arr-ssl", null),
			new Tuple<string, string>("X-Forwarded-Protocol", "https"),
			new Tuple<string, string>("X-Forwarded-Ssl", "on"),
			new Tuple<string, string>("X-Url-Scheme", "https")
		};

        /// <summary>
        /// Tries to get the <see cref="HttpRequestBase"/> instance without throwing exceptions
        /// </summary>
        /// <returns>The <see cref="HttpRequestBase"/> instance or <c>null</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HttpRequestBase SafeGetHttpRequest(this HttpContext httpContext)
		{
			if (httpContext == null)
			{
				return null;
			}

			return SafeGetHttpRequest(new HttpContextWrapper(httpContext));
		}

		/// <summary>
		/// Tries to get the <see cref="HttpRequestBase"/> instance without throwing exceptions
		/// </summary>
		/// <returns>The <see cref="HttpRequestBase"/> instance or <c>null</c>.</returns>
		public static HttpRequestBase SafeGetHttpRequest(this HttpContextBase httpContext)
		{
			if (httpContext == null)
			{
				return null;
			}

			if (httpContext.Handler != null || httpContext is FakeHttpContext)
			{
				// Having a handler means we're most likely in the MVC routing pipeline.
				return httpContext.Request;
			}

			try
			{
				return httpContext.Request;
			}
			catch
			{
				return null;
			}
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSecureConnection(this HttpRequest request)
        {
            return IsSecureConnection(new HttpRequestWrapper(request));
        }

		/// <summary>
		/// Gets a value which indicates whether the HTTP connection uses secure sockets (HTTPS protocol). 
		/// Works with Cloud's load balancers.
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		public static bool IsSecureConnection(this HttpRequestBase request)
		{
			if (request.IsSecureConnection)
			{
				return true;
			}

			foreach (var tuple in _sslHeaders)
			{
				var serverVar = request.ServerVariables[tuple.Item1];
				if (serverVar != null)
				{
					return tuple.Item2 == null || tuple.Item2.Equals(serverVar, StringComparison.OrdinalIgnoreCase);
				}
			}

			return false;
		}

		[SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetFormsAuthenticationCookie(this HttpWebRequest webRequest, HttpRequestBase httpRequest)
		{
			CopyCookie(webRequest, httpRequest, FormsAuthentication.FormsCookieName);
		}

		[SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
			return key.HasValue() ? CACHE_REGION_NAME + key : null;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetItem<T>(this HttpContext httpContext, string key, Func<T> factory = null, bool forceCreation = true)
		{
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
			var keys = cache.AllKeys(pattern);

			foreach (var key in keys.ToArray())
			{
				cache.Remove(key);
			}
		}

		public static string[] AllKeys(this Cache cache, string pattern)
		{
			pattern = pattern == "*" ? CACHE_REGION_NAME : pattern;

			var keys = from entry in HttpRuntime.Cache.AsParallel().Cast<DictionaryEntry>()
					   let key = entry.Key.ToString()
					   where key.StartsWith(pattern, StringComparison.OrdinalIgnoreCase)
					   select key;

			return keys.ToArray();
		}

		public static ControllerContext GetRootControllerContext(this ControllerContext controllerContext)
        {
            Guard.NotNull(controllerContext, nameof(controllerContext));

            var ctx = controllerContext;

            while (ctx.ParentActionViewContext != null)
            {
                ctx = ctx.ParentActionViewContext;
            }

            return ctx;
        }

        public static bool IsBareBonePage(this ControllerContext controllerContext)
        {
            var ctx = controllerContext.GetRootControllerContext();

            if (ctx is ViewContext viewContext)
            {
                // IsPopUp or Framed
                if (viewContext.ViewBag.IsPopup == true || viewContext.ViewBag.Framed == true)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
