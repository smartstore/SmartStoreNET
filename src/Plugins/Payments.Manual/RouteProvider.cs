using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Mvc.Routes;

namespace SmartStore.Plugin.Payments.Manual
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Payments.Manual.Configure",
                 "Plugins/PaymentManual/Configure",
                 new { controller = "PaymentManual", action = "Configure" },
                 new[] { "SmartStore.Plugin.Payments.Manual.Controllers" }
            );

            routes.MapRoute("Plugin.Payments.Manual.PaymentInfo",
                 "Plugins/PaymentManual/PaymentInfo",
                 new { controller = "PaymentManual", action = "PaymentInfo" },
                 new[] { "SmartStore.Plugin.Payments.Manual.Controllers" }
            );
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
