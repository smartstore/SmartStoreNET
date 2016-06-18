using SmartStore.Core.Caching;
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
		private const string CacheKeyPrefix = "OutputCache://";

		private readonly ICacheManager _cache;
		private readonly DonutHoleProcessor _donutHoleProcessor;
		private bool _isCachingRequest;

		public OutputCacheFilter(Func<string, ICacheManager> cache)
		{
			_cache = cache("static");
			_donutHoleProcessor = new DonutHoleProcessor();
		}

		public virtual void OnException(ExceptionContext filterContext)
		{
			ExecuteCallback(filterContext, true);
		}

		public virtual void OnActionExecuting(ActionExecutingContext filterContext)
		{
			if (filterContext.IsChildAction)
				return;

			if (!filterContext.HttpContext.Request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase))
				return;

			//if (filterContext.HttpContext.User.Identity.IsAuthenticated)
			//	return;

			if (filterContext.HttpContext.Request.IsAdminArea())
				return;

			dynamic cacheSettings = new ExpandoObject();
			cacheSettings.IsServerCachingEnabled = true;
			cacheSettings.Duration = 300;

			dynamic cacheControlStrategy = new ExpandoObject();
			cacheControlStrategy.IsServerCachingEnabled = true;

			var cacheKey = ComputeCacheKey(filterContext, GetCacheKeyParameters(filterContext));

			// If we are unable to generate a cache key it means we can't do anything
			if (string.IsNullOrEmpty(cacheKey))
			{
				return;
			}

			// Are we actually storing data on the server side ?
			if (cacheSettings.IsServerCachingEnabled)
			{
				OutputCacheItem cachedItem = null;

				_cache.TryGet(cacheKey, out cachedItem);

				// We have a cached version on the server side
				if (cachedItem != null)
				{
					// We inject the previous result into the MVC pipeline
					// The MVC action won't execute as we injected the previous cached result.
					filterContext.Result = new ContentResult
					{
						Content = _donutHoleProcessor.ReplaceHole(cachedItem.Content, filterContext),
						ContentType = cachedItem.ContentType
					};
				}
			}

			// Did we already injected something ?
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

			// Will be called back by OnResultExecuted -> ExecuteCallback
			filterContext.HttpContext.Items[cacheKey] = new Action<bool>(hasErrors =>
			{
				// Removing this executing action from the context
				filterContext.HttpContext.Items.Remove(cacheKey);

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

				filterContext.HttpContext.Response.Write(_donutHoleProcessor.RemoveHoleWrappers(cacheItem.Content, filterContext));

				if (cacheSettings.IsServerCachingEnabled && filterContext.HttpContext.Response.StatusCode == 200)
				{
					_cache.Set(cacheKey, cacheItem, (int)cacheSettings.Duration);
				}
			});
		}

		public void OnActionExecuted(ActionExecutedContext filterContext)
		{
		}

		public void OnResultExecuting(ResultExecutingContext filterContext)
		{
		}

		public virtual void OnResultExecuted(ResultExecutedContext filterContext)
		{
			if (!_isCachingRequest)
			{
				return;
			}

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
			var cacheKey = ComputeCacheKey(context, GetCacheKeyParameters(context));

			if (string.IsNullOrEmpty(cacheKey))
			{
				return;
			}

			var callback = context.HttpContext.Items[cacheKey] as Action<bool>;

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
			response.Cache.SetExpires(DateTime.Now.AddSeconds(300));
			response.Cache.SetMaxAge(new TimeSpan(0, 0, 300));

			//response.Cache.SetNoStore();
		}

		private string ComputeCacheKey(ControllerContext context, IDictionary<string, object> parameters)
		{
			string areaName = context.RouteData.GetAreaName();
			string controllerName = context.RouteData.Values["controller"].ToString();
			string actionName = context.RouteData.Values["action"].ToString();

			if (string.IsNullOrEmpty(actionName) || string.IsNullOrEmpty(controllerName))
			{
				return null;
			}

			var builder = new StringBuilder(CacheKeyPrefix);

			if (areaName != null)
			{
				builder.AppendFormat("{0}/", areaName.ToLowerInvariant());
			}

			if (controllerName != null)
			{
				builder.AppendFormat("{0}/", controllerName.ToLowerInvariant());
			}

			if (actionName != null)
			{
				builder.AppendFormat("{0}#", actionName.ToLowerInvariant());
			}

			if (parameters != null)
			{
				foreach (var p in parameters)
				{
					builder.Append(BuildKeyFragment(p));
				}
			}

			return builder.ToString();
		}

		public string BuildKeyFragment(KeyValuePair<string, object> fragment)
		{
			var value = fragment.Value == null ? "<null>" : fragment.Value.ToString().ToLowerInvariant();

			return string.Format("{0}={1}#", fragment.Key.ToLowerInvariant(), value);
		}

		protected virtual IDictionary<string, object> GetCacheKeyParameters(ControllerContext context)
		{
			return new Dictionary<string, object>(context.RouteData.Values);
		}
	}
}