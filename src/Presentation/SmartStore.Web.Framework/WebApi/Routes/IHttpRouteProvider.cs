using System.Web.Http;

namespace SmartStore.Web.Framework.WebApi.Routes
{
    public interface IHttpRouteProvider
    {
        void RegisterRoutes(HttpRouteCollection routes);

        int Priority { get; }
    }
}
