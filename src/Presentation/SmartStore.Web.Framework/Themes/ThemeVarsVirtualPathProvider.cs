using System;
using System.Collections;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;
using System.Web.Optimization;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Infrastructure;

namespace SmartStore.Web.Framework.Themes
{
    public class ThemeVarsVirtualPathProvider : VirtualPathProvider
    {
		internal const string OverriddenStoreIdKey = "OverriddenStoreId";
		internal const string OverriddenThemeNameKey = "OverriddenThemeName";

		private readonly VirtualPathProvider _previous;

        public ThemeVarsVirtualPathProvider(VirtualPathProvider previous)
        {
            _previous = previous;
        }

        public override bool FileExists(string virtualPath)
        {
            return (ThemeHelper.PathIsThemeVars(virtualPath) || _previous.FileExists(virtualPath));
        }
         
        public override VirtualFile GetFile(string virtualPath)
        {
            if (ThemeHelper.PathIsThemeVars(virtualPath))
            {
				string themeName = ResolveCurrentThemeName();
				int storeId = ResolveCurrentStoreId();
                return new ThemeVarsVirtualFile(virtualPath, themeName, storeId);
            }

            return _previous.GetFile(virtualPath);
        }

		internal static int ResolveCurrentStoreId()
		{
			int storeId = 0;

			var httpContext = HttpContext.Current;
			if (httpContext != null && httpContext.Items != null)
			{
				if (httpContext.Items[OverriddenStoreIdKey] != null)
				{
					storeId = (int)httpContext.Items[OverriddenStoreIdKey];
					if (storeId > 0)
						return storeId;
				}
			}

			return EngineContext.Current.Resolve<IStoreContext>().CurrentStore.Id;
		}

		internal static string ResolveCurrentThemeName()
		{
			var themeName = string.Empty;

			var httpContext = HttpContext.Current;
			if (httpContext != null && httpContext.Items != null)
			{
				if (httpContext.Items[OverriddenThemeNameKey] != null)
				{
					themeName = (string)httpContext.Items[OverriddenThemeNameKey];
					if (themeName.HasValue())
						return themeName;
				}
			}

			return EngineContext.Current.Resolve<IThemeContext>().WorkingDesktopTheme;
		}
        
        public override CacheDependency GetCacheDependency(
            string virtualPath,
            IEnumerable virtualPathDependencies,
            DateTime utcStart)
        {

            bool isLess;
            if (IsStyleSheet(virtualPath, out isLess))
            {
                if (isLess)
                {
                    // the LESS HTTP handler made the call
                    // [...]
                }
                else
                {
                    // the Bundler made the call
                    var bundle = BundleTable.Bundles.GetBundleFor(virtualPath);
                    if (bundle == null)
                    {
                        return _previous.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
                    }
                }
                
                var arrPathDependencies = virtualPathDependencies.Cast<string>().ToArray();

                // determine the virtual themevars.less import reference
                var themeVarsFile = arrPathDependencies.Where(x => ThemeHelper.PathIsThemeVars(x)).FirstOrDefault();

                if (themeVarsFile.IsEmpty())
                {
                    // no themevars import... so no special considerations here
                    return _previous.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
                }

                // exclude the themevars import from the file dependencies list,
                // 'cause this one cannot be monitored by the physical file system
                var fileDependencies = arrPathDependencies.Except(new string[] { themeVarsFile });

                if (arrPathDependencies.Any())
                {
                    int storeId = ResolveCurrentStoreId();
                    string themeName = ResolveCurrentThemeName();
                    // invalidate the cache when variables change
                    string cacheKey = AspNetCache.BuildKey(FrameworkCacheConsumer.BuildThemeVarsCacheKey(themeName, storeId));
                    var cacheDependency = new CacheDependency(fileDependencies.Select(x => HostingEnvironment.MapPath(x)).ToArray(), new string[] { cacheKey }, utcStart);
                    return cacheDependency;
                }

                return null;
            }

            return _previous.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
        }

        private static bool IsStyleSheet(string virtualPath, out bool isLess)
        {
            bool isCss = false;
            isLess = virtualPath.EndsWith(".less", StringComparison.OrdinalIgnoreCase);
            if (!isLess)
                isCss = virtualPath.EndsWith(".css", StringComparison.OrdinalIgnoreCase);
            return isLess || isCss;
        }

    }
}