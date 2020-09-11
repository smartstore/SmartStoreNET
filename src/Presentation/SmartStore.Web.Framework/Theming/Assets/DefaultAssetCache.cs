using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;
using System.Web.Optimization;
using SmartStore.Core;
using SmartStore.Core.Domain.Themes;
using SmartStore.Core.IO;
using SmartStore.Core.Logging;
using SmartStore.Core.Themes;
using SmartStore.Services;
using SmartStore.Utilities.Threading;

namespace SmartStore.Web.Framework.Theming.Assets
{
    public class DefaultAssetCache : IAssetCache
    {
        public const string MinificationCode = "min";
        public const string AutoprefixCode = "autoprefix";
        public const string UrlRewriteCode = "urlrewrite";

        public static readonly IAssetCache Null = new NullCache();
        const string CacheKeyPrefix = "sm:AssetCacheEntry:";

        private readonly bool _isEnabled;
        private readonly ThemeSettings _themeSettings;
        private readonly IApplicationEnvironment _env;
        private readonly IThemeContext _themeContext;
        private readonly IThemeRegistry _themeRegistry;
        private readonly IThemeFileResolver _themeFileResolver;
        private readonly ICommonServices _services;

        private VirtualFolder _cacheFolder;

        public DefaultAssetCache(
            ThemeSettings themeSettings,
            IApplicationEnvironment env,
            IThemeFileResolver themeFileResolver,
            IThemeContext themeContext,
            IThemeRegistry themeRegistry,
            ICommonServices services)
        {
            _themeSettings = themeSettings;
            _isEnabled = _themeSettings.AssetCachingEnabled == 2 || (_themeSettings.AssetCachingEnabled == 0 && !HttpContext.Current.IsDebuggingEnabled);

            _env = env;
            _themeContext = themeContext;
            _themeFileResolver = themeFileResolver;
            _themeRegistry = themeRegistry;
            _services = services;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public IVirtualFolder CacheFolder
        {
            get
            {
                if (_cacheFolder == null)
                {
                    _cacheFolder = new VirtualFolder(
                        _env.TenantFolder.GetVirtualPath("AssetCache"),
                        _env.TenantFolder.VirtualPathProvider,
                        Logger);
                }

                return _cacheFolder;
            }
        }

        public CachedAssetEntry GetAsset(string virtualPath)
        {
            if (!_isEnabled)
                return null;

            Guard.NotEmpty(virtualPath, nameof(virtualPath));

            var cacheDirectoryName = ResolveCacheDirectoryName(virtualPath, out string themeName, out int storeId);

            if (CacheFolder.DirectoryExists(cacheDirectoryName))
            {
                try
                {
                    var deps = CacheFolder.ReadFile(CacheFolder.Combine(cacheDirectoryName, "asset.dependencies"));
                    var hash = CacheFolder.ReadFile(CacheFolder.Combine(cacheDirectoryName, "asset.hash"));

                    if (!TryValidate(virtualPath, deps, hash, out IEnumerable<string> parsedDeps, out string currentHash))
                    {
                        Logger.DebugFormat("Invalidating cached asset for '{0}' because it is not valid anymore.", virtualPath);
                        InvalidateAssetInternal(cacheDirectoryName, themeName, storeId);
                        return null;
                    }

                    // TODO: determine correct file extension
                    var content = CacheFolder.ReadFile(CacheFolder.Combine(cacheDirectoryName, "asset.css"));
                    if (content == null)
                    {
                        using (KeyedLock.Lock(BuildLockKey(cacheDirectoryName)))
                        {
                            InvalidateAssetInternal(cacheDirectoryName, themeName, storeId);
                            return null;
                        }
                    }

                    var codes = ParseFileContent(CacheFolder.ReadFile(CacheFolder.Combine(cacheDirectoryName, "asset.pcodes")));

                    var entry = new CachedAssetEntry
                    {
                        Content = content,
                        HashCode = currentHash,
                        OriginalVirtualPath = virtualPath,
                        VirtualPathDependencies = parsedDeps,
                        PhysicalPath = CacheFolder.MapPath(cacheDirectoryName),
                        ThemeName = themeName,
                        StoreId = storeId,
                        ProcessorCodes = codes.ToArray()
                    };

                    SetupEvictionObserver(entry);

                    Logger.DebugFormat("Succesfully read asset '{0}' from cache.", virtualPath);

                    return entry;
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat(ex, "Error while resolving asset '{0}' from the asset cache.", virtualPath);
                }
            }

            return null;
        }

        public CachedAssetEntry InsertAsset(string virtualPath, IEnumerable<string> virtualPathDependencies, string content, params string[] processorCodes)
        {
            if (!_isEnabled)
                return null;

            Guard.NotEmpty(virtualPath, nameof(virtualPath));
            Guard.NotEmpty(content, nameof(content));

            var cacheDirectoryName = ResolveCacheDirectoryName(virtualPath, out string themeName, out int storeId);

            using (KeyedLock.Lock(BuildLockKey(cacheDirectoryName)))
            {
                CacheFolder.CreateDirectory(cacheDirectoryName);

                try
                {
                    // Save main content file
                    // TODO: determine correct file extension
                    CreateFileFromEntries(cacheDirectoryName, "asset.css", new[] { content });

                    // Save dependencies file
                    var deps = ResolveVirtualPathDependencies(virtualPath, virtualPathDependencies, themeName);
                    CreateFileFromEntries(cacheDirectoryName, "asset.dependencies", deps);

                    // Save hash file
                    var currentHash = BundleTable.VirtualPathProvider.GetFileHash(virtualPath, deps);
                    CreateFileFromEntries(cacheDirectoryName, "asset.hash", new[] { currentHash });

                    // Save codes file
                    CreateFileFromEntries(cacheDirectoryName, "asset.pcodes", processorCodes);

                    var entry = new CachedAssetEntry
                    {
                        Content = content,
                        HashCode = currentHash,
                        OriginalVirtualPath = virtualPath,
                        VirtualPathDependencies = deps,
                        PhysicalPath = CacheFolder.MapPath(cacheDirectoryName),
                        ThemeName = themeName,
                        StoreId = storeId,
                        ProcessorCodes = processorCodes
                    };

                    SetupEvictionObserver(entry);

                    Logger.DebugFormat("Succesfully inserted asset '{0}' to cache.", virtualPath);

                    return entry;
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat(ex, "Error while inserting asset '{0}' to the asset cache.", virtualPath);
                    InvalidateAssetInternal(cacheDirectoryName, themeName, storeId);
                }
            }

            return null;
        }

        public void Clear()
        {
            if (_env.TenantFolder.DirectoryExists("AssetCache"))
            {
                _env.TenantFolder.TryDeleteDirectory("AssetCache");
                // Remove the eviction observer also
                HttpRuntime.Cache.RemoveByPattern(CacheKeyPrefix);
            }
        }

        public bool InvalidateAsset(string virtualPath)
        {
            Guard.NotEmpty(virtualPath, nameof(virtualPath));

            var cacheDirectoryName = ResolveCacheDirectoryName(virtualPath, out string themeName, out int storeId);

            using (KeyedLock.Lock(BuildLockKey(cacheDirectoryName)))
            {
                if (CacheFolder.DirectoryExists(cacheDirectoryName))
                {
                    return InvalidateAssetInternal(cacheDirectoryName, themeName, storeId);
                }

                return false;
            }
        }

        private string BuildLockKey(string dirName)
        {
            return "AssetCache.Dir." + dirName;
        }

        /// <summary>
        /// Invalidates a cached asset when any themevar was changed or the theme was touched on file system
        /// </summary>
        /// <param name="entry"></param>
        private static void SetupEvictionObserver(CachedAssetEntry entry)
        {
            if (entry.ThemeName == null)
                return;

            var cacheKey = CacheKeyPrefix + "{0}:{1}".FormatInvariant(entry.ThemeName, entry.StoreId);

            var cacheDependency = new CacheDependency(
                new string[0],
                new[] { FrameworkCacheConsumer.BuildThemeVarsCacheKey(entry.ThemeName, entry.StoreId) },
                DateTime.UtcNow);

            HttpRuntime.Cache.Insert(
                cacheKey,
                entry.PhysicalPath,
                cacheDependency,
                Cache.NoAbsoluteExpiration,
                Cache.NoSlidingExpiration,
                CacheItemPriority.Default,
                OnCacheItemRemoved);
        }

        private static void OnCacheItemRemoved(string key, object value, CacheItemRemovedReason reason)
        {
            if (HostingEnvironment.ShutdownReason > ApplicationShutdownReason.None)
            {
                // Don't evict cached files during app shutdown. Dependant cache keys change
                // during a shutdown caused by HttpRuntime.Cache full eviction.
                return;
            }

            // Keep this low level
            var path = value as string;
            if (path.HasValue() && reason == CacheItemRemovedReason.DependencyChanged)
            {
                try
                {
                    Directory.Delete(path, true);
                }
                catch { }
            }
        }

        private IEnumerable<string> ResolveVirtualPathDependencies(string virtualPath, IEnumerable<string> deps, string themeName)
        {
            var list = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var d in (deps ?? new string[0]).Concat(new[] { virtualPath }))
            {
                var result = _themeFileResolver.Resolve(d);
                list.Add(result != null ? result.ResultVirtualPath : d);
            }

            // Add all theme.config files in the theme hierarchy chain
            var manifest = _themeRegistry.GetThemeManifest(themeName);
            while (manifest != null)
            {
                list.Add(VirtualPathUtility.ToAbsolute(CacheFolder.Combine(manifest.Location, manifest.ThemeName, "theme.config")));
                manifest = manifest.BaseTheme;
            }

            return list;
        }

        private bool InvalidateAssetInternal(string cacheDirectoryName, string theme, int storeId)
        {
            if (CacheFolder.TryDeleteDirectory(cacheDirectoryName))
            {
                if (theme.HasValue())
                {
                    // Remove the eviction observer
                    var cacheKey = CacheKeyPrefix + "{0}:{1}".FormatInvariant(theme, storeId);
                    HttpRuntime.Cache.Remove(cacheKey);

                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks whether asset is up-to-date.
        /// </summary>
        /// <param name="lastDeps"></param>
        /// <param name="lastHash"></param>
        /// <param name="parsedDeps"></param>
        /// <param name="currentHash"></param>
        /// <returns><c>false</c> if asset is not valid anymore and must be evicted from cache</returns>
        private bool TryValidate(string virtualPath, string lastDeps, string lastHash, out IEnumerable<string> parsedDeps, out string currentHash)
        {
            parsedDeps = null;
            currentHash = null;

            try
            {
                if (lastDeps.IsEmpty() || lastHash.IsEmpty())
                {
                    return false;
                }

                parsedDeps = ParseFileContent(lastDeps);

                // Check if dependency files hash matches the last saved hash
                currentHash = BundleTable.VirtualPathProvider.GetFileHash(virtualPath, parsedDeps);

                return lastHash == currentHash;
            }
            catch
            {
                return false;
            }
        }

        private string ResolveCacheDirectoryName(string virtualPath, out string themeName, out int storeId)
        {
            themeName = null;
            storeId = 0;

            if (virtualPath[0] != '~')
            {
                virtualPath = VirtualPathUtility.ToAppRelative(virtualPath);
            }

            // Build a dir name: ~/Themes/Flex/Content/theme.scss > themes-flex-content-theme.scss-flex-1
            var folderName = virtualPath.TrimStart('~', '/', '\\')
                .Replace('/', '-')
                .Replace('\\', '-');

            if (ThemeHelper.PathIsInheritableThemeFile(virtualPath))
            {
                themeName = _themeContext.CurrentTheme.ThemeName;
                storeId = _services.StoreContext.CurrentStore.Id;
                folderName += "-" + themeName + "-" + storeId;
            }

            return folderName.ToLowerInvariant();
        }

        private IEnumerable<string> ParseFileContent(string content)
        {
            var list = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (content.IsEmpty()) return list;

            var sr = new StringReader(content);
            while (true)
            {
                var f = sr.ReadLine();
                if (f != null && f.HasValue())
                {
                    list.Add(f);
                }
                else
                {
                    break;
                }
            }

            return list;
        }

        private void CreateFileFromEntries(string dirName, string fileName, IEnumerable<string> entries)
        {
            if (entries == null || !entries.Any())
                return;

            var sb = new StringBuilder();
            foreach (var f in entries)
            {
                sb.AppendLine(f);
            }

            var content = sb.ToString().TrimEnd();
            if (content.HasValue())
            {
                CacheFolder.CreateTextFile(CacheFolder.Combine(dirName, fileName), content);
            }
        }

        class NullCache : IAssetCache
        {
            public void Clear()
            {
            }

            public CachedAssetEntry GetAsset(string virtualPath)
            {
                return null;
            }

            public CachedAssetEntry InsertAsset(string virtualPath, IEnumerable<string> virtualPathDependencies, string content, params string[] processorCodes)
            {
                return null;
            }

            public bool InvalidateAsset(string virtualPath)
            {
                return false;
            }
        }
    }
}
