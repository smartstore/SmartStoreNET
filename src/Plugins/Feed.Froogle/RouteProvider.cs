using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Mvc.Routes;

namespace SmartStore.Plugin.Feed.Froogle
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Feed.Froogle",
                 "Plugins/FeedFroogle/{action}",
                 new { controller = "FeedFroogle", action = "Configure" },
                 new[] { "SmartStore.Plugin.Feed.Froogle.Controllers" }
            )
			.DataTokens["area"] = "PromotionFeed.Froogle";
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
