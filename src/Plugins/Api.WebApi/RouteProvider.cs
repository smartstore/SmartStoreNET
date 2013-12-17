using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Mvc.Routes;

namespace SmartStore.Plugin.Api.WebApi
{
	public partial class RouteProvider : IRouteProvider
	{
		public void RegisterRoutes(RouteCollection routes)
		{
            routes.MapRoute("Plugin.Api.WebApi.Action", "Plugins/WebApi/{action}", new { controller = "WebApi" }, new[] { "SmartStore.Plugin.Api.WebApi.Controllers" });
		}
		public int Priority
		{
			get
			{
				return 0;
			}
		}
	}
}
