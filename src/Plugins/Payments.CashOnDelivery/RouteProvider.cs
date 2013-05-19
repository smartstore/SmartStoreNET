using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Mvc.Routes;

namespace SmartStore.Plugin.Payments.CashOnDelivery
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Payments.CashOnDelivery.Configure",
                 "Plugins/PaymentCashOnDelivery/Configure",
                 new { controller = "PaymentCashOnDelivery", action = "Configure" },
                 new[] { "SmartStore.Plugin.Payments.CashOnDelivery.Controllers" }
            );

            routes.MapRoute("Plugin.Payments.CashOnDelivery.PaymentInfo",
                 "Plugins/PaymentCashOnDelivery/PaymentInfo",
                 new { controller = "PaymentCashOnDelivery", action = "PaymentInfo" },
                 new[] { "SmartStore.Plugin.Payments.CashOnDelivery.Controllers" }
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
