using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Routing;
using SmartStore.Core.Infrastructure;

namespace SmartStore.Web.Framework.WebApi.Routes
{
    public class HttpRoutePublisher : IHttpRoutePublisher
    {
        private readonly ITypeFinder _typeFinder;

        public HttpRoutePublisher(ITypeFinder typeFinder)
        {
            this._typeFinder = typeFinder;
        }

        public void RegisterRoutes(HttpRouteCollection routes)
        {
            var routeProviderTypes = _typeFinder.FindClassesOfType<IHttpRouteProvider>();
            var routeProviders = new List<IHttpRouteProvider>();
            foreach (var providerType in routeProviderTypes)
            {
                var provider = Activator.CreateInstance(providerType) as IHttpRouteProvider;
                routeProviders.Add(provider);
            }
            routeProviders = routeProviders.OrderByDescending(rp => rp.Priority).ToList();
            routeProviders.ForEach(rp => rp.RegisterRoutes(routes));
        }
    }
}
