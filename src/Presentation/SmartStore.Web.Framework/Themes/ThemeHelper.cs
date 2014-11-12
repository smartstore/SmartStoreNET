using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;
using SmartStore.Core;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Themes;
using SmartStore.Utilities;

namespace SmartStore.Web.Framework.Themes
{
    internal static class ThemeHelper
    {
		private static readonly Regex s_inheritableThemeFilePattern;
		private static readonly Regex s_themeVarsPattern;

		internal const string OverriddenStoreIdKey = "OverriddenStoreId";
		internal const string OverriddenThemeNameKey = "OverriddenThemeName";

		public static readonly string ThemesBasePath;

		static ThemeHelper()
		{
			ThemesBasePath = CommonHelper.GetAppSetting<string>("sm:ThemesBasePath", "~/Themes/").EnsureEndsWith("/");

			var pattern = @"^{0}(.*)/(.*)(\.png|gif|jpg|jpeg|css|less|js|cshtml|svg|json)$".FormatInvariant(ThemesBasePath);
			s_inheritableThemeFilePattern = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
			s_themeVarsPattern = new Regex(@"^~/\.db/themevars.less$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
		}

        internal static bool PathListContainsThemeVars(IEnumerable<string> pathes)
        {
            Guard.ArgumentNotNull(() => pathes);

            return pathes.Any(x => PathIsThemeVars(x));
        }

        internal static bool PathIsThemeVars(string virtualPath)
        {
            if (string.IsNullOrEmpty(virtualPath))
                return false;

			if (virtualPath[0] != '~')
			{
				virtualPath = VirtualPathUtility.ToAppRelative(virtualPath);
			}

			return s_themeVarsPattern.IsMatch(virtualPath);
        }

		internal static bool PathIsInheritableThemeFile(string virtualPath)
		{
			if (string.IsNullOrEmpty(virtualPath))
				return false;

			if (virtualPath[0] != '~')
			{
				virtualPath = VirtualPathUtility.ToAppRelative(virtualPath);
			}

			return s_inheritableThemeFilePattern.IsMatch(virtualPath);
		}

		internal static ThemeManifest ResolveCurrentTheme(RouteData routeData = null, bool tryGetOverriddenTheme = false)
		{
			if (tryGetOverriddenTheme)
			{
				var themeName = string.Empty;

				if (routeData != null)
				{
					object themeOverride;
					if (routeData.DataTokens != null && routeData.DataTokens.TryGetValue("ThemeOverride", out themeOverride))
					{
						themeName = themeOverride as string;
					}
				}

				if (themeName.IsEmpty())
				{
					var httpContext = HttpContext.Current;
					if (httpContext != null && httpContext.Items != null)
					{
						if (httpContext.Items.Contains(OverriddenThemeNameKey))
						{
							themeName = (string)httpContext.Items[OverriddenThemeNameKey];
						}
					}
				}

				if (themeName.HasValue())
				{
					var manifest = EngineContext.Current.Resolve<IThemeRegistry>().GetThemeManifest(themeName);
					if (manifest != null)
					{
						return manifest;
					}
				}
			}

			return EngineContext.Current.Resolve<IThemeContext>().CurrentTheme;
		}

		internal static int ResolveCurrentStoreId(bool tryGetOverriddenStoreId = false)
		{
			if (tryGetOverriddenStoreId)
			{
				int storeId = 0;

				var httpContext = HttpContext.Current;
				if (httpContext != null && httpContext.Items != null)
				{
					if (httpContext.Items.Contains(OverriddenStoreIdKey))
					{
						storeId = (int)httpContext.Items[OverriddenStoreIdKey];
						if (storeId > 0)
							return storeId;
					}
				}
			}

			return EngineContext.Current.Resolve<IStoreContext>().CurrentStore.Id;
		}

    }
}
