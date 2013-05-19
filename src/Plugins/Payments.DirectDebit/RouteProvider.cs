using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Mvc.Routes;

namespace SmartStore.Plugin.Payments.DirectDebit
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Payments.DirectDebit.Configure",
                 "Plugins/PaymentDirectDebit/Configure",
                 new { controller = "PaymentDirectDebit", action = "Configure" },
                 new[] { "SmartStore.Plugin.Payments.DirectDebit.Controllers" }
            );

            routes.MapRoute("Plugin.Payments.DirectDebit.PaymentInfo",
                 "Plugins/PaymentDirectDebit/PaymentInfo",
                 new { controller = "PaymentDirectDebit", action = "PaymentInfo" },
                 new[] { "SmartStore.Plugin.Payments.DirectDebit.Controllers" }
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
