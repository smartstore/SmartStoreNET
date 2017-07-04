using System;
using System.Web.Mvc;
using System.Web.Routing;

// use base SmartStore Namespace to ensure the extension methods are always available
namespace SmartStore
{
	public static class RouteExtensions
	{
		public static string GetAreaName(this RouteData routeData)
		{
			object area;
			if (routeData.DataTokens.TryGetValue("area", out area))
			{
				return (area as string);
			}
			return routeData.Route.GetAreaName();
		}

		public static string GetAreaName(this RouteBase route)
		{
			var area = route as IRouteWithArea;
			if (area != null)
			{
				return area.Area;
			}
			var route2 = route as Route;
			if ((route2 != null) && (route2.DataTokens != null))
			{
				return (route2.DataTokens["area"] as string);
			}
			return null;
		}

		/// <summary>
		/// Generates an identifier for the given route in the form "[{area}.]{controller}.{action}"
		/// </summary>
		public static string GenerateRouteIdentifier(this RouteData routeData)
		{
			string area = routeData.GetAreaName();
			string controller = routeData.GetRequiredString("controller");
			string action = routeData.GetRequiredString("action");

			return "{0}{1}.{2}".FormatInvariant(area.HasValue() ? area + "." : "", controller, action);
		}

		public static bool IsRouteEqual(this RouteData routeData, string controller, string action)
		{
			if (routeData == null)
				return false;

			return routeData.GetRequiredString("controller").IsCaseInsensitiveEqual(controller) && routeData.GetRequiredString("action").IsCaseInsensitiveEqual(action);
		}

	}
}
