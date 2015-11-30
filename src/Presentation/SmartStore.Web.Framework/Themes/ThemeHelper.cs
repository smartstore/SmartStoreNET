using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Optimization;
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
		private static readonly Regex s_extensionlessPathPattern;

		public static readonly string ThemesBasePath;

		static ThemeHelper()
		{
			ThemesBasePath = CommonHelper.GetAppSetting<string>("sm:ThemesBasePath", "~/Themes/").EnsureEndsWith("/");

			var pattern = @"^{0}(.*)/(.+)(\.)(png|gif|jpg|jpeg|css|less|js|cshtml|svg|json)$".FormatInvariant(ThemesBasePath);
			s_inheritableThemeFilePattern = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
			s_themeVarsPattern = new Regex(@"^~/\.db/themevars.less$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
			s_extensionlessPathPattern = new Regex(@"/(.+)/([^/.]*)$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

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

		internal static bool IsAdminArea()
		{
			if (HttpContext.Current != null)
			{
				return HttpContext.Current.Request.IsAdminArea();
			}

			return false;
		}

		internal static bool IsStyleSheet(string path, out bool isLess, out bool isBundle)
		{
			bool isCss = false;
			isBundle = false;
			isLess = path.EndsWith(".less", StringComparison.OrdinalIgnoreCase);

			if (!isLess)
			{
				isCss = path.EndsWith(".css", StringComparison.OrdinalIgnoreCase);
				if (!isCss && s_extensionlessPathPattern.IsMatch(path))
				{
					// StyleBundles are  extension-less, so we have to ask 'BundleTable' 
					// if a style bundle has been registered for the given path.
					var bundle = BundleTable.Bundles.GetBundleFor(path);
					if (bundle != null)
					{
						isBundle = true;
						if (bundle is SmartStyleBundle || bundle is StyleBundle)
						{
							isCss = true;
						}
					}
				}
			}

			return isLess || isCss;
		}

		internal static ThemeManifest ResolveCurrentTheme()
		{
			return EngineContext.Current.Resolve<IThemeContext>().CurrentTheme;
		}

		internal static int ResolveCurrentStoreId()
		{
			return EngineContext.Current.Resolve<IStoreContext>().CurrentStore.Id;
		}

    }
}
