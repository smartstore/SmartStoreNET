using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Controllers;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Routing;

namespace SmartStore.Web.Infrastructure
{
    public partial class GeneralRoutes : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapLocalizedRoute(
                "Default_Localized",
                "{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                new { controller = new IsKnownController() },
                new[] { "SmartStore.Web.Controllers" }
            );

            routes.MapRoute(
                "Default",
                "{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                new { controller = new IsKnownController() },
                new[] { "SmartStore.Web.Controllers" }
            );
        }

        public int Priority { get; } = -999;
    }

    internal class IsKnownController : IRouteConstraint
    {
        private readonly static HashSet<string> s_knownControllers = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

        static IsKnownController()
        {
            var assembly = typeof(HomeController).Assembly;
            var controllerTypes = from t in assembly.GetExportedTypes()
                                  where typeof(IController).IsAssignableFrom(t) && t.Namespace == "SmartStore.Web.Controllers"
                                  select t;

            foreach (var type in controllerTypes)
            {
                var name = type.Name.Substring(0, type.Name.Length - 10);
                s_knownControllers.Add(name);
            }
        }

        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            if (values.TryGetValue(parameterName, out var value))
            {
                var requestedController = Convert.ToString(value);
                if (s_knownControllers.Contains(requestedController))
                {
                    if (requestedController.IsCaseInsensitiveEqual("download"))
                    {
                        // Special case for '~/download'. We have a known controller called "Download", which unfortunately blocks
                        // the usage of the url '~/download' (without action). To be able to use '/download' as a SEO slug,
                        // we check here whether the requested route has an action name other than the default (empty or 'Index').
                        var action = Convert.ToString(values["action"]);
                        if (action.IsEmpty() || string.Equals(action, Convert.ToString(route.Defaults["action"]), StringComparison.OrdinalIgnoreCase))
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }

            return false;
        }
    }
}
