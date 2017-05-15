using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;
using SmartStore.Core.Caching;
using SmartStore.Core.Infrastructure;
using SmartStore.Utilities;
using SmartStore.Web.Framework.Plugins;

namespace SmartStore.Web.Framework.Theming
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
			var styleResult = ThemeHelper.IsStyleSheet(virtualPath);
			if (styleResult != null && (styleResult.IsThemeVars || styleResult.IsModuleImports))
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
			var styleResult = ThemeHelper.IsStyleSheet(virtualPath);
			if (styleResult == null)
			{
				return GetCacheDependencyInternal(virtualPath, virtualPathDependencies, utcStart);
			}
            else
            {
                if (styleResult.IsCss)
                {
					// it's a static css file (no bundle, no sass/less)
					return GetCacheDependencyInternal(virtualPath, virtualPathDependencies, utcStart);
                }
                
                var arrPathDependencies = virtualPathDependencies.Cast<string>().ToArray();

                // determine the virtual themevars.(scss|less) import reference
                var themeVarsFile = arrPathDependencies.Where(x => ThemeHelper.PathIsThemeVars(x)).FirstOrDefault();
				var moduleImportsFile = arrPathDependencies.Where(x => ThemeHelper.PathIsModuleImports(x)).FirstOrDefault();
				if (themeVarsFile.IsEmpty() && moduleImportsFile.IsEmpty())
                {
                    // no themevars or moduleimports import... so no special considerations here
					return GetCacheDependencyInternal(virtualPath, virtualPathDependencies, utcStart);
                }

				// exclude the special imports from the file dependencies list,
				// 'cause this one cannot be monitored by the physical file system
				var fileDependencies = arrPathDependencies.Except((new string[] { themeVarsFile, moduleImportsFile }).Where(x => x.HasValue()));

                if (arrPathDependencies.Any())
                {
                    int storeId = ThemeHelper.ResolveCurrentStoreId();
                    var theme = ThemeHelper.ResolveCurrentTheme();
                    // invalidate the cache when variables change
                    string cacheKey = FrameworkCacheConsumer.BuildThemeVarsCacheKey(theme.ThemeName, storeId);
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
					string mappedPath = null;
					if (CommonHelper.IsDevEnvironment && HttpContext.Current.IsDebuggingEnabled)
					{
						// We're in debug mode and in dev environment: try to map path with VPP
						var file = HostingEnvironment.VirtualPathProvider.GetFile(dep) as DebugPluginVirtualFile;
						if (file != null)
						{
							mappedPath = file.PhysicalPath;
						}
					}

					fileNames.Add(mappedPath ?? HostingEnvironment.MapPath(dep));
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