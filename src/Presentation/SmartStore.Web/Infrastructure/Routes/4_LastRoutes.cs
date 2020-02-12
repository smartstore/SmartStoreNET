using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Routing;

namespace SmartStore.Web.Infrastructure
{
    public partial class LastRoutes : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
			// TODO: actually this one's never reached, because the "GenericUrl" routes handles this.
			routes.MapLocalizedRoute("PageNotFound",
				"{*path}",
				new { controller = "Error", action = "NotFound" },
				new[] { "SmartStore.Web.Controllers" });
        }

        public int Priority
        {
            get
            {
                return int.MinValue;
            }
        }
    }
}
