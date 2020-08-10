using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Conventions;
using SmartStore.Web.Framework.WebApi;

namespace SmartStore.WebApi.Services
{
    public class CustomRoutingConvention : EntitySetRoutingConvention
    {
        public override string SelectAction(ODataPath odataPath, HttpControllerContext context, ILookup<string, HttpActionDescriptor> actionMap)
        {
            var method = context.Request.Method;

            if (odataPath.PathTemplate.IsCaseInsensitiveEqual("~/entityset/key/navigation/key"))
            {
                if (method == HttpMethod.Get || method == HttpMethod.Post || method == HttpMethod.Delete)
                {
                    // We ignore standard OData path cause they differ:
                    // ~/entityset/key/$links/navigation (OData 3 "link"), ~/entityset/key/navigation/$ref (OData 4 "reference")

                    var navigationProperty = GetNavigation(odataPath, 2);

                    if (navigationProperty.IsEmpty())
                    {
                        throw context.Request.BadRequestException(WebApiGlobal.Error.NoNavigationFromPath);
                    }

                    var methodName = string.Concat("Navigation", navigationProperty);

                    $"process custom path {methodName}".Dump();
                }
            }

            // Not a match.
            return null;
        }

        #region Utilities

        //      public static bool GetNormalizedKey(this ODataPath odataPath, int segmentIndex, out int key)
        //{
        //	if (odataPath.Segments.Count > segmentIndex)
        //	{
        //		string rawKey = (odataPath.Segments[segmentIndex] as KeyValuePathSegment).Value;
        //		if (rawKey.HasValue())
        //		{
        //			if (rawKey.StartsWith("'"))
        //				rawKey = rawKey.Substring(1, rawKey.Length - 2);

        //			if (int.TryParse(rawKey, out key))
        //				return true;
        //		}
        //	}
        //	key = 0;
        //	return false;
        //}

        private static string GetNavigation(ODataPath odataPath, int segmentIndex)
        {
            if (odataPath.Segments.Count > segmentIndex)
            {
                var navigationProperty = (odataPath.Segments[segmentIndex] as NavigationPathSegment).NavigationPropertyName;
                return navigationProperty;
            }

            return null;
        }

        #endregion
    }
}