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
		private readonly MemoryOutputCacheProvider _outputCache;
		private readonly ICommonServices _services;
		private readonly IThemeContext _themeContext;
		private readonly OutputCacheSettings _settings;
		private readonly IOutputCacheControlPolicy _policy;
		private readonly IDisplayedEntities _displayedEntities;

		private bool _isCachingRequest;
		private string _routeKey;
		private string _cacheKey;
		private CacheableRoute _cacheableRoute;
		private DateTime _now;

		public OutputCacheFilter(
			ICommonServices services,
			IThemeContext themeContext,
			IDisplayedEntities displayedEntities)
		{
			_outputCache = new MemoryOutputCacheProvider();
			_services = services;
			_themeContext = themeContext;
			_settings = new OutputCacheSettings();
			_policy = new OutputCacheControlPolicy();
			_displayedEntities = displayedEntities;
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

			_now = DateTime.UtcNow;

			_routeKey = CacheUtility.GetRouteKey(filterContext);

			// Get out if we are unable to generate a route key (Area/Controller/Action)
			if (string.IsNullOrEmpty(_routeKey))
				return;

			_cacheableRoute = _policy.GetCacheableRoute(_routeKey);

			// do not cache when route is not in white list or its caching duration is <= 0
			if (_cacheableRoute == null || (_cacheableRoute.Duration.HasValue && _cacheableRoute.Duration.Value < 1))
				return;

			_cacheKey = String.Intern(CacheUtility.ComputeCacheKey(_routeKey, GetCacheKeyParameters(filterContext)));

			// If we are unable to generate a cache key it means we can't do anything
			if (string.IsNullOrEmpty(_cacheKey))
				return;

			// Are we actually storing data on the server side?
			var cachedItem = _outputCache.Get(_cacheKey);

			var response = filterContext.HttpContext.Response;

			// We have a cached version on the server side
			if (cachedItem != null)
			{
				// Adds some debug information to the response header if requested.
				if (_settings.DebugMode)
				{
					response.AddHeader("X-Cached-On", cachedItem.CachedOnUtc.ToString("r"));
					response.AddHeader("X-Cached-Until", cachedItem.ValidUntilUtc.ToString("r"));
				}

				// We inject the previous result into the MVC pipeline
				// The MVC action won't execute as we injected the previous cached result.
				filterContext.Result = new ContentResult
				{
					Content = CacheUtility.ReplaceDonutHoles(cachedItem.Content, filterContext),
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

			var originalWriter = response.Output;

			response.Output = cachingWriter;

			_isCachingRequest = true;
			filterContext.HttpContext.Items["OutputCache.IsCachingRequest"] = true;

			// Will be called back by OnResultExecuted -> ExecuteCallback
			filterContext.HttpContext.Items[_cacheKey] = new Action<bool>(hasErrors =>
			{
				// Removing this executing action from the context
				filterContext.HttpContext.Items.Remove(_cacheKey);

				// We restore the original writer for response
				response.Output = originalWriter;

				if (hasErrors)
				{
					return; // Something went wrong, we are not going to cache something bad
				}

				var shouldSaveItem = true;

				// Page might now have been rendered and cached by another request: double check!
				var cacheItem = _outputCache.Get(_cacheKey);
				if (cacheItem == null)
				{
					cacheItem = new OutputCacheItem
					{
						CacheKey = _cacheKey,
						RouteKey = _routeKey,
						CachedOnUtc = _now,
						Url = filterContext.HttpContext.Request.Url.AbsolutePath,
						QueryString = filterContext.HttpContext.Request.Url.Query,
						Duration = _cacheableRoute.Duration ?? _settings.DefaultCacheDuration,
						Tags = _displayedEntities.GetCacheControlTags().ToArray(),
						Theme = "", // TODO
						StoreId = 0, // TODO
						LanguageId = 0, // TODO
						CurrencyId = 0, // TODO
						CustomerRoles = null, // TODO
						Content = cachingWriter.ToString(),
						ContentType = response.ContentType,
					};
				}
				else
				{
					shouldSaveItem = false;
				}

				response.Write(CacheUtility.ReplaceDonutHoles(cacheItem.Content, filterContext));

				if (response.StatusCode == 200 && shouldSaveItem)
				{
					_outputCache.Set(_cacheKey, cacheItem);
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
			var duration = _cacheableRoute.Duration ?? _settings.DefaultCacheDuration;

			response.Cache.SetCacheability(HttpCacheability.Public);
			response.Cache.SetExpires(DateTime.Now.AddSeconds(duration));
			response.Cache.SetMaxAge(new TimeSpan(0, 0, duration));

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