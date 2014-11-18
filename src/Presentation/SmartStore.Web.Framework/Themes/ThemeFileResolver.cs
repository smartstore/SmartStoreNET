using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using SmartStore.Core;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Themes;

namespace SmartStore.Web.Framework.Themes
{

	public interface IThemeFileResolver
	{
		InheritedThemeFileResult Resolve(string virtualPath);
	}

	public class ThemeFileResolver : DisposableObject, IThemeFileResolver
	{
		private readonly ConcurrentDictionary<FileKey, InheritedThemeFileResult> _files = new ConcurrentDictionary<FileKey, InheritedThemeFileResult>();
		private readonly IThemeRegistry _themeRegistry;

		public ThemeFileResolver(IThemeRegistry themeRegistry)
		{
			this._themeRegistry = themeRegistry;

			// listen to file monitoring events
			this._themeRegistry.ThemeFolderDeleted += OnThemeFolderDeleted;
			this._themeRegistry.ThemeFolderRenamed += OnThemeFolderRenamed;
			this._themeRegistry.BaseThemeChanged += OnBaseThemeChanged;
			this._themeRegistry.ThemeFileChanged += OnThemeFileChanged;
		}

		private void OnThemeFolderDeleted(object sender, ThemeFolderDeletedEventArgs e)
		{
			OnThemeRemoved(e.Name);
		}

		private void OnThemeFolderRenamed(object sender, ThemeFolderRenamedEventArgs e)
		{
			OnThemeRemoved(e.OldName);
		}

		private void OnBaseThemeChanged(object sender, BaseThemeChangedEventArgs e)
		{
			// We should be smarter than just clearing the whole cache here. BUT:
			// Changing the base theme is a very rare case, whereas determining all dependant files is rather sophisticated.
			// So, who cares ;-)
			_files.Clear();
		}

		private void OnThemeRemoved(string themeName)
		{
			var keys = _files.Keys.Where(x => x.ThemeName.IsCaseInsensitiveEqual(themeName)).ToList();
			foreach (var key in keys)
			{
				if (key.ThemeName.IsCaseInsensitiveEqual(themeName) || _themeRegistry.IsChildThemeOf(key.ThemeName, themeName))
				{
					// remove all cached pathes for this theme (also in all derived themes)
					InheritedThemeFileResult result;
					_files.TryRemove(key, out result);
				}
			}
		}

		private void OnThemeFileChanged(object sender, ThemeFileChangedEventArgs e)
		{
			if (e.IsConfigurationFile)
				return;

			// get keys with same relative path
			var keys = _files.Keys.Where(x => x.RelativePath.IsCaseInsensitiveEqual(e.RelativePath)).ToList();
			foreach (var key in keys)
			{
				if (key.ThemeName.IsCaseInsensitiveEqual(e.ThemeName) || _themeRegistry.IsChildThemeOf(key.ThemeName, e.ThemeName))
				{
					// remove all cached pathes for this file/theme combination (also in all derived themes)
					InheritedThemeFileResult result;
					if (_files.TryRemove(key, out result))
					{
						if (e.ChangeType == ThemeFileChangeType.Created)
						{
							// The file is new: no chance that our VPP dependencies
							// could have been notified about this (only deletions/changes are monitored).
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
				this._themeRegistry.ThemeFileChanged -= OnThemeFileChanged;
				this._themeRegistry.ThemeFolderDeleted -= OnThemeFolderDeleted;
				this._themeRegistry.BaseThemeChanged -= OnBaseThemeChanged;
				this._themeRegistry.ThemeFolderRenamed -= OnThemeFolderRenamed;
			}
		}

		/// <summary>
		/// Tries to resolve a file up in the current theme's hierarchy chain.
		/// </summary>
		/// <param name="virtualPath">The original virtual path of the theme file</param>
		/// <returns>
		/// If the current working themme is based on another theme AND the requested file
		/// was physically found in the theme's hierarchy chain, an instance of <see cref="InheritedThemeFileResult" /> will be returned.
		/// In any other case the return value is <c>null</c>.
		/// </returns>
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
			var isAdmin = EngineContext.Current.Resolve<IWorkContext>().IsAdmin; // ThemeHelper.IsAdminArea()
			if (isAdmin)
			{
				currentTheme = _themeRegistry.GetThemeManifest(requestedThemeName);
			}
			else
			{
				bool isLess = false;
				if (ThemeHelper.IsStyleSheet(virtualPath, out isLess))
				{
					// special consideration for LESS files: they can be validated
					// in the backend. For validation, a "theme" query is appended 
					// to the url. During validation we must work with the actual
					// requested theme instead dynamically resolving the working theme.
					var httpContext = HttpContext.Current;
					if (httpContext != null && httpContext.Request != null)
					{
						var qs = httpContext.Request.QueryString;
						if (qs["theme"].HasValue())
						{
							EngineContext.Current.Resolve<IThemeContext>().SetRequestTheme(qs["theme"]);
						}
					}
				}
				
				currentTheme = ThemeHelper.ResolveCurrentTheme();
				if (currentTheme.BaseTheme == null)
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
			if (manifest != null && manifest.BaseTheme != null)
			{
				var baseLocation = LocateFile(manifest.BaseTheme.ThemeName, relativePath, out virtualPath, out physicalPath);
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

	public class InheritedThemeFileResult
	{
		/// <summary>
		/// The unrooted relative path of the file (without <c>~/Themes/ThemeName/</c>)
		/// </summary>
		public string RelativePath { get; set; }

		/// <summary>
		/// The original virtual path
		/// </summary>
		public string OriginalVirtualPath { get; set; }

		/// <summary>
		/// The result virtual path (the path in which the file is actually located)
		/// </summary>
		public string ResultVirtualPath { get; set; }

		/// <summary>
		/// The result physical path (the path in which the file is actually located)
		/// </summary>
		public string ResultPhysicalPath { get; set; }

		/// <summary>
		/// The name of the requesting theme
		/// </summary>
		public string OriginalThemeName { get; set; }

		/// <summary>
		/// The name of the resulting theme where the file is actually located
		/// </summary>
		public string ResultThemeName { get; set; }
	}

}
