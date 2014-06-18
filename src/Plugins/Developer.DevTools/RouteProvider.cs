using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Mvc.Routes;

namespace SmartStore.Plugin.Developer.DevTools
{
    
	public class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
			var route = routes.MapRoute("DevTools.MyCheckout",
				 "MyCheckout/{action}",
				 new { controller = "MyCheckout", action = "MyBillingAddress" },
				 new[] { "SmartStore.Plugin.Developer.DevTools.Controllers" }
            );
			route.DataTokens["area"] = "DevTools.DevTools";

			route = routes.MapRoute("FilterTest.Admin",
				 "Admin/DevTools/{action}/{id}",
				 new { controller = "DevToolsAdmin", action = "Index", id = UrlParameter.Optional },
				 new[] { "SmartStore.Plugin.Developer.DevTools.Controllers" }
			);
			route.DataTokens["area"] = "Developer.DevTools";
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
