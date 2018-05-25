using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Hosting;
using System.Threading;
using SmartStore.Core.Infrastructure;
using SmartStore.Utilities.Threading;
using SmartStore.Utilities;
using SmartStore.Core.Themes;

namespace SmartStore.Web.Framework.Theming
{
	public abstract class SmartVirtualPathProvider : VirtualPathProvider
	{
		private readonly IThemeRegistry _themeRegistry;
		private readonly Dictionary<string, string> _cachedDebugFilePaths = new Dictionary<string, string>();
		private readonly ContextState<Dictionary<string, string>> _requestState = new ContextState<Dictionary<string, string>>("PluginDebugViewVPP.RequestCache", () => new Dictionary<string, string>());
		private readonly DirectoryInfo _pluginsDebugDir;
		private readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();

		protected static readonly bool _isDebug;

		static SmartVirtualPathProvider()
		{
			_isDebug = HttpContext.Current.IsDebuggingEnabled && CommonHelper.IsDevEnvironment;
		}

		protected SmartVirtualPathProvider()
		{
			var appRootPath = HostingEnvironment.MapPath("~/").EnsureEndsWith("\\");

			var pluginsDebugPath = Path.GetFullPath(Path.Combine(appRootPath, @"..\..\Plugins"));
			if (Directory.Exists(pluginsDebugPath))
			{
				_pluginsDebugDir = new DirectoryInfo(pluginsDebugPath);
			}

			_themeRegistry = EngineContext.Current.Resolve<IThemeRegistry>();
		}

		protected internal string ResolveDebugFilePath(string virtualPath)
		{
			if (!_isDebug)
				return null;

			// Two-Level caching: RequestCache > AppCache
			var d = _requestState.GetState();

			if (!d.TryGetValue(virtualPath, out var debugPath))
			{
				if (!IsExtensionPath(virtualPath, out var root, out var appRelativePath))
				{
					// don't query again in this request
					d[virtualPath] = null; 
					return null;
				}

				if (!d.TryGetValue(appRelativePath, out debugPath))
				{
					// (perf) concurrency with ReaderWriterLockSlim seems way faster than ConcurrentDictionary
					using (_rwLock.GetUpgradeableReadLock())
					{
						if (!_cachedDebugFilePaths.TryGetValue(appRelativePath, out debugPath))
						{
							using (_rwLock.GetWriteLock())
							{
								debugPath = FindDebugFile(appRelativePath, root);
								_cachedDebugFilePaths[appRelativePath] = d[appRelativePath] = debugPath;
							}
						}
					}
				}
			}

			return debugPath;
		}

		private string FindDebugFile(string appRelativePath, string root)
		{
			if (_pluginsDebugDir == null)
				return null;

			// strip "~/Plugins/" or "~/Themes/"
			var unrooted = appRelativePath.Substring(root.Length);

			// either plugin or theme name 
			var extensionName = unrooted.Substring(0, unrooted.IndexOf('/'));

			// get "Views/Something/View.cshtml"
			var relativePath = unrooted.Substring(extensionName.Length + 1);

			if (root == "~/Themes/")
			{
				var theme = _themeRegistry.GetThemeManifest(extensionName);
				if (theme != null && theme.IsSymbolicLink)
				{
					// Linked theme folders cannot compute cache dependencies correctly when
					// working with source paths. We must determine the link target path, 
					var finalPath = Path.Combine(theme.Path, relativePath.Replace('/', '\\'));
					return File.Exists(finalPath) ? finalPath : null;
				}
			}
			else
			{
				// Root is "~/Plugin/"
				var foldersToCheck = new[] { extensionName, extensionName + "-sym" };

				foreach (var folder in foldersToCheck)
				{
					var pluginDir = new DirectoryInfo(Path.Combine(_pluginsDebugDir.FullName, folder));
					if (pluginDir != null && pluginDir.Exists)
					{
						var result = Path.Combine(pluginDir.FullName, relativePath).Replace("/", "\\");
						return File.Exists(result) ? result : null;
					}
				}
			}

			return null;
		}

		private bool IsExtensionPath(string virtualPath, out string root, out string appRelativePath)
		{
			root = null;
			appRelativePath = virtualPath;

			if (virtualPath != null && virtualPath.Length > 0 && virtualPath[0] != '~')
			{
				appRelativePath = VirtualPathUtility.ToAppRelative(virtualPath);
			}

			if (appRelativePath.StartsWith("~/Plugins/", StringComparison.InvariantCultureIgnoreCase))
			{
				root = "~/Plugins/";
				return true;
			}

			if (appRelativePath.StartsWith("~/Themes/", StringComparison.InvariantCultureIgnoreCase))
			{
				root = "~/Themes/";
				return true;
			}

			return false;
		}
	}

	internal class DebugVirtualFile : VirtualFile
	{
		public DebugVirtualFile(string virtualPath, string debugPath)
			: base(virtualPath)
		{
			this.PhysicalPath = debugPath;
		}

		public string PhysicalPath { get; }

		public override bool IsDirectory
		{
			get { return false; }
		}
		
		public override Stream Open()
		{
			var fileStream = new FileStream(PhysicalPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			return fileStream;
		}
	}
}
