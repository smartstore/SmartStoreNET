using System.Web;
using System.Web.Routing;
using System.IO;
using System.Web.Mvc;
using SmartStore.Services.Orders;
using SmartStore.Core.Infrastructure;
using SmartStore.Web.Framework.Themes;
using SmartStore.Core;
using System.Web.SessionState;

namespace SmartStore
{
    public static class HttpContextExtensions
	{
		internal const string OverriddenThemeNameKey = "OverriddenThemeName";
		internal const string OverriddenStoreIdKey = "OverriddenStoreId";

		public static bool IsAdminArea(this HttpRequest request)
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

		#region Theme & Store overrides (Request basis > Admin/LESS validation)

		public static void SetThemeOverride(this HttpRequest request, string theme)
		{
			SetThemeOverride(new HttpRequestWrapper(request), theme);
		}

		public static void SetStoreOverride(this HttpRequest request, int? storeId)
		{
			SetStoreOverride(new HttpRequestWrapper(request), storeId);
		}

		public static string GetThemeOverride(this HttpRequest request)
		{
			return GetThemeOverride(new HttpRequestWrapper(request));
		}

		public static int? GetStoreOverride(this HttpRequest request)
		{
			return GetStoreOverride(new HttpRequestWrapper(request));
		}

		public static void SetThemeOverride(this HttpRequestBase request, string theme)
		{
			try
			{
				var dataTokens = request.RequestContext.RouteData.DataTokens;
				if (theme.HasValue())
				{
					dataTokens[OverriddenThemeNameKey] = theme;
				}
				else if (dataTokens.ContainsKey(OverriddenThemeNameKey))
				{
					dataTokens.Remove(OverriddenThemeNameKey);
				}

				EngineContext.Current.Resolve<IThemeContext>().CurrentTheme = null;
			}
			catch { }
		}

		public static void SetStoreOverride(this HttpRequestBase request, int? storeId)
		{
			try
			{
				var dataTokens = request.RequestContext.RouteData.DataTokens;
				if (storeId.GetValueOrDefault() > 0)
				{
					dataTokens[OverriddenStoreIdKey] = storeId.Value;
				}
				else if (dataTokens.ContainsKey(OverriddenStoreIdKey))
				{
					dataTokens.Remove(OverriddenStoreIdKey);
				}

				EngineContext.Current.Resolve<IStoreContext>().CurrentStore = null;
			}
			catch { }
		}

		public static string GetThemeOverride(this HttpRequestBase request)
		{
			try
			{
				return (string)request.RequestContext.RouteData.DataTokens[OverriddenThemeNameKey];
			}
			catch
			{
				return null;
			}
		}

		public static int? GetStoreOverride(this HttpRequestBase request)
		{
			try
			{
				var value = request.RequestContext.RouteData.DataTokens[OverriddenStoreIdKey];
				if (value != null)
				{
					return (int)value;
				}

				return null;
			}
			catch
			{
				return null;
			}
		}

		#endregion


		#region Theme & Store overrides (Session basis > Preview mode)

		public static void SetThemeOverride(this HttpSessionState session, string theme)
		{
			SetThemeOverride(new HttpSessionStateWrapper(session), theme);
		}

		public static void SetStoreOverride(this HttpSessionState session, int? storeId)
		{
			SetStoreOverride(new HttpSessionStateWrapper(session), storeId);
		}

		public static string GetThemeOverride(this HttpSessionState session)
		{
			return GetThemeOverride(new HttpSessionStateWrapper(session));
		}

		public static int? GetStoreOverride(this HttpSessionState session)
		{
			return GetStoreOverride(new HttpSessionStateWrapper(session));
		}

		public static void SetThemeOverride(this HttpSessionStateBase session, string theme)
		{
			try
			{
				if (theme.HasValue())
				{
					session[OverriddenThemeNameKey] = theme;
				}
				else if (session[OverriddenThemeNameKey] != null)
				{
					session.Remove(OverriddenThemeNameKey);
				}

				EngineContext.Current.Resolve<IThemeContext>().CurrentTheme = null;
			}
			catch { }
		}

		public static void SetStoreOverride(this HttpSessionStateBase session, int? storeId)
		{
			try
			{
				if (storeId.GetValueOrDefault() > 0)
				{
					session[OverriddenStoreIdKey] = storeId.Value;
				}
				else if (session[OverriddenStoreIdKey] != null)
				{
					session.Remove(OverriddenStoreIdKey);
				}

				EngineContext.Current.Resolve<IThemeContext>().CurrentTheme = null;
			}
			catch { }
		}

		public static string GetThemeOverride(this HttpSessionStateBase session)
		{
			try
			{
				return (string)session[OverriddenThemeNameKey];
			}
			catch
			{
				return null;
			}
		}

		public static int? GetStoreOverride(this HttpSessionStateBase session)
		{
			try
			{
				var value = session[OverriddenStoreIdKey];
				if (value != null)
				{
					return (int)value;
				}

				return null;
			}
			catch
			{
				return null;
			}
		}

		#endregion
	}
}
