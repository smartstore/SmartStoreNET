using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Routing;
using System.IO;
using System.Diagnostics;

namespace SmartStore
{
	/// <remarks>codehint: sm-add</remarks>
    public static class HttpContextExtensions
	{
        private const string ROUTEDATA_CACHEKEY = "__Current_RouteData__";

        public static Stream ToFileStream(this HttpRequestBase request, out string fileName, out string contentType, string paramName = "qqfile") {
			fileName = contentType = "";
			Stream stream = null;

            if (request[paramName].HasValue())
            {
                stream = request.InputStream;
                fileName = request[paramName];
            }
            else
            {
                if (request.Files.Count > 0)
                {
                    stream = request.Files[0].InputStream;
                    contentType = request.Files[0].ContentType;
                    fileName = Path.GetFileName(request.Files[0].FileName);
                }
            }

            if (contentType.IsNullOrEmpty())
            {
                contentType = SmartStore.Core.IO.MimeTypes.MapNameToMimeType(fileName);
            }

			return stream;
		}

        public static void SetRouteData(this HttpContextBase httpContext, RouteData data)
        {
            Guard.ArgumentNotNull(() => httpContext);
            Guard.ArgumentNotNull(() => data);

            httpContext.Items[ROUTEDATA_CACHEKEY] = data;
        }

        public static RouteData GetRouteData(this HttpContextBase httpContext)
        {
            Guard.ArgumentNotNull(() => httpContext);

            if (httpContext.Items.Contains(ROUTEDATA_CACHEKEY))
            {
                return httpContext.Items[ROUTEDATA_CACHEKEY] as RouteData; 
            }

            return null;
        }

        public static bool TryGetRouteData(this HttpContextBase httpContext, out RouteData routeData)
        {
            Guard.ArgumentNotNull(() => httpContext);

            routeData = null;

            if (httpContext.Items.Contains(ROUTEDATA_CACHEKEY))
            {
                routeData = httpContext.Items[ROUTEDATA_CACHEKEY] as RouteData;
                return true;
            }

            return false;
        }



	}
}
