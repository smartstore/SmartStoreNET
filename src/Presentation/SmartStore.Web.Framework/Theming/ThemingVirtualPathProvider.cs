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
                if (!result.IsBased)
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
                if (!result.IsBased)
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

            var fileNames = MapDependencyPaths(virtualPathDependencies.Cast<string>(), out _);
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

            var mappedPaths = MapDependencyPaths(virtualPathDependencies.Cast<string>(), out var cacheKeys);

            return new CacheDependency(mappedPaths, cacheKeys, utcStart);
        }

        /// <summary>
        /// Maps virtual to physical paths. Used to compute cache dependecies and file hashes.
        /// </summary>
        internal string[] MapDependencyPaths(IEnumerable<string> virtualPathDependencies, out string[] cacheKeys)
        {
            cacheKeys = null;

            var mappedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var cacheKeySet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var dep in virtualPathDependencies)
            {
                var file = GetFile(dep);

                if (file is IFileDependencyProvider provider)
                {
                    provider.AddFileDependencies(mappedPaths, cacheKeySet);
                }
                else if (file != null)
                {
                    mappedPaths.Add(HostingEnvironment.MapPath(file.VirtualPath));
                }
            }

            cacheKeys = cacheKeySet.ToArray();

            var paths = mappedPaths.ToArray();
            Array.Sort<string>(paths);

            return paths;
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