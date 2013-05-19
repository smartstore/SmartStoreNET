using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Mvc.Routes;

namespace SmartStore.Plugin.Payments.Invoice
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Payments.Invoice.Configure",
                 "Plugins/PaymentInvoice/Configure",
                 new { controller = "PaymentInvoice", action = "Configure" },
                 new[] { "SmartStore.Plugin.Payments.Invoice.Controllers" }
            );

            routes.MapRoute("Plugin.Payments.Invoice.PaymentInfo",
                 "Plugins/PaymentInvoice/PaymentInfo",
                 new { controller = "PaymentInvoice", action = "PaymentInfo" },
                 new[] { "SmartStore.Plugin.Payments.Invoice.Controllers" }
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
