using SmartStore.Core.Data;
using SmartStore.Web.Framework.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace SmartStore.DevTools.OutputCache
{
	public interface IOutputCacheControlPolicy
	{
		bool IsRequestCacheable(ActionExecutingContext context);
		bool IsResultCacheable(ResultExecutedContext context);
		CacheableRoute GetCacheableRoute(string route);
	}

	public class OutputCacheControlPolicy : IOutputCacheControlPolicy
	{
		private static readonly string[] CacheableContentTypes = { "text/html", "text/xml", "text/json", "text/plain" };
		private readonly OutputCacheSettings _settings;

		public OutputCacheControlPolicy()
		{
			_settings = new OutputCacheSettings();
		}

		public virtual bool IsRequestCacheable(ActionExecutingContext context)
		{
			// TODO: NoCache when: Notifications are available, a redirect is in action, result is binary, OutputCacheAttribute is present
			// TODO: CacheControlAttribute

			if (!DataSettings.DatabaseIsInstalled())
				return false;

			if (!_settings.IsEnabled || _settings.DefaultCacheDuration < 1)
				return false;

			// don't cache child actions (another filter is responsible for processing donut holes)
			if (context.IsChildAction)
				return false;

			// don't cache request methods other than GET
			if (!context.HttpContext.Request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase))
				return false;

			// don't cache authenticated requests when 'CacheAuthenticatedRequests' is false
			if (!_settings.CacheAuthenticatedRequests && context.HttpContext.User.Identity.IsAuthenticated)
				return false;

			// don't cache when notifications are about to be shown
			if (context.Controller.TempData.ContainsKey(NotifyAttribute.NotificationsKey))
				return false;

			// don't cache admin pages
			if (context.HttpContext.Request.IsAdminArea())
				return false;

			// Respect OutputCacheAttribute if applied.
			var actionAttrs = context.ActionDescriptor.GetCustomAttributes(typeof(OutputCacheAttribute), true);
			var controllerAttrs = context.ActionDescriptor.ControllerDescriptor.GetCustomAttributes(typeof(OutputCacheAttribute), true);
			var attr = actionAttrs.Concat(controllerAttrs).Cast<OutputCacheAttribute>().FirstOrDefault();
			if (attr != null)
			{
				if (attr.Duration <= 0 || attr.NoStore)
					return false;
			}

			return true;
		}

		public virtual CacheableRoute GetCacheableRoute(string route)
		{
			return _settings.CacheableRoutes.FirstOrDefault(x => x.Route.IsCaseInsensitiveEqual(route));
		}

		public virtual bool IsResultCacheable(ResultExecutedContext context)
		{
			var result = context.Result;

			// only cache view results
			if (result is ViewResultBase)
				return true;

			// do not cache file results
			if (result is FileResult)
				return false;

			if (CacheableContentTypes.Contains(context.HttpContext.Response.ContentType))
				return true;

			return false;
		}
	}
}