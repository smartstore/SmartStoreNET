using System.Web;
using System.Web.Routing;
using System.IO;
using System.Web.Mvc;
using SmartStore.Services.Orders;

namespace SmartStore
{
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

		public static CheckoutState GetCheckoutState(this HttpContextBase httpContext)
		{
			Guard.ArgumentNotNull(() => httpContext);
			
			var state = httpContext.Session.SafeGetValue<CheckoutState>(CheckoutState.CheckoutStateSessionKey);

			if (state != null)
				return state;

			state = new CheckoutState();
			httpContext.Session.SafeSet(CheckoutState.CheckoutStateSessionKey, state);

			return state;
		}

		public static void RemoveCheckoutState(this HttpContextBase httpContext)
		{
			Guard.ArgumentNotNull(() => httpContext);

			httpContext.Session.SafeRemove(CheckoutState.CheckoutStateSessionKey);
		}
	}
}
