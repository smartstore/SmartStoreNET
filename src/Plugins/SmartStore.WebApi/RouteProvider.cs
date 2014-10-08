using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Mvc.Routes;

namespace SmartStore.WebApi
{
	public partial class RouteProvider : IRouteProvider
	{
		public void RegisterRoutes(RouteCollection routes)
		{
            routes.MapRoute("SmartStore.WebApi.Action",
				"Plugins/SmartStore.WebApi/{action}", 
				new { controller = "WebApi" }, 
				new[] { "SmartStore.WebApi.Controllers" }
			)
			.DataTokens["area"] = "SmartStore.WebApi";
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
