using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Caching;
using System.Web.Hosting;
using SmartStore.Core.Caching;
using SmartStore.Core.Infrastructure;

namespace SmartStore.Web.Framework.Themes
{
    public class ThemingVirtualPathProvider : VirtualPathProvider
    {
		private readonly VirtualPathProvider _previous;

        public ThemingVirtualPathProvider(VirtualPathProvider previous)
        {
            _previous = previous;
        }

        public override bool FileExists(string virtualPath)
        {
			if (ThemeHelper.PathIsThemeVars(virtualPath))
			{
				return true;
			}

			var result = GetResolveResult(virtualPath);
			if (result != null)
			{
				if (!result.IsExplicit)
				{
					return true;
				}
				else
				{
					virtualPath = result.OriginalVirtualPath;
				}
			}

			return _previous.FileExists(virtualPath);
        }
         
        public override VirtualFile GetFile(string virtualPath)
        {
			if (ThemeHelper.PathIsThemeVars(virtualPath))
			{
				var theme = ThemeHelper.ResolveCurrentTheme();
				int storeId = ThemeHelper.ResolveCurrentStoreId();
				return new ThemeVarsVirtualFile(virtualPath, theme.ThemeName, storeId);
			}

			var result = GetResolveResult(virtualPath);
			if (result != null)
			{
				if (!result.IsExplicit)
				{
					return new InheritedVirtualThemeFile(result);
				}
				else
				{
					virtualPath = result.OriginalVirtualPath;
				}
			}

            return _previous.GetFile(virtualPath);
        }
        
        public override CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
        {
            bool isLess;
			bool isBundle;
			if (!ThemeHelper.IsStyleSheet(virtualPath, out isLess, out isBundle))
			{
				return GetCacheDependencyInternal(virtualPath, virtualPathDependencies, utcStart);
			}
            else
            {
                if (!isLess && !isBundle)
                {
					// it's a static css file (no bundle, no less)
					return GetCacheDependencyInternal(virtualPath, virtualPathDependencies, utcStart);
                }
                
                var arrPathDependencies = virtualPathDependencies.Cast<string>().ToArray();

                // determine the virtual themevars.less import reference
                var themeVarsFile = arrPathDependencies.Where(x => ThemeHelper.PathIsThemeVars(x)).FirstOrDefault();

                if (themeVarsFile.IsEmpty())
                {
                    // no themevars import... so no special considerations here
					return GetCacheDependencyInternal(virtualPath, virtualPathDependencies, utcStart);
                }

                // exclude the themevars import from the file dependencies list,
                // 'cause this one cannot be monitored by the physical file system
                var fileDependencies = arrPathDependencies.Except(new string[] { themeVarsFile });

                if (arrPathDependencies.Any())
                {
                    int storeId = ThemeHelper.ResolveCurrentStoreId();
                    var theme = ThemeHelper.ResolveCurrentTheme();
                    // invalidate the cache when variables change
                    string cacheKey = AspNetCache.BuildKey(FrameworkCacheConsumer.BuildThemeVarsCacheKey(theme.ThemeName, storeId));
					var cacheDependency = new CacheDependency(MapDependencyPaths(fileDependencies), new string[] { cacheKey }, utcStart);
                    return cacheDependency;
                }

                return null;
            }
        }

		private CacheDependency GetCacheDependencyInternal(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
		{
			return new CacheDependency(MapDependencyPaths(virtualPathDependencies.Cast<string>()), utcStart);
		}

		private string[] MapDependencyPaths(IEnumerable<string> virtualPathDependencies)
		{
			var fileNames = new List<string>();

			foreach (var dep in virtualPathDependencies)
			{
				var result = GetResolveResult(dep);
				if (result != null)
				{
					fileNames.Add(result.IsExplicit ? HostingEnvironment.MapPath(result.OriginalVirtualPath) : result.ResultPhysicalPath);
				}
				else
				{
					fileNames.Add(HostingEnvironment.MapPath(dep));
				}
			}

			return fileNames.ToArray();
		}

		private InheritedThemeFileResult GetResolveResult(string virtualPath)
		{
			var result = EngineContext.Current.Resolve<IThemeFileResolver>().Resolve(virtualPath);
			return result;
		}

    }
}