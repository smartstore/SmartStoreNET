using System.Web.Routing;

namespace SmartStore.Web.Framework.Routing
{
    public interface IRoutePublisher
    {
        void RegisterRoutes(RouteCollection routeCollection);
    }
}
