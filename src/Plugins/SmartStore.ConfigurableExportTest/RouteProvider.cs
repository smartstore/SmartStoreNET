using SmartStore.Web.Framework.Routing;
using System.Web.Mvc;
using System.Web.Routing;

namespace SmartStore.Plugin.ConfigurableExport
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("SmartStore.ConfigurableExportTest",
                 "Plugins/ConfigurableExport/{action}",
                 new { controller = "ConfigurableExport", action = "Configure" },
                 new[] { "SmartStore.ConfigurableExport.Controllers" }
            )
            .DataTokens["area"] = "SmartStore.ConfigurableExportTest";
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
