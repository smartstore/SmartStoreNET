using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Services;
using SmartStore.Web.Framework.Theming;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace SmartStore.DevTools.OutputCache
{
	public class OutputCacheFilter : IActionFilter, IResultFilter, IExceptionFilter
	{
		private readonly ICacheManager _cache;
		private readonly ICommonServices _services;
		private readonly IThemeContext _themeContext;
		private readonly DonutHoleProcessor _donutHoleProcessor;
		private readonly OutputCacheSettings _settings;
		private readonly IOutputCacheControlPolicy _policy;

		private bool _isCachingRequest;
		private string _cacheKey;

		public OutputCacheFilter(
			Func<string, ICacheManager> cache, 
			ICommonServices services,
			IThemeContext themeContext)
		{
			_cache = cache("static");
			_services = services;
			_themeContext = themeContext;
			_donutHoleProcessor = new DonutHoleProcessor();
			_settings = new OutputCacheSettings();
			_policy = new OutputCacheControlPolicy();
		}

		public virtual void OnException(ExceptionContext filterContext)
		{
			ExecuteCallback(filterContext, true);
		}

		public virtual void OnActionExecuting(ActionExecutingContext filterContext)
		{
			if (!_policy.IsRequestCacheable(filterContext))
				return;

			// TODO: Debug mode

			var routeKey = CacheHelpers.GetRouteKey(filterContext);

			// Get out if we are unable to generate a route key (Area/Controller/Action)
			if (string.IsNullOrEmpty(routeKey))
				return;

			var cacheableRoute = _policy.GetCacheableRoute(routeKey);

			// do not cache when route is not in white list or its caching duration is <= 0
			if (cacheableRoute == null || (cacheableRoute.Duration.HasValue && cacheableRoute.Duration.Value < 1))
				return;

			_cacheKey = String.Intern(CacheHelpers.ComputeCacheKey(routeKey, GetCacheKeyParameters(filterContext)));

			// If we are unable to generate a cache key it means we can't do anything
			if (string.IsNullOrEmpty(_cacheKey))
				return;

			// Are we actually storing data on the server side?
			OutputCacheItem cachedItem = null;

			_cache.TryGet(_cacheKey, out cachedItem);

			// We have a cached version on the server side
			if (cachedItem != null)
			{
				// We inject the previous result into the MVC pipeline
				// The MVC action won't execute as we injected the previous cached result.
				filterContext.Result = new ContentResult
				{
					Content = _donutHoleProcessor.ReplaceHoles(cachedItem.Content, filterContext),
					ContentType = cachedItem.ContentType
				};
			}

			// Did we already inject something ?
			if (filterContext.Result != null)
			{
				return; // No need to continue 
			}

			// We are hooking into the pipeline to replace the response Output writer
			// by something we own and later eventually gonna cache
			var cachingWriter = new StringWriter(CultureInfo.InvariantCulture);

			var originalWriter = filterContext.HttpContext.Response.Output;

			filterContext.HttpContext.Response.Output = cachingWriter;

			_isCachingRequest = true;
			filterContext.HttpContext.Items["OutputCache.IsCachingRequest"] = true;

			// Will be called back by OnResultExecuted -> ExecuteCallback
			filterContext.HttpContext.Items[_cacheKey] = new Action<bool>(hasErrors =>
			{
				// Removing this executing action from the context
				filterContext.HttpContext.Items.Remove(_cacheKey);

				// We restore the original writer for response
				filterContext.HttpContext.Response.Output = originalWriter;

				if (hasErrors)
				{
					return; // Something went wrong, we are not going to cache something bad
				}

				// Now we use owned caching writer to actually store data
				var cacheItem = new OutputCacheItem
				{
					Content = cachingWriter.ToString(),
					ContentType = filterContext.HttpContext.Response.ContentType
				};

				filterContext.HttpContext.Response.Write(_donutHoleProcessor.ReplaceHoles(cacheItem.Content, filterContext));

				if (filterContext.HttpContext.Response.StatusCode == 200)
				{
					_cache.Set(_cacheKey, cacheItem, cacheableRoute.Duration ?? _settings.DefaultCacheDuration);
				}
			});
		}

		public void OnActionExecuted(ActionExecutedContext filterContext)
		{
			// nada
		}

		public void OnResultExecuting(ResultExecutingContext filterContext)
		{
			// nada
		}

		public virtual void OnResultExecuted(ResultExecutedContext filterContext)
		{
			if (!_isCachingRequest)
				return;

			if (!_policy.IsResultCacheable(filterContext))
				return;

			// See OnActionExecuting
			ExecuteCallback(filterContext, filterContext.Exception != null);

			// The main action is responsible for setting the right HTTP cache headers for the final response.
			SetCacheHeaders(filterContext.HttpContext.Response);
		}

		/// <summary>
		/// Executes the callback.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="hasErrors">if set to <c>true</c> [has errors].</param>
		private void ExecuteCallback(ControllerContext context, bool hasErrors)
		{
			if (string.IsNullOrEmpty(_cacheKey))
			{
				return;
			}

			var callback = context.HttpContext.Items[_cacheKey] as Action<bool>;

			if (callback != null)
			{
				callback.Invoke(hasErrors);
			}
		}

		/// <summary>
		/// Sets the cache headers for the HTTP response given <see cref="settings" />.
		/// </summary>
		/// <param name="response">The HTTP response.</param>
		protected virtual void SetCacheHeaders(HttpResponseBase response)
		{
			response.Cache.SetCacheability(HttpCacheability.Public);
			response.Cache.SetExpires(DateTime.Now.AddSeconds(_settings.DefaultCacheDuration));
			response.Cache.SetMaxAge(new TimeSpan(0, 0, _settings.DefaultCacheDuration));

			//response.Cache.SetNoStore();
		}

		protected virtual IDictionary<string, object> GetCacheKeyParameters(ActionExecutingContext context)
		{
			var result = new Dictionary<string, object>();

			// Vary by action parameters.
			foreach (var p in context.ActionParameters)
			{
				result.Add(p.Key, p.Value);
			}

			// Vary by query string parameters.
			foreach (var key in context.HttpContext.Request.QueryString.AllKeys)
			{
				if (key == null || result.ContainsKey(key.ToLowerInvariant()))
				{
					// already added as ActionParamater per mvc model binding
					continue;
				}

				var item = context.HttpContext.Request.QueryString[key];
				result.Add(
					key.ToLowerInvariant(),
					item != null
						? item.ToLowerInvariant()
						: string.Empty
				);
			}

			// Vary by customer roles 'CacheAuthenticatedRequests' is true
			if (context.HttpContext.User.Identity.IsAuthenticated)
			{
				var roleIds = _services.WorkContext.CurrentCustomer.CustomerRoles.Where(x => x.Active).Select(x => x.Id);
				result.Add("$roles", String.Join(",", roleIds));
			}

			// Vary by language
			result.Add("$lang", _services.WorkContext.WorkingLanguage.Id);

			// Vary by currency
			result.Add("$cur", _services.WorkContext.WorkingCurrency.Id);

			// Vary by store
			result.Add("$store", _services.StoreContext.CurrentStore.Id);

			// Vary by theme
			result.Add("$theme", _themeContext.CurrentTheme.ThemeName);

			return result;
		}
	}
}