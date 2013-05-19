using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Mvc.Routes;

namespace SmartStore.Plugin.SMS.Clickatell
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.SMS.Clickatell.Configure",
                 "Plugins/SMSClickatell/Configure",
                 new { controller = "SmsClickatell", action = "Configure" },
                 new[] { "SmartStore.Plugin.SMS.Clickatell.Controllers" }
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
