using System.Web.Http;

namespace SmartStore.Web.Framework.WebApi.Routes
{
    public interface IHttpRoutePublisher
    {
        void RegisterRoutes(HttpRouteCollection routeCollection);
    }
}
