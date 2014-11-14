using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Routing;

// use base SmartStore Namespace to ensure the extension methods are always available
namespace SmartStore
{
	
	public static class ThemeHttpExtensions
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


		public static void SetThemeOverride(this HttpRequest request, string theme)
		{
			if (request != null && theme.HasValue())
				request.RequestContext.RouteData.DataTokens[OverriddenThemeNameKey] = theme;
		}

		public static void SetThemeOverride(this HttpRequestBase request, string theme)
		{
			try
			{
				if (request != null && theme.HasValue())
					request.RequestContext.RouteData.DataTokens[OverriddenThemeNameKey] = theme;
			}
			catch { }
		}

		public static void SetStoreOverride(this HttpRequest request, int storeId)
		{
			if (request != null && storeId > 0)
				request.RequestContext.RouteData.DataTokens[OverriddenStoreIdKey] = storeId;
		}

		public static void SetStoreOverride(this HttpRequestBase request, int storeId)
		{
			try
			{
				if (request != null && storeId > 0)
					request.RequestContext.RouteData.DataTokens[OverriddenStoreIdKey] = storeId;
			}
			catch { }
		}


		public static string GetThemeOverride(this HttpRequest request)
		{
			if (request != null)
				return (string)request.RequestContext.RouteData.DataTokens[OverriddenThemeNameKey];

			return null;
		}

		public static string GetThemeOverride(this HttpRequestBase request)
		{
			try
			{
				if (request != null)
					return (string)request.RequestContext.RouteData.DataTokens[OverriddenThemeNameKey];

				return null;
			}
			catch
			{
				return null;
			}
		}

		public static int? GetStoreOverride(this HttpRequest request)
		{
			if (request != null)
			{
				if (request.RequestContext.RouteData.DataTokens.ContainsKey(OverriddenStoreIdKey))
				{
					return (int)request.RequestContext.RouteData.DataTokens[OverriddenStoreIdKey];
				}
			}

			return null;
		}

		public static int? GetStoreOverride(this HttpRequestBase request)
		{
			try
			{
				if (request != null)
				{
					if (request.RequestContext.RouteData.DataTokens.ContainsKey(OverriddenStoreIdKey))
					{
						return (int)request.RequestContext.RouteData.DataTokens[OverriddenStoreIdKey];
					}
				}

				return null;
			}
			catch 
			{
				return null;
			}
		}

	}

}
