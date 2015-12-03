using System.Web.Routing;

namespace SmartStore.Web.Framework.Routing
{
    public interface IRouteProvider
    {
        void RegisterRoutes(RouteCollection routes);

        int Priority { get; }
    }
}
