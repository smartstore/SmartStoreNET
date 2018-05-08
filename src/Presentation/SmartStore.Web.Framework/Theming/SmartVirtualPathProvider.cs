using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Hosting;
using System.Threading;
using SmartStore.Core.Infrastructure;
using SmartStore.Utilities.Threading;
using SmartStore.Utilities;

namespace SmartStore.Web.Framework.Theming
{
	public abstract class SmartVirtualPathProvider : VirtualPathProvider
	{
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
		}

		protected string ResolveDebugFilePath(string virtualPath)
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
			
			var unrooted = appRelativePath.Substring(root.Length); // strip "~/Plugins/" or "~/Themes/"
			string area = unrooted.Substring(0, unrooted.IndexOf('/'));

			// get "Views/Something/View.cshtml"
			var viewPath = unrooted.Substring(area.Length + 1);

			var foldersToCheck = new[] { area, area + "-sym" };

			foreach (var folder in foldersToCheck)
			{
				var pluginDir = new DirectoryInfo(Path.Combine(_pluginsDebugDir.FullName, folder));
				if (pluginDir != null && pluginDir.Exists)
				{
					var result = Path.Combine(pluginDir.FullName, viewPath).Replace("/", "\\");
					return File.Exists(result) ? result : null;
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
			var fileView = new FileStream(PhysicalPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			return fileView;
		}
	}
}
