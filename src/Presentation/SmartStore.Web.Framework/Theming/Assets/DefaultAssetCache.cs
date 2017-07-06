using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.Optimization;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.IO;
using SmartStore.Core.Logging;
using SmartStore.Core.Themes;
using SmartStore.Services;

namespace SmartStore.Web.Framework.Theming.Assets
{
	public class DefaultAssetCache : IAssetCache
	{
		public readonly static IAssetCache Null = new NullCache();
		private readonly static object _lock = new object();

		private readonly IApplicationEnvironment _env;
		private readonly IThemeContext _themeContext;
		private readonly IThemeRegistry _themeRegistry;
		private readonly IThemeFileResolver _themeFileResolver;
		private readonly ICommonServices _services;

		private VirtualFolder _cacheFolder;

		public DefaultAssetCache(
			IApplicationEnvironment env,
			IThemeFileResolver themeFileResolver,
			IThemeContext themeContext,
			IThemeRegistry themeRegistry,
			ICommonServices services)
		{
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
			Guard.NotEmpty(virtualPath, nameof(virtualPath));

			string themeName;
			int storeId;
			var cacheDirectoryName = ResolveCacheDirectoryName(virtualPath, out themeName, out storeId);
			
			if (CacheFolder.DirectoryExists(cacheDirectoryName))
			{
				var deps = CacheFolder.ReadFile(CacheFolder.Combine(cacheDirectoryName, "asset.dependencies"));
				var hash = CacheFolder.ReadFile(CacheFolder.Combine(cacheDirectoryName, "asset.hash"));

				IEnumerable<string> parsedDeps;
				string currentHash;

				if (!TryValidate(virtualPath, deps, hash, out parsedDeps, out currentHash))
				{
					Logger.DebugFormat("Invalidating cached asset for '{0}' because it is not valid anymore.", virtualPath);
					InvalidateAssetInternal(cacheDirectoryName);
					return null;
				}

				// TODO: determine correct file extension
				var content = CacheFolder.ReadFile(CacheFolder.Combine(cacheDirectoryName, "asset.css")); 
				if (content == null)
				{
					lock (_lock)
					{
						InvalidateAssetInternal(cacheDirectoryName);
						return null;
					}
				}			

				var entry = new CachedAssetEntry
				{
					Content = content,
					HashCode = currentHash,
					OriginalVirtualPath = virtualPath,
					VirtualPathDependencies = parsedDeps,
					PhysicalPath = CacheFolder.MapPath(cacheDirectoryName),
					ThemeName = themeName,
					StoreId = storeId
				};

				SetupEvictionObserver(entry);

				Logger.DebugFormat("Succesfully read asset '{0}' from cache.", virtualPath);

				return entry;
			}

			return null;
		}

		public CachedAssetEntry InsertAsset(string virtualPath, IEnumerable<string> virtualPathDependencies, string content)
		{
			Guard.NotEmpty(virtualPath, nameof(virtualPath));
			Guard.NotEmpty(content, nameof(content));

			lock (_lock)
			{
				string themeName;
				int storeId;
				var cacheDirectoryName = ResolveCacheDirectoryName(virtualPath, out themeName, out storeId);

				CacheFolder.CreateDirectory(cacheDirectoryName);

				try
				{
					// Save main content file
					// TODO: determine correct file extension
					CacheFolder.CreateTextFile(CacheFolder.Combine(cacheDirectoryName, "asset.css"), content);

					// Save dependencies file
					var deps = ResolveVirtualPathDependencies(virtualPath, virtualPathDependencies, themeName);
					CacheFolder.CreateTextFile(CacheFolder.Combine(cacheDirectoryName, "asset.dependencies"), CreateDependenciesFile(deps));

					// Save hash file
					var currentHash = BundleTable.VirtualPathProvider.GetFileHash(virtualPath, deps);
					CacheFolder.CreateTextFile(CacheFolder.Combine(cacheDirectoryName, "asset.hash"), currentHash);

					var entry = new CachedAssetEntry
					{
						Content = content,
						HashCode = currentHash,
						OriginalVirtualPath = virtualPath,
						VirtualPathDependencies = deps,
						PhysicalPath = CacheFolder.MapPath(cacheDirectoryName),
						ThemeName = themeName,
						StoreId = storeId
					};

					SetupEvictionObserver(entry);

					Logger.DebugFormat("Succesfully inserted asset '{0}' to cache.", virtualPath);

					return entry;
				}
				catch
				{
					if (CacheFolder.DirectoryExists(cacheDirectoryName))
					{
						InvalidateAssetInternal(cacheDirectoryName);
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Invalidates a cached assets when any themevar was changed or the theme was touched on file system
		/// </summary>
		/// <param name="entry"></param>
		private static void SetupEvictionObserver(CachedAssetEntry entry)
		{
			if (entry.ThemeName.HasValue())
			{
				var cacheKey = "sm:AssetCacheEntry:{0}:{1}".FormatInvariant(entry.ThemeName, entry.StoreId);

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
		}

		private static void OnCacheItemRemoved(string key, object value, CacheItemRemovedReason reason)
		{
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
				list.Add(CacheFolder.Combine(manifest.Location, manifest.ThemeName, "theme.config"));
				manifest = manifest.BaseTheme;
			}

			return list;
		}

		public bool InvalidateAsset(string virtualPath)
		{
			Guard.NotEmpty(virtualPath, nameof(virtualPath));

			lock (_lock)
			{
				string themeName;
				int storeId;
				var cacheDirectoryName = ResolveCacheDirectoryName(virtualPath, out themeName, out storeId);

				if (CacheFolder.DirectoryExists(cacheDirectoryName))
				{
					InvalidateAssetInternal(cacheDirectoryName);
					return true;
				}

				return false;
			}
		}

		private void InvalidateAssetInternal(string cacheDirectoryName)
		{
			CacheFolder.DeleteDirectory(cacheDirectoryName);
		}

		public void Clear()
		{
			lock (_lock)
			{
				if (_env.TenantFolder.DirectoryExists("AssetCache"))
				{
					_env.TenantFolder.DeleteDirectory("AssetCache");
				}
			}
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

				parsedDeps = ParseDependenciesFile(lastDeps);

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

		private IEnumerable<string> ParseDependenciesFile(string deps)
		{
			var list = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			var sr = new StringReader(deps);
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

		private string CreateDependenciesFile(IEnumerable<string> deps)
		{
			var sb = new StringBuilder();
			foreach (var f in deps)
			{
				//var f2 = f;
				//if (f2[0] != '~')
				//{
				//	f2 = VirtualPathUtility.ToAppRelative(f);
				//}
				sb.AppendLine(f);
			}

			return sb.ToString();
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

			public CachedAssetEntry InsertAsset(string virtualPath, IEnumerable<string> virtualPathDependencies, string content)
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
