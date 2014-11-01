using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Web;
using System.Web.Hosting;
using SmartStore.Core.Themes;
using SmartStore.Utilities;
using SmartStore.Core.Infrastructure;
using SmartStore.Core;
using System.Text.RegularExpressions;

namespace SmartStore.Web.Framework.Themes
{
	
	public class ThemeFileResolver : DisposableObject, IThemeFileResolver
	{
		private readonly ConcurrentDictionary<FileKey, InheritedThemeFileResult> _files = new ConcurrentDictionary<FileKey, InheritedThemeFileResult>();
		private FileSystemWatcher _monitor;
		private readonly Regex _monitorFilterPattern = new Regex(@"^\.(png|gif|jpg|jpeg|css|less|cshtml|svg|json)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private readonly string _themesBasePath;

		private readonly IThemeRegistry _themeRegistry;

		public ThemeFileResolver(IThemeRegistry themeRegistry)
		{
			this._themeRegistry = themeRegistry;
			this._themesBasePath = HostingEnvironment.MapPath(ThemeHelper.ThemesBasePath);

			MonitorFiles();
		}

		private void MonitorFiles() 
		{
			_monitor = new FileSystemWatcher();

			_monitor.Path = _themesBasePath;
			_monitor.Filter = "*.*";
			_monitor.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite | NotifyFilters.FileName;
			_monitor.IncludeSubdirectories = true;
			_monitor.EnableRaisingEvents = true;

			_monitor.Created += (s, e) => FileSystemChanged(e.Name, e.FullPath, true);
			_monitor.Deleted += (s, e) => FileSystemChanged(e.Name, e.FullPath, false);
			_monitor.Renamed += (s, e) => { FileSystemChanged(e.OldName, e.OldFullPath, false); FileSystemChanged(e.Name, e.FullPath, true); };
		}

		/// <summary>
		/// Only called when files are created or deleted, NOT when changed.
		/// </summary>
		/// <param name="name">The file name</param>
		/// <param name="fullPath">The full path</param>
		/// <param name="created">Indicates whether file was created or deleted</param>
		private void FileSystemChanged(string name, string fullPath, bool created)
		{
			if (!_monitorFilterPattern.IsMatch(Path.GetExtension(name)))
				return;

			var idx = name.IndexOf('\\');
			if (idx < 0)
			{
				return;
			}

			string themeName = name.Substring(0, idx);
			string relativePath = name.Substring(themeName.Length + 1).Replace('\\', '/');

			// get keys with same relative path
			var keys = _files.Keys.Where(x => x.RelativePath.IsCaseInsensitiveEqual(relativePath)).ToList();
			foreach (var key in keys)
			{
				if (key.ThemeName.IsCaseInsensitiveEqual(themeName) || _themeRegistry.IsChildThemeOf(key.ThemeName, themeName))
				{
					// remove all cached pathes for this theme or any of it's child themes
					InheritedThemeFileResult result;
					if (_files.TryRemove(key, out result))
					{
						if (created)
						{
							// The file is new: no chance that our VPP dependencies
							// could have been notified about this (only deletions are monitored).
							// Therefore we brutally set the result file's last write time
							// to enforece VPP to invalidate cache.
							try
							{
								File.SetLastWriteTimeUtc(result.ResultPhysicalPath, DateTime.UtcNow);
							}
							catch { }
						}
					}
				}
			}

		}

		protected override void OnDispose(bool disposing)
		{
			if (disposing)
			{
				if (_monitor != null)
				{
					_monitor.Dispose();
					_monitor = null;
				}
			}
		}

		public InheritedThemeFileResult Resolve(string virtualPath)
		{
			Guard.ArgumentNotEmpty(() => virtualPath);

			virtualPath = VirtualPathUtility.ToAppRelative(virtualPath);

			if (!ThemeHelper.PathIsInheritableThemeFile(virtualPath))
			{
				return null;
			}

			string requestedThemeName;
			string relativePath;
			TokenizePath(virtualPath, out requestedThemeName, out relativePath);

			ThemeManifest currentTheme;

			var isAdmin = EngineContext.Current.Resolve<IWorkContext>().IsAdmin;
			if (isAdmin)
			{
				currentTheme = _themeRegistry.GetThemeManifest(requestedThemeName);
			}
			else
			{
				currentTheme = ThemeHelper.ResolveCurrentTheme();
				if (currentTheme.BaseThemeName == null)
				{
					// dont't bother resolving files: the current theme is not inherited.
					// Let the current VPP do the work.
					return null;
				}
			}

			if (!currentTheme.ThemeName.Equals(requestedThemeName, StringComparison.OrdinalIgnoreCase))
			{
				if (!_themeRegistry.IsChildThemeOf(currentTheme.ThemeName, requestedThemeName))
				{
					return null;
				}
			}

			var fileKey = new FileKey(currentTheme.ThemeName, relativePath);

			var result = _files.GetOrAdd(fileKey, (k) =>
			{
				// ALWAYS begin the search with the current working theme's location!
				string resultVirtualPath;
				string resultPhysicalPath;
				string actualLocation = LocateFile(currentTheme.ThemeName, relativePath, out resultVirtualPath, out resultPhysicalPath);
				
				if (actualLocation != null)
				{
					return new InheritedThemeFileResult
					{
						RelativePath = relativePath,
						OriginalVirtualPath = virtualPath,
						ResultVirtualPath = resultVirtualPath,
						ResultPhysicalPath = resultPhysicalPath,
						OriginalThemeName = requestedThemeName,
						ResultThemeName = actualLocation
					};
				}

				return null;
			});

			return result;
		}

		private void TokenizePath(string virtualPath, out string themeName, out string relativePath)
		{
			themeName = null;
			relativePath = null;

			var unrooted = virtualPath.Substring(ThemeHelper.ThemesBasePath.Length); // strip "~/Themes/"
			themeName = unrooted.Substring(0, unrooted.IndexOf('/'));
			relativePath = unrooted.Substring(themeName.Length + 1);
		}

		/// <summary>
		/// Tries to locate the file
		/// </summary>
		/// <returns>the theme's name where the file is actually located</returns>
		private string LocateFile(string themeName, string relativePath, out string virtualPath, out string physicalPath)
		{
			virtualPath = null;
			physicalPath = null;

			virtualPath = VirtualPathUtility.Combine("{0}{1}/".FormatInvariant(ThemeHelper.ThemesBasePath, themeName), relativePath);
			physicalPath = HostingEnvironment.MapPath(virtualPath);
			if (File.Exists(physicalPath))
			{
				return themeName;
			}

			var manifest = _themeRegistry.GetThemeManifest(themeName);
			if (manifest != null && manifest.BaseThemeName.HasValue())
			{
				var baseLocation = LocateFile(manifest.BaseThemeName, relativePath, out virtualPath, out physicalPath);
				return baseLocation;
			}

			virtualPath = null;
			physicalPath = null;
			return null;
		}


		private class FileKey : Tuple<string, string>
		{
			public FileKey(string themeName, string relativePath)
				: base(themeName.ToLower(), relativePath.ToLower())
			{
			}

			public string ThemeName
			{
				get { return base.Item1; }
			}

			public string RelativePath
			{
				get { return base.Item2; }
			}
		}

	}

}
