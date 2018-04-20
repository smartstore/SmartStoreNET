using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Services.Orders;
using SmartStore.Utilities;

namespace SmartStore
{
	public static class HttpExtensions
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

		public static PostedFileResult ToPostedFileResult(this HttpRequestBase httpRequest)
		{
			if (httpRequest != null && httpRequest.Files.Count > 0)
			{
				return httpRequest.Files[0].ToPostedFileResult();
			}

			return null;
		}

		public static PostedFileResult ToPostedFileResult(this HttpPostedFile httpFile)
		{
			if (httpFile != null && httpFile.ContentLength > 0)
			{
				return new PostedFileResult(new HttpPostedFileWrapper(httpFile));
			}

			return null;
		}

		public static PostedFileResult ToPostedFileResult(this HttpPostedFileBase httpFile)
		{
			if (httpFile != null && httpFile.ContentLength > 0)
			{
				return new PostedFileResult(httpFile);
			}

			return null;
		}

		public static IEnumerable<PostedFileResult> ToPostedFileResults(this HttpFileCollection httpFileCollection)
		{
			if (httpFileCollection != null && httpFileCollection.Count > 0)
			{
				return new HttpFileCollectionWrapper(httpFileCollection).ToPostedFileResults();
			}

			return Enumerable.Empty<PostedFileResult>();
		}

		public static IEnumerable<PostedFileResult> ToPostedFileResults(this HttpFileCollectionBase httpFileCollection)
		{
			if (httpFileCollection == null)
				yield break;

			var batchId = Guid.NewGuid();

			for (var i = 0; i < httpFileCollection.Count; i++) 
			{
				var httpFile = httpFileCollection[i];
				var result = httpFile.ToPostedFileResult();
				if (result != null)
				{
					result.BatchId = batchId;
					yield return result;
				}
			}
		}

        public static RouteData GetRouteData(this HttpContextBase httpContext)
        {
            Guard.NotNull(httpContext, nameof(httpContext));

            if (httpContext.Handler is MvcHandler handler && handler.RequestContext != null)
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
			Guard.NotNull(httpContext, nameof(httpContext));

			var state = httpContext.Session.SafeGetValue<CheckoutState>(CheckoutState.CheckoutStateSessionKey);

			if (state != null)
				return state;

			state = new CheckoutState();
			httpContext.Session.SafeSet(CheckoutState.CheckoutStateSessionKey, state);

			return state;
		}

		public static void RemoveCheckoutState(this HttpContextBase httpContext)
		{
			Guard.NotNull(httpContext, nameof(httpContext));

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
				cookie = new HttpCookie("sm.UserThemeChoice")
				{
					HttpOnly = true,
					Expires = DateTime.UtcNow.AddYears(1)
				};				
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
				cookie = new HttpCookie("sm.PreviewModeOverrides") { HttpOnly = true };
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
			var disposable = new ActionDisposable(() =>
			{
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

		public static string GetContentUrl(this HttpContextBase context, string path)
		{
			if (path.HasValue())
			{
				if (!path.StartsWith("~"))
				{
					if (!path.StartsWith("/"))
						path = "/" + path;
					path = "~" + path;
				}

				return UrlHelper.GenerateContentUrl(path, context);
			}

			return path;
		}
	}
}
