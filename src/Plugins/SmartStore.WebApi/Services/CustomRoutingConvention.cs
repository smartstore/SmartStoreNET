using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Conventions;
using SmartStore.Utilities;
using SmartStore.Web.Framework.WebApi;

namespace SmartStore.WebApi.Services
{
    /// <summary>
    /// Used to serve URL paths that are ignored by OData by default.
    /// <see cref="https://docs.microsoft.com/en-us/aspnet/web-api/overview/odata-support-in-aspnet-web-api/odata-routing-conventions"/>
    /// </summary>
    public class CustomRoutingConvention : EntitySetRoutingConvention
    {
        public override string SelectAction(ODataPath odataPath, HttpControllerContext context, ILookup<string, HttpActionDescriptor> actionMap)
        {
            var method = context.Request.Method;
            var path = odataPath.PathTemplate;

            if (path.IsCaseInsensitiveEqual("~/entityset/key/navigation/key") ||
                path.IsCaseInsensitiveEqual("~/entityset/key/navigation"))
            {
                // We ignore standard OData path cause they differ:
                // ~/entityset/key/$links/navigation (OData 3 "link"), ~/entityset/key/navigation/$ref (OData 4 "reference").

                if (method == HttpMethod.Get || method == HttpMethod.Post || method == HttpMethod.Delete)
                {
                    // Add keys to route data, so they will bind to action parameters.
                    var navigationProperty = GetNavigationName(odataPath, 2);

                    if (navigationProperty.IsEmpty())
                    {
                        throw context.Request.BadRequestException(WebApiGlobal.Error.NoNavigationFromPath);
                    }

                    if (GetNormalizedKey(odataPath, 1, out var key) && key != 0)
                    {
                        context.RouteData.Values[ODataRouteConstants.Key] = key;
                    }
                    else
                    {
                        throw context.Request.BadRequestException(WebApiGlobal.Error.NoKeyFromPath);
                    }

                    // Allow relatedKey = 0 to remove all assignments.
                    if (GetNormalizedKey(odataPath, 3, out var relatedKey))
                    {
                        context.RouteData.Values[ODataRouteConstants.RelatedKey] = relatedKey;
                    }
                    else if (method == HttpMethod.Post)
                    {
                        // relatedKey is mandatory.
                        throw context.Request.BadRequestException(WebApiGlobal.Error.NoRelatedKeyFromPath);
                    }

                    var methodName = Inflector.Capitalize(method.ToString()) + navigationProperty;
                    if (actionMap.Contains(methodName))
                    {
                        return methodName;
                    }

                    //$"process custom path {methodName} {actionMap.Contains(methodName)}".Dump();
                }
            }
            else if (path.IsCaseInsensitiveEqual("~/entityset/key/property"))
            {
                if (method == HttpMethod.Get)
                {
                    if (GetNormalizedKey(odataPath, 1, out var key) && key != 0)
                    {
                        context.RouteData.Values[ODataRouteConstants.Key] = key;
                    }
                    else
                    {
                        throw context.Request.BadRequestException(WebApiGlobal.Error.NoKeyFromPath);
                    }

                    var propertyName = GetPropertyName(odataPath, 2);

                    if (propertyName.IsEmpty())
                    {
                        throw context.Request.BadRequestException(WebApiGlobal.Error.PropertyNotFound.FormatInvariant(string.Empty));
                    }

                    //GetProperty
                    var methodName = Inflector.Capitalize(method.ToString()) + propertyName;
                    if (actionMap.Contains(methodName))
                    {
                        return methodName;
                    }
                }
            }

            // Not a match. We do not support requested path.
            return null;
        }

        #region Utilities

        private static bool GetNormalizedKey(ODataPath odataPath, int segmentIndex, out int key)
        {
            if (odataPath.Segments.Count > segmentIndex)
            {
                var rawKey = (odataPath.Segments[segmentIndex] as KeyValuePathSegment).Value;
                if (rawKey.HasValue())
                {
                    if (rawKey.StartsWith("'"))
                    {
                        rawKey = rawKey.Substring(1, rawKey.Length - 2);
                    }

                    if (int.TryParse(rawKey, out key))
                    {
                        return true;
                    }
                }
            }

            key = 0;
            return false;
        }

        private static string GetNavigationName(ODataPath odataPath, int segmentIndex)
        {
            if (odataPath.Segments.Count > segmentIndex)
            {
                var navigationProperty = (odataPath.Segments[segmentIndex] as NavigationPathSegment).NavigationPropertyName;
                return navigationProperty;
            }

            return null;
        }

        private static string GetPropertyName(ODataPath odataPath, int segmentIndex)
        {
            if (odataPath.Segments.Count > segmentIndex)
            {
                var propertyName = (odataPath.Segments[segmentIndex] as PropertyAccessPathSegment).PropertyName;
                return propertyName;
            }

            return null;
        }

        #endregion
    }
}