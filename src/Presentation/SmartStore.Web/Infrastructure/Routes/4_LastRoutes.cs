using System.Web.Routing;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Routing;
using SmartStore.Web.Framework.Seo;

namespace SmartStore.Web.Infrastructure
{
    public partial class GenericPathRoutes : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            // Inner routes from GenericPathRoute solely needed for URL creation, NOT for route matching.
            foreach (var path in GenericPathRoute.Paths)
            {
                routes.Add(path.EntityName, path.Route);
            }
        }

        public int Priority { get; } = int.MinValue + 50;
    }

    public partial class LastRoute : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            // TODO: actually this one's never reached, because the "GenericUrl" routes handles this.
            routes.MapLocalizedRoute("PageNotFound",
                "{*path}",
                new { controller = "Error", action = "NotFound" },
                new[] { "SmartStore.Web.Controllers" });
        }

        public int Priority { get; } = int.MinValue;
    }
}
