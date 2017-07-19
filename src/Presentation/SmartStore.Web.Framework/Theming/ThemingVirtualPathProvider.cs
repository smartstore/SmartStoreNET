using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Themes;
using SmartStore.Utilities;
using SmartStore.Web.Framework.Plugins;

namespace SmartStore.Web.Framework.Theming
{
    public class ThemingVirtualPathProvider : SmartVirtualPathProvider
    {
		private readonly VirtualPathProvider _previous;
		private static readonly ContextState<Dictionary<string, InheritedThemeFileResult>> _requestState;

		static ThemingVirtualPathProvider()
		{
			_requestState = new ContextState<Dictionary<string, InheritedThemeFileResult>>("ThemeFileResolver.RequestCache", () => new Dictionary<string, InheritedThemeFileResult>());
		}

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
			string debugPath = ResolveDebugFilePath(virtualPath);
			if (debugPath != null)
			{
				return new DebugPluginVirtualFile(virtualPath, debugPath);
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
			string debugPath = ResolveDebugFilePath(virtualPath);
			if (debugPath != null)
			{
				return new CacheDependency(debugPath);
			}

			return new CacheDependency(MapDependencyPaths(virtualPathDependencies.Cast<string>()), utcStart);
		}

		public override string GetFileHash(string virtualPath, IEnumerable virtualPathDependencies)
		{
			string debugPath = ResolveDebugFilePath(virtualPath);
			if (debugPath != null)
			{
				return File.GetLastWriteTime(debugPath).ToString();
			}

			return _previous.GetFileHash(virtualPath, virtualPathDependencies);
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
					if (_isDebug)
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
			var d = _requestState.GetState();

			InheritedThemeFileResult result;
			if (!d.TryGetValue(virtualPath, out result))
			{
				result = d[virtualPath] = EngineContext.Current.Resolve<IThemeFileResolver>().Resolve(virtualPath);
			}

			return result;
		}

    }
}