using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Mvc.Routes;

namespace SmartStore.Plugin.SMS.Clickatell
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("SMS.Clickatell",
                 "Plugins/SMSClickatell/{action}",
                 new { controller = "SmsClickatell", action = "Configure" },
                 new[] { "SmartStore.Plugin.SMS.Clickatell.Controllers" }
            )
			.DataTokens["area"] = "SMS.Clickatell";
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
