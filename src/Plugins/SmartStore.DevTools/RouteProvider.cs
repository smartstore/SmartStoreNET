using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Routing;

namespace SmartStore.DevTools
{

    public class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("SmartStore.DevTools",
                 "Plugin/SmartStore.DevTools/{action}/{id}",
                 new { controller = "DevTools", action = "Configure", id = UrlParameter.Optional },
                 new[] { "SmartStore.DevTools.Controllers" }
            )
            .DataTokens["area"] = "SmartStore.DevTools";

            //routes.MapRoute("SmartStore.DevTools.MyCheckout",
            //	 "MyCheckout/{action}",
            //	 new { controller = "MyCheckout", action = "MyBillingAddress" },
            //	 new[] { "SmartStore.DevTools.Controllers" }
            //)
            //.DataTokens["area"] = "SmartStore.DevTools";
        }
        public int Priority => 0;
    }

}
