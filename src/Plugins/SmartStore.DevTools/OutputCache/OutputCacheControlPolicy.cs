using SmartStore.Core.Data;
using SmartStore.Web.Framework.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SmartStore.DevTools.OutputCache
{
	public interface IOutputCacheControlPolicy
	{
		bool IsRequestCacheable(ActionExecutingContext context);
		bool IsResultCacheable(ResultExecutedContext context);
	}

	public class OutputCacheControlPolicy : IOutputCacheControlPolicy
	{
		private static readonly string[] CacheableContentTypes = { "text/html", "text/xml", "text/json", "text/plain" };
		private readonly OutputCacheSettings _settings;

		public OutputCacheControlPolicy()
		{
			_settings = new OutputCacheSettings();
		}

		public bool IsRequestCacheable(ActionExecutingContext context)
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

			return true;
		}

		public bool IsResultCacheable(ResultExecutedContext context)
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