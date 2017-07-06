using System;
using System.Collections;
using System.Linq;
using System.Web.Caching;
using System.Web.Hosting;

namespace SmartStore.Web.Framework.Theming.Assets
{
    public sealed class BundlingVirtualPathProvider : ThemingVirtualPathProvider
    {
        public BundlingVirtualPathProvider(VirtualPathProvider previous)
			: base(previous)
        {
        }

        public override bool FileExists(string virtualPath)
        {
			var styleResult = ThemeHelper.IsStyleSheet(virtualPath);
			if (styleResult != null && (styleResult.IsThemeVars || styleResult.IsModuleImports))
			{
				return true;
			}

			return base.FileExists(virtualPath);
        }
         
        public override VirtualFile GetFile(string virtualPath)
        {
			var styleResult = ThemeHelper.IsStyleSheet(virtualPath);
			if (styleResult != null)
			{
				if (styleResult.IsThemeVars)
				{
					var theme = ThemeHelper.ResolveCurrentTheme();
					int storeId = ThemeHelper.ResolveCurrentStoreId();
					return new ThemeVarsVirtualFile(virtualPath, styleResult.Extension, theme.ThemeName, storeId);
				}
				else if (styleResult.IsModuleImports)
				{
					return new ModuleImportsVirtualFile(virtualPath, ThemeHelper.IsAdminArea());
				}
			}

			return base.GetFile(virtualPath);
        }
        
        public override CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
        {
			var styleResult = ThemeHelper.IsStyleSheet(virtualPath);
			if (styleResult == null)
			{
				return base.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
			}
            else
            {
                if (styleResult.IsCss)
                {
					// it's a static css file (no bundle, no sass/less)
					return base.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
                }
                
                var arrPathDependencies = virtualPathDependencies.Cast<string>().ToArray();

                // determine the virtual themevars.(scss|less) import reference
                var themeVarsFile = arrPathDependencies.Where(x => ThemeHelper.PathIsThemeVars(x)).FirstOrDefault();
				var moduleImportsFile = arrPathDependencies.Where(x => ThemeHelper.PathIsModuleImports(x)).FirstOrDefault();
				if (themeVarsFile.IsEmpty() && moduleImportsFile.IsEmpty() && !styleResult.IsBundle)
                {
                    // no themevars or moduleimports import... so no special considerations here
					return base.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
                }

				// exclude the special imports from the file dependencies list,
				// 'cause this one cannot be monitored by the physical file system
				var fileDependencies = arrPathDependencies
					.Except((new string[] { themeVarsFile, moduleImportsFile })
					.Where(x => x.HasValue()))
					.ToArray();

                if (arrPathDependencies.Any())
                {
                    int storeId = ThemeHelper.ResolveCurrentStoreId();
                    var theme = ThemeHelper.ResolveCurrentTheme();
                    // invalidate the cache when variables change
                    string cacheKey = FrameworkCacheConsumer.BuildThemeVarsCacheKey(theme.ThemeName, storeId);
					return new CacheDependency(ThemingVirtualPathProvider.MapDependencyPaths(fileDependencies), new string[] { cacheKey }, utcStart);
                }

				return null;
            }
        }
    }
}