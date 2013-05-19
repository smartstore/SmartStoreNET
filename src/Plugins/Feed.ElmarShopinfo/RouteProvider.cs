using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Mvc.Routes;

namespace SmartStore.Plugin.Feed.ElmarShopinfo
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
			routes.MapRoute("Plugin.Feed.ElmarShopinfo.Configure",
				 "Plugins/FeedElmarShopinfo/Configure",
				 new { controller = "FeedElmarShopinfo", action = "Configure" },
				 new[] { "SmartStore.Plugin.Feed.ElmarShopinfo.Controllers" }
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