using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

namespace SmartStore.Web.Framework.Localization
{

    public static class LocalizedRouteExtensions
    {
        private const string CULTURECODE_TOKEN = "cultureCode";

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

        public static Route MapLocalizedRoute(this RouteCollection routes, string name, string url, object defaults, object constraints, string[] namespaces)
        {
            if (routes == null)
            {
                throw new ArgumentNullException("routes");
            }
            if (url == null)
            {
                throw new ArgumentNullException("url");
            }

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

            routes.Add(name, route);

            return route;
        }

        /// <summary>
        /// Modifies routes of type <see cref="LocalizedRoute"/> so that
        /// seo friendly urls are not matched and handled anymore.
        /// </summary>
        public static void DisableSeoFriendlyUrls(this RouteCollection routes)
        {
            Guard.ArgumentNotNull(() => routes);

            string urlToken = "{" + CULTURECODE_TOKEN + "}";

            foreach (var route in routes.OfType<LocalizedRoute>())
            {
                // 'reduce' url pattern
                var url = route.Url;
                if (url.StartsWith(urlToken))
                {
                    url = url.Substring(urlToken.Length + 1);
                    route.Url = url;
                }

                // remove CultureCodeConstraint
                if (route.Constraints != null && route.Constraints.ContainsKey(CULTURECODE_TOKEN))
                {
                    route.Constraints.Remove(CULTURECODE_TOKEN);
                }

                // remove default cultureCode from DEFAULTS
                if (route.Defaults != null && route.Defaults.ContainsKey(CULTURECODE_TOKEN))
                {
                    route.Defaults.Remove(CULTURECODE_TOKEN);
                }

                // set route handler back to default MvcRouteHandler;
                if (route.RouteHandler.GetType() == typeof(LocalizedRouteHandler))
                {
                    route.RouteHandler = new MvcRouteHandler();
                }

                route.SeoFriendlyUrlsEnabled = false;
                route.DefaultCultureCode = null;
            }
        }

        /// <summary>
        /// Modifies routes of type <see cref="LocalizedRoute"/> so that
        /// seo friendly urls are matched and handled.
        /// </summary>
        /// <param name="allowedCultureCodes">
        /// A list of culture codes that should be accepted during a request.
        /// The first entry is considered the default culture.
        /// </param>
        public static void EnableSeoFriendlyUrls(this RouteCollection routes, IEnumerable<string> allowedCultureCodes)
        {
            Guard.ArgumentNotNull(() => routes);
            Guard.ArgumentNotNull(() => allowedCultureCodes);

            if (!allowedCultureCodes.Any())
            {
                throw Error.Argument("allowedCultureCodes", "The list of allowed culture codes must contain at least one item.");
            }

            string urlToken = "{" + CULTURECODE_TOKEN + "}";
            string defaultCultureCode = allowedCultureCodes.First();
            var constraint = new CultureCodeConstraint(allowedCultureCodes.ToArray());

            foreach (var route in routes.OfType<LocalizedRoute>())
            {
                // add culture token to url pattern
                var url = route.Url;
                if (!url.StartsWith(urlToken))
                {
                    url = "{0}/{1}".FormatInvariant(urlToken, url);
                    route.Url = url;
                }
                
                // add CultureCodeConstraint
                if (route.Constraints == null)
                {
                    route.Constraints = new RouteValueDictionary();
                }
                if (route.Constraints.ContainsKey(CULTURECODE_TOKEN))
                {
                    route.Constraints.Remove(CULTURECODE_TOKEN);
                }
                route.Constraints.Add(CULTURECODE_TOKEN, constraint);

                // add default cultureCode to DEFAULTS
                if (route.Defaults == null)
                {
                    route.Defaults = new RouteValueDictionary();
                }
                if (route.Defaults.ContainsKey(CULTURECODE_TOKEN))
                {
                    route.Defaults.Remove(CULTURECODE_TOKEN);
                }
                route.Defaults.Add(CULTURECODE_TOKEN, "default");

                // set specialized LocalizedRouteHandler
                if (route.RouteHandler.GetType() != typeof(LocalizedRouteHandler))
                {
                    route.RouteHandler = new LocalizedRouteHandler();
                }

                route.SeoFriendlyUrlsEnabled = true;
                route.DefaultCultureCode = defaultCultureCode;
            }
        }
        
        public static void ClearSeoFriendlyUrlsCachedValueForRoutes(this RouteCollection routes)
        {
            if (routes == null)
            {
                throw new ArgumentNullException("routes");
            }
            foreach (var route in routes)
            {
                if (route is LocalizedRoute)
                {
                    ((LocalizedRoute) route).ClearSeoFriendlyUrlsCachedValue();
                }
            }
        }
    }
}