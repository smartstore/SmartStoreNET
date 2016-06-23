using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;

namespace SmartStore.DevTools.OutputCache
{
	internal static class CacheUtility
	{
		private static readonly Regex s_rgDonutHoles = new Regex("<!--Donut#(.*?)#-->(.*?)<!--EndDonut-->", RegexOptions.Compiled | RegexOptions.Singleline);

		const string CacheKeyPrefix = "OutputCache://";

		public static string GetRouteKey(ActionExecutingContext context)
		{
			string areaName = context.RouteData.GetAreaName();
			string controllerName = context.ActionDescriptor.ControllerDescriptor.ControllerName;
			string actionName = context.ActionDescriptor.ActionName;

			if (string.IsNullOrEmpty(actionName) || string.IsNullOrEmpty(controllerName))
			{
				return null;
			}

			var builder = new StringBuilder();

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
				builder.AppendFormat("{0}", actionName.ToLowerInvariant());
			}

			return builder.ToString();
		}

		public static string ComputeCacheKey(string routeKey, IDictionary<string, object> parameters)
		{
			Guard.ArgumentNotNull(() => routeKey);

			var builder = new StringBuilder(CacheKeyPrefix + routeKey);

			if (parameters != null)
			{
				builder.Append("#");
				foreach (var p in parameters)
				{
					builder.Append(BuildKeyFragment(p));
				}
			}

			return builder.ToString();
		}

		private static string BuildKeyFragment(KeyValuePair<string, object> fragment)
		{
			var value = fragment.Value == null ? "<null>" : fragment.Value.ToString().ToLowerInvariant();

			return string.Format("{0}={1}#", fragment.Key.ToLowerInvariant(), value);
		}

		public static string ReplaceDonutHoles(string content, ControllerContext filterContext)
		{
			return s_rgDonutHoles.Replace(content, match =>
			{
				var actionSettings = JsonConvert.DeserializeObject<ChildActionData>(match.Groups[1].Value);
				var result = InvokeAction(
					filterContext.Controller,
					actionSettings.ActionName,
					actionSettings.ControllerName,
					actionSettings.RouteValues
				);

				return result;
			});
		}

		private static string InvokeAction(ControllerBase controller, string actionName, string controllerName, RouteValueDictionary routeValues)
		{
			var viewContext = new ViewContext(
				controller.ControllerContext,
				new WebFormView(controller.ControllerContext, "tmp"),
				controller.ViewData,
				controller.TempData,
				TextWriter.Null
			);

			var htmlHelper = new HtmlHelper(viewContext, new ViewPage());

			routeValues["OutputCache.InvokingChildAction"] = true;

			return htmlHelper.Action(actionName, controllerName, routeValues).ToString();
		}
	}
}