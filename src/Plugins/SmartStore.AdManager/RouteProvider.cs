using SmartStore.Web.Framework.Routing;
using System.Web.Mvc;
using System.Web.Routing;

namespace SmartStore.Plugin.AdManager
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("SmartStore.AdManager",
                 "Plugins/AdManager/{action}",
                 new { controller = "AdManager", action = "Configure" },
                 new[] { "SmartStore.AdManager.Controllers" }
            )
            .DataTokens["area"] = "SmartStore.AdManager";
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
