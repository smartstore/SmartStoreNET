using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace SmartStore.Web.Framework.Seo
{
    public static class GenericPathRouteExtensions
    {
        public static Route MapGenericPathRoute(this RouteCollection routes, string name, string url)
        {
            return MapGenericPathRoute(routes, name, url, null /* defaults */, (object)null /* constraints */);
        }

        public static Route MapGenericPathRoute(this RouteCollection routes, string name, string url, object defaults)
        {
            return MapGenericPathRoute(routes, name, url, defaults, (object)null /* constraints */);
        }

        public static Route MapGenericPathRoute(this RouteCollection routes, string name, string url, object defaults, object constraints)
        {
            return MapGenericPathRoute(routes, name, url, defaults, constraints, null /* namespaces */);
        }

        public static Route MapGenericPathRoute(this RouteCollection routes, string name, string url, string[] namespaces)
        {
            return MapGenericPathRoute(routes, name, url, null /* defaults */, null /* constraints */, namespaces);
        }

        public static Route MapGenericPathRoute(this RouteCollection routes, string name, string url, object defaults, string[] namespaces)
        {
            return MapGenericPathRoute(routes, name, url, defaults, null /* constraints */, namespaces);
        }

        public static Route MapGenericPathRoute(this RouteCollection routes, string name, string url, object defaults, object constraints, string[] namespaces)
        {
            Guard.NotNull(routes, nameof(routes));
            Guard.NotNull(url, nameof(url));

            var route = new GenericPathRoute(url, new MvcRouteHandler())
            {
                Defaults = new RouteValueDictionary(defaults),
                Constraints = new RouteValueDictionary(constraints),
                DataTokens = new RouteValueDictionary()
            };

            if ((namespaces != null) && (namespaces.Length > 0))
            {
                route.DataTokens["Namespaces"] = namespaces;
            }

            routes.Add(name, route);

            return route;
        }

        /// <summary>
        /// Changes the url pattern of an existing named route.
        /// </summary>
        /// <param name="routes">Route collection</param>
        /// <param name="name">Name of the route</param>
        /// <param name="url">The new url pattern</param>
        /// <returns>The route instance</returns>
        public static Route ChangeRouteUrl(this RouteCollection routes, string name, string url)
        {
            Guard.NotNull(routes, nameof(routes));
            Guard.NotEmpty(name, nameof(name));
            Guard.NotEmpty(url, nameof(url));

            var route = routes[name] as Route;

            if (route == null)
            {
                throw new ArgumentException("The route '{0}' does not exist or is not assignable from 'Route'.".FormatInvariant(name), nameof(name));
            }

            route.Url = url;

            return route;
        }
    }
}