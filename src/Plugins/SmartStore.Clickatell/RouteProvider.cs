using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Mvc.Routes;

namespace SmartStore.Clickatell
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("SmartStore.Clickatell",
				 "Plugins/SmartStore.Clickatell/{action}",
                 new { controller = "SmsClickatell", action = "Configure" },
                 new[] { "SmartStore.Clickatell.Controllers" }
            )
			.DataTokens["area"] = "SmartStore.Clickatell";
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
