using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;

namespace SmartStore.Web.Framework.Plugins
{
	public class PluginDebugViewVirtualPathProvider : VirtualPathProvider
	{
		private readonly ConcurrentDictionary<string, string> _cachedDebugFilePaths = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		private readonly DirectoryInfo _pluginsDebugDir;

		public PluginDebugViewVirtualPathProvider()
		{
			var appRootPath = HostingEnvironment.MapPath("~/").EnsureEndsWith("\\");
			var debugPath = Path.GetFullPath(Path.Combine(appRootPath, @"..\..\Plugins"));
			if (Directory.Exists(debugPath))
			{
				_pluginsDebugDir = new DirectoryInfo(debugPath);
			}
		}

		public override bool FileExists(string virtualPath)
		{
			// Require files in production path to exist, do never fallback to dev path.
			// Doing so could lead to deployment errors (e.g. forgetting to copy a view file to production folder)
			return Previous.FileExists(virtualPath);
		}

		public override VirtualFile GetFile(string virtualPath)
		{
			if (!IsPluginPath(virtualPath))
			{
				return Previous.GetFile(virtualPath);
			}

			string debugPath = ResolveDebugFilePath(virtualPath);
			return debugPath != null
				? new DebugPluginVirtualFile(virtualPath, debugPath)
				: Previous.GetFile(virtualPath);
		}

		public override CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
		{
			if (!IsPluginPath(virtualPath))
			{
				return Previous.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
			}

			string debugPath = ResolveDebugFilePath(virtualPath);
			return debugPath != null 
				? new CacheDependency(debugPath) 
				: Previous.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
		}

		public override string GetFileHash(string virtualPath, IEnumerable virtualPathDependencies)
		{
			if (!IsPluginPath(virtualPath))
			{
				return Previous.GetFileHash(virtualPath, virtualPathDependencies);
			}

			string debugPath = ResolveDebugFilePath(virtualPath);
			return debugPath != null
				? File.GetLastWriteTime(debugPath).ToString()
				: Previous.GetFileHash(virtualPath, virtualPathDependencies);
		}

		private string ResolveDebugFilePath(string virtualPath)
		{
			return _cachedDebugFilePaths.GetOrAdd(virtualPath, FindDebugFile);
		}

		private string FindDebugFile(string virtualPath)
		{
			if (_pluginsDebugDir == null)
				return null;
			
			var unrooted = virtualPath.Substring(9); // strip "/Plugins/"
			string area = unrooted.Substring(0, unrooted.IndexOf('/'));

			var pluginDir = _pluginsDebugDir.EnumerateDirectories("*{0}*".FormatInvariant(area), SearchOption.TopDirectoryOnly).FirstOrDefault();
			if (pluginDir != null)
			{
				// get "Views/Something/View.cshtml"
				var viewPath = unrooted.Substring(area.Length + 1);
				var result = Path.Combine(pluginDir.FullName, viewPath).Replace("/", "\\");
				return File.Exists(result) ? result : null;
			}

			return null;
		}

		private static bool IsPluginPath(string virtualPath)
		{
			var result = virtualPath.StartsWith("/Plugins/", StringComparison.InvariantCultureIgnoreCase);
			return result;
		}

	}

	internal class DebugPluginVirtualFile : VirtualFile
	{
		private readonly string _debugPath;

		public DebugPluginVirtualFile(string virtualPath, string debugPath)
			: base(virtualPath)
		{
			this._debugPath = debugPath;
		}
		
		public override bool IsDirectory
		{
			get { return false; }
		}
		
		public override Stream Open()
		{
			var fileView = new FileStream(_debugPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			return fileView;
		}
	}
}
