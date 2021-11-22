using SmartStore.Web.Framework.Routing;
using System.Web.Mvc;
using System.Web.Routing;

namespace SmartStore.Plugin.StrubeExport
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("SmartStore.StrubeExport",
                 "Plugins/StrubeExport/{action}",
                 new { controller = "StrubeExport", action = "Configure" },
                 new[] { "SmartStore.StrubeExport.Controllers" }
            )
            .DataTokens["area"] = "SmartStore.StrubeExport";
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
