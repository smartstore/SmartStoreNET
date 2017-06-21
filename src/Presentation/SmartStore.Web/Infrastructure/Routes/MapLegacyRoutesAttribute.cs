using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Core.Data;
using SmartStore.Core.Infrastructure;
using SmartStore.Utilities;
using SmartStore.Web.Framework;

namespace SmartStore.Web.Infrastructure
{
	public class LegacyRoutesStarter : IStartupTask
	{
		internal const string MediaPattern = @"/media/(?<dir>thumbs|uploaded)/(?<path>.*?)$";

		public void Execute()
		{
			if (CommonHelper.GetAppSetting<bool>("sm:EnableLegacyRoutesMapping"))
			{
				SmartUrlRoutingModule.RegisterRoutablePath(MediaPattern, "GET");
			}
		}

		public int Order
		{
			get { return 0; }
		}
	}

	public sealed class MapLegacyRoutesAttribute : ActionFilterAttribute
	{
		private static readonly bool s_enabled = CommonHelper.GetAppSetting<bool>("sm:EnableLegacyRoutesMapping");
		private static readonly IList<MappedRoute> s_mappedRoutes = new List<MappedRoute>();

		static MapLegacyRoutesAttribute()
		{
			if (s_enabled)
			{
				// /Media/(Thumbs|Uploaded)/{*path} > /Media/Default/(Thumbs|Uploaded)/{*path}
				AddLegacyRoute(LegacyRoutesStarter.MediaPattern, "/media/" + DataSettings.Current.TenantName + "/${dir}/${path}", "GET");

				AddLegacyRoute(@"/productreviews/(?<id>\d+)$", "/product/reviews/${id}", "GET");
				AddLegacyRoute("/sitemapseo$", "/sitemap.xml", "GET");
				AddLegacyRoute("/config$", "/settings", "GET");
			}
		}

		private static void AddLegacyRoute(string path, string newPath, string verb = ".*")
		{
			var rgPath = new Regex(path, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
			var rgVerb = new Regex(verb, RegexOptions.CultureInvariant | RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);

			s_mappedRoutes.Add(new MappedRoute { LegacyPath = rgPath, NewPath = newPath, HttpMethod = rgVerb });
		}

		public override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			if (!s_enabled)
				return;

			if (!DataSettings.DatabaseIsInstalled())
				return;

			if (filterContext.IsChildAction)
				return;

			var mappedPath = GetMappedPath(filterContext.HttpContext.Request);
			if (mappedPath != null)
			{
				filterContext.Result = new RedirectResult(mappedPath, true);
			}
		}

		private string GetMappedPath(HttpRequestBase request)
		{
			var path = request.AppRelativeCurrentExecutionFilePath.TrimStart('~').TrimEnd('/');
			var method = request.HttpMethod.EmptyNull();

			foreach (var route in s_mappedRoutes)
			{
				if (route.LegacyPath.IsMatch(path) && route.HttpMethod.IsMatch(method))
				{
					var newPath = route.LegacyPath.Replace(path, route.NewPath);
					return newPath;
				}
			}

			return null;
		}

		private class MappedRoute
		{
			public Regex LegacyPath { get; set; }
			public string NewPath { get; set; }
			public Regex HttpMethod { get; set; }
		}

	}

}