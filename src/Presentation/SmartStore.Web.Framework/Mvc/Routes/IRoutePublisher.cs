using System.Web.Routing;

namespace SmartStore.Web.Framework.Mvc.Routes
{
    public interface IRoutePublisher
    {
        void RegisterRoutes(RouteCollection routeCollection);
    }
}
