using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Caching;
using System.Web.Hosting;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Themes;
using SmartStore.Utilities;

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
					if (result.Query.HasValue() && result.Query.IndexOf('.') >= 0)
					{
						// libSass tries to locate files by appending .[s]css extension to our querystring. Prevent this shit!
						return false;
					}
					else
					{
						// Let system VPP check for this file
						virtualPath = result.ResultVirtualPath ?? result.OriginalVirtualPath;
					}
				}
			}

			return _previous.FileExists(virtualPath);
        }
         
        public override VirtualFile GetFile(string virtualPath)
        {
			VirtualFile file = null;
			string debugPath = null;

			var result = GetResolveResult(virtualPath);
			if (result != null)
			{
				// File is an inherited theme file. Set the result virtual path.
				virtualPath = result.ResultVirtualPath ?? result.OriginalVirtualPath;
				if (!result.IsExplicit)
				{
					file = new InheritedVirtualThemeFile(result);
				}
			}

			if (result == null || file is InheritedVirtualThemeFile)
			{
				// Handle plugin and symlinked theme folders in debug mode.
				debugPath = ResolveDebugFilePath(virtualPath);
				if (debugPath != null)
				{
					file = new DebugVirtualFile(file?.VirtualPath ?? virtualPath, debugPath);
				}
			}

			return file ?? _previous.GetFile(virtualPath);
        }

		public override string GetFileHash(string virtualPath, IEnumerable virtualPathDependencies)
		{
			if (virtualPathDependencies == null)
			{
				return _previous.GetFileHash(virtualPath, virtualPathDependencies);
			}

			var fileNames = MapDependencyPaths(virtualPathDependencies.Cast<string>());
			var combiner = HashCodeCombiner.Start();

			foreach (var fileName in fileNames)
			{
				combiner.Add(new FileInfo(fileName));
			}

			return combiner.CombinedHashString;
		}

		public override CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
		{
			if (virtualPathDependencies == null)
			{
				return null;
			}

			return new CacheDependency(MapDependencyPaths(virtualPathDependencies.Cast<string>()), utcStart);
		}

		internal static string[] MapDependencyPaths(IEnumerable<string> virtualPathDependencies)
		{
			// Maps virtual to physical paths. Used to compute cache dependecies and file hashes.

			var fileNames = new List<string>();

			foreach (var dep in virtualPathDependencies)
			{
				string mappedPath = null;
				var file = HostingEnvironment.VirtualPathProvider.GetFile(dep);

				if (file is InheritedVirtualThemeFile file1)
				{
					mappedPath = file1.ResolveResult.ResultPhysicalPath;
				}
				else if (file is DebugVirtualFile file2)
				{
					mappedPath = file2.PhysicalPath;
				}
				else if (file != null)
				{
					mappedPath = HostingEnvironment.MapPath(file.VirtualPath);
				}

				if (mappedPath.HasValue())
				{
					fileNames.Add(mappedPath);
				}
			}

			return fileNames.ToArray();
		}

		private static InheritedThemeFileResult GetResolveResult(string virtualPath)
		{
			var d = _requestState.GetState();

			if (!d.TryGetValue(virtualPath, out var result))
			{
				result = d[virtualPath] = EngineContext.Current.Resolve<IThemeFileResolver>().Resolve(virtualPath);
			}

			return result;
		}

    }
}