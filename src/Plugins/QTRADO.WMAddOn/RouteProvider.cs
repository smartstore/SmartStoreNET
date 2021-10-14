using System.Web.Mvc;
using System.Web.Routing;

using SmartStore.Web.Framework.Routing;

namespace SmartStore.Plugin.WMAddOn
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("QTRADO.WMAddOn",
                 "Plugins/WMAddOn/{action}",
                 new { controller = "WMAddOn", action = "Configure" },
                 new[] { "SmartStore.WMAddOn.Controllers" }
            )
            .DataTokens["area"] = "QTRADO.WMAddOn";
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
