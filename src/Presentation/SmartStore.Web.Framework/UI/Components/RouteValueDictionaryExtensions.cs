using System;
using System.Collections.Generic;
using System.Web.Routing;

namespace SmartStore.Web.Framework.UI
{    
    public static class RouteValueDictionaryExtensions
    {
        public static void ApplyTo(this RouteValueDictionary routeValues, INavigatable instance, Action<INavigatable, string, string, RouteValueDictionary> callBack)
        {
            object actionName;
            object controllerName;
            RouteValueDictionary values = new RouteValueDictionary();
            GetActionParams(routeValues, out actionName, out controllerName, values);
            callBack(instance, (string)actionName, (string)controllerName, values);
        }

        public static TBuilder ApplyTo<TBuilder>(this RouteValueDictionary routeValues, Func<string, string, RouteValueDictionary, TBuilder> callBack)
        {
            object actionName;
            object controllerName;
            RouteValueDictionary values = new RouteValueDictionary();
            GetActionParams(routeValues, out actionName, out controllerName, values);
            return callBack((string)actionName, (string)controllerName, values);
        }

        private static void GetActionParams(RouteValueDictionary routeValues, out object actionName, out object controllerName, RouteValueDictionary values)
        {
            routeValues.TryGetValue("action", out actionName);
            routeValues.TryGetValue("controller", out controllerName);
            routeValues.Remove("action");
            routeValues.Remove("controller");
            values.Merge((IDictionary<string, object>)routeValues);
        }
    }
}
