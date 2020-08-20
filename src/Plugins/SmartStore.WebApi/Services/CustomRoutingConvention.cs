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
    /// <see cref="https://docs.microsoft.com/en-us/odata/webapi/custom-routing-convention"/>
    /// </summary>
    public class CustomRoutingConvention : EntitySetRoutingConvention
    {
        private static readonly string[] _navigationPaths = new string[]
        {
            "~/entityset/key/navigation/key",
            "~/entityset/key/navigation"
        };

        private static readonly string[] _propertyPaths = new string[]
        {
            "~/entityset/key/property",
            "~/entityset/key/cast/property",
            "~/singleton/property",
            "~/singleton/cast/property"
        };

        public override string SelectAction(ODataPath odataPath, HttpControllerContext context, ILookup<string, HttpActionDescriptor> actionMap)
        {
            var path = odataPath?.PathTemplate;
            var method = context?.Request?.Method;

            if (path == null || method == null)
            {
                return null;
            }

            if (_navigationPaths.Contains(path))
            {
                // Standard OData path differ:
                // ~/entityset/key/$links/navigation (OData 3 "link"), ~/entityset/key/navigation/$ref (OData 4 "reference").

                if (method == HttpMethod.Get || method == HttpMethod.Post || method == HttpMethod.Delete)
                {
                    // Add keys to route data, so they will bind to action parameters.
                    if (GetNormalizedKey(odataPath, 1, out var key) && key != 0)
                    {
                        context.RouteData.Values[ODataRouteConstants.Key] = key;
                    }
                    else
                    {
                        throw context.Request.BadRequestException(WebApiGlobal.Error.NoKeyFromPath);
                    }

                    var navPropertyName = (odataPath.Segments[2] as NavigationPathSegment)?.NavigationPropertyName;
                    if (navPropertyName.IsEmpty())
                    {
                        throw context.Request.BadRequestException(WebApiGlobal.Error.NoNavigationFromPath);
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

                    var methodName = Inflector.Capitalize(method.ToString()) + navPropertyName;
                    if (actionMap.Contains(methodName))
                    {
                        return methodName;
                    }
                }
            }
            else if (_propertyPaths.Contains(path))
            {
                if (method == HttpMethod.Get)
                {
                    if (path.StartsWith("~/entityset/key"))
                    {
                        if (GetNormalizedKey(odataPath, 1, out var key) && key != 0)
                        {
                            context.RouteData.Values[ODataRouteConstants.Key] = key;
                        }
                        else
                        {
                            throw context.Request.BadRequestException(WebApiGlobal.Error.NoKeyFromPath);
                        }
                    }

                    var propertyName = (odataPath.Segments.Last() as PropertyAccessPathSegment)?.PropertyName;
                    if (propertyName.HasValue())
                    {
                        context.RouteData.Values["propertyName"] = propertyName;
                    }
                    else
                    {
                        throw context.Request.BadRequestException(WebApiGlobal.Error.PropertyNotFound.FormatInvariant(string.Empty));
                    }

                    var methodName = Inflector.Capitalize(method.ToString()) + "Property";
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

        #endregion
    }
}