using System.Web.Mvc;
using System.Web.Routing;

namespace SmartStore.Web.Framework.Localization
{
    public static class LocalizedRouteExtensions
    {
        #region RouteCollection extensions

        public static Route MapLocalizedRoute(this RouteCollection routes, string name, string url)
        {
            return MapLocalizedRoute(routes, name, url, null /* defaults */, (object)null /* constraints */);
        }

        public static Route MapLocalizedRoute(this RouteCollection routes, string name, string url, object defaults)
        {
            return MapLocalizedRoute(routes, name, url, defaults, (object)null /* constraints */);
        }

        public static Route MapLocalizedRoute(this RouteCollection routes, string name, string url, object defaults, object constraints)
        {
            return MapLocalizedRoute(routes, name, url, defaults, constraints, null /* namespaces */);
        }

        public static Route MapLocalizedRoute(this RouteCollection routes, string name, string url, string[] namespaces)
        {
            return MapLocalizedRoute(routes, name, url, null /* defaults */, null /* constraints */, namespaces);
        }

        public static Route MapLocalizedRoute(this RouteCollection routes, string name, string url, object defaults, string[] namespaces)
        {
            return MapLocalizedRoute(routes, name, url, defaults, null /* constraints */, namespaces);
        }

        public static Route CreateLocalizedRoute(this RouteCollection routes, string url, object defaults, string[] namespaces)
        {
            return MapLocalizedRouteInternal(routes, null /* name */, url, defaults, null /* constraints */, namespaces, false);
        }

        public static Route MapLocalizedRoute(this RouteCollection routes, string name, string url, object defaults, object constraints, string[] namespaces)
        {
            return MapLocalizedRouteInternal(routes, name, url, defaults, constraints, namespaces, true);
        }

        private static Route MapLocalizedRouteInternal(this RouteCollection routes, string name, string url, object defaults, object constraints, string[] namespaces, bool add)
        {
            Guard.NotNull(routes, nameof(routes));
            Guard.NotNull(url, nameof(url));

            var route = new LocalizedRoute(url, new MvcRouteHandler())
            {
                Defaults = new RouteValueDictionary(defaults),
                Constraints = new RouteValueDictionary(constraints),
                DataTokens = new RouteValueDictionary()
            };

            if ((namespaces != null) && (namespaces.Length > 0))
            {
                route.DataTokens["Namespaces"] = namespaces;
            }

            if (add)
            {
                routes.Add(name, route);
            }

            return route;
        }

        #endregion
    }
}