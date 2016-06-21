using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace SmartStore.DevTools.OutputCache
{
	internal static class CacheHelpers
	{
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
	}
}