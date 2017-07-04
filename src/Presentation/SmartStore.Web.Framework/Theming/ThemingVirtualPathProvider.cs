using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;
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
			//return _previous.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
			return new CacheDependency(MapDependencyPaths(virtualPathDependencies.Cast<string>()), utcStart);
		}

		internal static string[] MapDependencyPaths(IEnumerable<string> virtualPathDependencies)
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

		private static InheritedThemeFileResult GetResolveResult(string virtualPath)
		{
			var result = EngineContext.Current.Resolve<IThemeFileResolver>().Resolve(virtualPath);
			return result;
		}

    }
}