using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Mvc.Routes;

namespace SmartStore.GoogleMerchantCenter
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
			routes.MapRoute("SmartStore.GoogleMerchantCenter",
				 "Plugins/SmartStore.GoogleMerchantCenter/{action}",
                 new { controller = "FeedFroogle", action = "Configure" },
				 new[] { "SmartStore.GoogleMerchantCenter.Controllers" }
            )
			.DataTokens["area"] = "SmartStore.GoogleMerchantCenter";
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
