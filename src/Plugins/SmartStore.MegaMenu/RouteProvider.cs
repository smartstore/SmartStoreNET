using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Routing;

namespace SmartStore.Plugin.MegaMenu
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("SmartStore.MegaMenu",
                 "Plugins/MegaMenu/{action}",
                 new { controller = "MegaMenu", action = "Configure" },
                 new[] { "SmartStore.MegaMenu.Controllers" }
            )
            .DataTokens["area"] = "SmartStore.MegaMenu";
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
