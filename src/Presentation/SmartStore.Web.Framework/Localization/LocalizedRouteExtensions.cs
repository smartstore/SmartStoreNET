using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Seo;

namespace SmartStore.Web.Framework.Localization
{

    public static class LocalizedRouteExtensions
    {

        #region CultureCode extensions

        /// <summary>
        /// Gets a value indicating whether a route url is explicitly preceded by a culture code
        /// </summary>
        /// <returns><c>true</c> when the url is explicitly preceded by a culture code</returns>
        public static bool IsCultureCodeSpecified(this RouteData routeData)
        {
            Guard.ArgumentNotNull(() => routeData);

            return routeData.Values.ContainsKey(LocalizedRoute.CULTURECODE_SPECIFIED_TOKEN);
        }

        /// <summary>
        /// Gets a value indicating whether a route url is explicitly preceded by a culture code
        /// </summary>
        /// <param name="specifiedOrDefaultCultureCode">
        /// Either the explicitly specified culture code OR the default value for this token
        /// </param>
        /// <returns><c>true</c> when the url is explicitly preceded by a culture code</returns>
        public static bool IsCultureCodeSpecified(this RouteData routeData, out string specifiedOrDefaultCultureCode)
        {
            specifiedOrDefaultCultureCode = null;

            bool specified = routeData.IsCultureCodeSpecified();

            var routeValues = routeData.Values;
            if (routeValues.ContainsKey(LocalizedRoute.CULTURECODE_TOKEN))
            {
                specifiedOrDefaultCultureCode = routeValues[LocalizedRoute.CULTURECODE_TOKEN] as string;
            }

            return specified;
        }

        internal static void SetCultureCodeSpecified(this RouteData routeData, bool specified)
        {
            Guard.ArgumentNotNull(() => routeData);

            var tokenName = LocalizedRoute.CULTURECODE_SPECIFIED_TOKEN;

            if (!specified)
            {
                routeData.Values.Remove(tokenName);
            }
            else
            {
                if (!routeData.DataTokens.ContainsKey(tokenName))
                {
                    routeData.Values.Add(tokenName, true);
                }
            }
        }

        internal static bool IsPathPrecededByCultureCode(this string url, string cultureCode) 
        {
            Guard.ArgumentNotNull(() => url);
            Guard.ArgumentNotEmpty(() => cultureCode);

            url = url.TrimStart(new char[] {'/', '~'});

            if (url.Length < cultureCode.Length)
            {
                return false;
            }

            bool match = url.StartsWith(cultureCode, StringComparison.OrdinalIgnoreCase);

            return match;
        }

        ///// <summary>
        ///// Tries to get an explicitly specified culture code
        ///// </summary>
        ///// <param name="routeValues"></param>
        ///// <param name="requestedCultureCode">The culture code, when it's explicitly specified in the requested url, otherwise <c>null</c></param>
        ///// <returns><c>true</c> if the culture code was explicitly set in the url</returns>
        //public static bool TryGetCultureCode(this RouteData routeData, out string requestedCultureCode)
        //{
        //    Guard.ArgumentNotNull(() => routeData);

        //    var routeValues = routeData.Values;
            
        //    requestedCultureCode = null;
        //    if (routeValues.ContainsKey(LocalizedRoute.CULTURECODE_TOKEN))
        //    {
        //        string value = routeValues[LocalizedRoute.CULTURECODE_TOKEN] as string;
        //        if (value.HasValue() && value != "default")
        //        {
        //            requestedCultureCode = value;
        //            return true;
        //        }
        //    }
            
        //    return false;
        //}

        public static string GetCultureCode(this RouteValueDictionary routeValues)
        {
            Guard.ArgumentNotEmpty(() => routeValues);

            if (routeValues.ContainsKey(LocalizedRoute.CULTURECODE_TOKEN)) 
            {
                return routeValues[LocalizedRoute.CULTURECODE_TOKEN] as string;
            }

            return null;
        }

        public static void SetCultureCode(this RouteValueDictionary routeValues, string cultureCode)
        {
            Guard.ArgumentNotEmpty(() => cultureCode);

            routeValues[LocalizedRoute.CULTURECODE_TOKEN] = cultureCode;
        }

        #endregion

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

            var tokenName = LocalizedRoute.CULTURECODE_TOKEN;

            string urlToken = "{" + tokenName + "}";

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
                if (route.Constraints != null && route.Constraints.ContainsKey(tokenName))
                {
                    route.Constraints.Remove(tokenName);
                }

                // remove default cultureCode from DEFAULTS
                if (route.Defaults != null && route.Defaults.ContainsKey(tokenName))
                {
                    route.Defaults.Remove(tokenName);
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

            string defaultCultureCode = allowedCultureCodes.First();
            var constraint = new CultureCodeConstraint(allowedCultureCodes.ToArray());

            //var routes2 = routes.ToArray();
            //for (int i = 0; i < routes2.Length; i++)
            //{
            //    var route = routes2[i] as LocalizedRoute;

            //    if (route == null)
            //    {
            //        continue;
            //    }

            //    var clonedRoute = route.Clone();

            //    ApplyThis(route);
            //    ApplyThis(clonedRoute, defaultCultureCode, constraint);

            //    routes.Insert(routes.IndexOf(route), clonedRoute);
            //}

            foreach (var route in routes.OfType<LocalizedRoute>())
            {
                ApplyThis(route, null, null);
            }
        }

        private static void ApplyThis(LocalizedRoute route, string defaultCultureCode = null, CultureCodeConstraint constraint = null)
        {
            var tokenName = LocalizedRoute.CULTURECODE_TOKEN;

            if (constraint != null)
            {
                // add CultureCodeConstraint
                if (route.Constraints == null)
                {
                    route.Constraints = new RouteValueDictionary();
                }
                if (route.Constraints.ContainsKey(tokenName))
                {
                    route.Constraints.Remove(tokenName);
                }
                route.Constraints.Add(tokenName, constraint);
            }
            
            if (defaultCultureCode.HasValue())
            {
                string urlToken = "{" + tokenName + "}";

                // add culture token to url pattern
                var originalUrl = route.Url;
                var url = route.Url;
                if (!url.StartsWith(urlToken))
                {
                    url = "{0}/{1}".FormatInvariant(urlToken, url).TrimEnd('/');
                    route.Url = url;
                }
                
                // add default cultureCode to DEFAULTS
                if (route.Defaults == null)
                {
                    route.Defaults = new RouteValueDictionary();
                }
                if (route.Defaults.ContainsKey(tokenName))
                {
                    route.Defaults.Remove(tokenName);
                }
                route.Defaults.Add(tokenName, defaultCultureCode);
            }

            // set specialized LocalizedRouteHandler
            if (route.RouteHandler.GetType() != typeof(LocalizedRouteHandler))
            {
                route.RouteHandler = new LocalizedRouteHandler();
            }

            route.SeoFriendlyUrlsEnabled = true;
            route.DefaultCultureCode = defaultCultureCode;
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

        #endregion
    }
}