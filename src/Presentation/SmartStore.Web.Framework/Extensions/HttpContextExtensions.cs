using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Services.Orders;
using SmartStore.Utilities;

namespace SmartStore
{
    public static class HttpContextExtensions
	{

		public static bool IsAdminArea(this HttpRequest request)
		{
			if (request != null)
			{
				return IsAdminArea(new HttpRequestWrapper(request));
			}

			return false;		
		}

		public static bool IsAdminArea(this HttpRequestBase request)
		{
			try
			{
				if (request != null)
				{
					var area = request.RequestContext.RouteData.GetAreaName();
					if (area != null)
					{
						return area.IsCaseInsensitiveEqual("admin");
					}
				}

				return false;
			}
			catch
			{
				return false;
			}
		}

		public static bool IsPublicArea(this HttpRequest request)
		{
			if (request != null)
			{
				return IsPublicArea(new HttpRequestWrapper(request));
			}

			return false;
		}

		public static bool IsPublicArea(this HttpRequestBase request)
		{
			try
			{
				if (request != null)
				{
					var area = request.RequestContext.RouteData.GetAreaName();
					return area.IsEmpty();
				}

				return false;
			}
			catch
			{
				return false;
			}
		}

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

			if (contentType.IsEmpty())
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

		internal static string GetUserThemeChoiceFromCookie(this HttpContextBase context)
		{
			if (context == null)
				return null;

			var cookie = context.Request.Cookies.Get("sm.UserThemeChoice");
			if (cookie != null)
			{
				return cookie.Value.NullEmpty();
			}

			return null;
		}

		internal static void SetUserThemeChoiceInCookie(this HttpContextBase context, string value)
		{
			if (context == null)
				return;

			var cookie = context.Request.Cookies.Get("sm.UserThemeChoice");

			if (value.HasValue() && cookie == null)
			{
				cookie = new HttpCookie("sm.UserThemeChoice");
				cookie.HttpOnly = true;
				cookie.Expires = DateTime.UtcNow.AddYears(1);					
			}

			if (value.HasValue())
			{
				cookie.Value = value;
				context.Request.Cookies.Set(cookie);
			}

			if (value.IsEmpty() && cookie != null)
			{
				cookie.Expires = DateTime.UtcNow.AddYears(-10);
			}

			if (cookie != null)
			{
				context.Response.SetCookie(cookie);
			}
		}

		internal static HttpCookie GetPreviewModeCookie(this HttpContextBase context, bool createIfMissing)
		{
			if (context == null)
				return null;

			var cookie = context.Request.Cookies.Get("sm.PreviewModeOverrides");
			
			if (cookie == null && createIfMissing)
			{
				cookie = new HttpCookie("sm.PreviewModeOverrides");
				cookie.HttpOnly = true;
				context.Request.Cookies.Set(cookie);
			}

			if (cookie != null)
			{
				// when cookie gets created or touched, extend its lifetime
				cookie.Expires = DateTime.UtcNow.AddMinutes(20);
			}

			return cookie;
		}

		internal static void SetPreviewModeValue(this HttpContextBase context, string key, string value)
		{
			if (context == null)
				return;

			var cookie = context.GetPreviewModeCookie(value.HasValue());
			if (cookie != null)
			{
				if (value.HasValue())
				{
					cookie.Values[key] = value;
				}
				else
				{
					cookie.Values.Remove(key);
				}
			}
		}

		public static IDisposable PreviewModeCookie(this HttpContextBase context)
		{
			var disposable = new ActionDisposable(() => {
				var cookie = GetPreviewModeCookie(context, false);
				if (cookie != null)
				{
					if (!cookie.HasKeys)
					{
						cookie.Expires = DateTime.UtcNow.AddYears(-10);
					}
					else
					{
						cookie.Expires = DateTime.UtcNow.AddMinutes(20);
					}

					context.Response.SetCookie(cookie);
				}
			});

			return disposable;
		}

	}
}
