using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Plugins;
using SmartStore.Web.Framework.Localization;

namespace SmartStore.Web.Framework.Mvc.Routes
{
    public class RoutePublisher : IRoutePublisher
    {
        private readonly ITypeFinder _typeFinder;

        public RoutePublisher(ITypeFinder typeFinder)
        {
            this._typeFinder = typeFinder;
        }

        public void RegisterRoutes(RouteCollection routes)
        {
			var routeProviderTypes = _typeFinder.FindClassesOfType<IRouteProvider>(ignoreInactivePlugins: true);
            var routeProviders = new List<IRouteProvider>();

            foreach (var providerType in routeProviderTypes)
            {
                var provider = Activator.CreateInstance(providerType) as IRouteProvider;
                routeProviders.Add(provider);
            }
            routeProviders = routeProviders.OrderByDescending(rp => rp.Priority).ToList();
            routeProviders.Each(rp => 
			{
				rp.RegisterRoutes(routes);
			});
        }
    }
}
