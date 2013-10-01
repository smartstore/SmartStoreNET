using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Routing;
using System.IO;
using System.Diagnostics;
using System.Web.Mvc;

namespace SmartStore
{
	/// <remarks>codehint: sm-add</remarks>
    public static class HttpContextExtensions
	{

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

        public static RouteData GetRouteData(this HttpContextBase httpContext)
        {
            Guard.ArgumentNotNull(() => httpContext);

            var handler = httpContext.Handler as MvcHandler;
            if (handler != null && handler.RequestContext != null)
            {
                return handler.RequestContext.RouteData;
            }

            return null;
        }

        public static bool TryGetRouteData(this HttpContextBase httpContext, out RouteData routeData)
        {
            routeData = httpContext.GetRouteData();
            return routeData != null;
        }



	}
}
