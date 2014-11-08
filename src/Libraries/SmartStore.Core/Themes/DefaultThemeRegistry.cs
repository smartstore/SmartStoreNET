using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using SmartStore.Core;
using SmartStore.Core.Infrastructure;
using SmartStore.Utilities.Threading;
using SmartStore.Core.Themes;
using SmartStore.Utilities;
using SmartStore.Core.Caching;
using SmartStore.Core.IO.WebSite;
using SmartStore.Core.Events;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Web;

namespace SmartStore.Core.Themes
{
    public partial class DefaultThemeRegistry : DisposableObject, IThemeRegistry
    {
		#region Fields

		private readonly bool _enableMonitoring;
		private readonly string _themesBasePath;
		private readonly IEventPublisher _eventPublisher;
		private readonly ConcurrentDictionary<string, ThemeManifest> _themes = new ConcurrentDictionary<string, ThemeManifest>(StringComparer.InvariantCultureIgnoreCase);

		private readonly Regex _fileFilterPattern = new Regex(@"^\.(config|png|gif|jpg|jpeg|css|less|js|cshtml|svg|json)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		private FileSystemWatcher _monitorFolders;
		private FileSystemWatcher _monitorFiles;

		#endregion

		#region Constructors

		public DefaultThemeRegistry(IEventPublisher eventPublisher)
        {
			this._enableMonitoring = CommonHelper.GetAppSetting<bool>("sm:MonitorThemesFolder", true);
			this._themesBasePath = CommonHelper.GetAppSetting<string>("sm:ThemesBasePath", "~/Themes/").EnsureEndsWith("/");
			this._eventPublisher = eventPublisher;
			
			// load all themes initially
			LoadThemes();

			if (_enableMonitoring)
			{
				// start FS watcher
				StartMonitoring();
			}
        }

		#endregion 

		#region IThemeRegistry

        public bool ThemeManifestExists(string themeName)
        {
            if (themeName.IsEmpty())
                return false;

			return _themes.ContainsKey(themeName);
        }

		public ThemeManifest GetThemeManifest(string themeName)
		{
			ThemeManifest value;
			if (themeName.HasValue() && _themes.TryGetValue(themeName, out value))
			{
				return value;
			}
			return null;
		}

		private ThemeManifest GetOrAddThemeManifest(string themeName)
		{
			var manifest = GetThemeManifest(themeName);

			if (manifest == null)
			{
				var folderData = ThemeManifest.CreateThemeFolderDataByName(themeName, GetOrAddThemeManifest, _themesBasePath);
				if (folderData != null)
				{
					manifest = ThemeManifest.Create(folderData);
					if (manifest != null)
					{
						AddThemeManifest(manifest);
					}
				}
			}

			return manifest;
		}

		public ICollection<ThemeManifest> GetThemeManifests()
		{
			return _themes.Values.AsReadOnly();
		}

		public void AddThemeManifest(ThemeManifest manifest)
		{
			Guard.ArgumentNotNull(() => manifest);

			TryRemoveManifest(manifest.ThemeName);
			if (ValidateThemeInheritance(manifest, _themes))
			{
				_themes.TryAdd(manifest.ThemeName, manifest);
			}
		}

		private bool TryRemoveManifest(string themeName)
		{
			bool result;
			ThemeManifest existing;
			if (result = _themes.TryRemove(themeName, out existing))
			{
				_eventPublisher.Publish(new ThemeTouchedEvent(themeName));
			}
			return result;
		}

		public bool IsChildThemeOf(string themeName, string baseTheme)
		{
			if (themeName.IsEmpty() && baseTheme.IsEmpty())
			{
				return false;
			}

			if (themeName.Equals(baseTheme, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			var current = GetThemeManifest(themeName);
			if (current == null)
				return false;

			while (current != null && current.BaseTheme != null)
			{
				if (baseTheme.Equals(current.BaseTheme.ThemeName, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}

				current = current.BaseTheme;
			}

			return false;
		}

		private void LoadThemes()
		{
			var virtualBasePath = _themesBasePath;
			var basePath = CommonHelper.MapPath(virtualBasePath);
			var dirs = new DirectoryInfo(basePath).EnumerateDirectories("*", SearchOption.TopDirectoryOnly).ToList();

			foreach (var dir in dirs)
			{
				try
				{
					GetOrAddThemeManifest(dir.Name);

					//var manifest = GetOrAddThemeManifest(dir.Name);
					//if (manifest != null && !_themes.ContainsKey(manifest.ThemeName))
					//{
					//	_themes.TryAdd(manifest.ThemeName, manifest);
					//}
				}
				catch (Exception ex)
				{
					Debug.WriteLine("ERR - unable to create manifest for theme '{0}': {1}".FormatCurrent(dir.Name, ex.Message));
				}
			}
		}

		private bool ValidateThemeInheritance(ThemeManifest manifest, IDictionary<string, ThemeManifest> map)
		{
			return true;
			//var stack = new List<string>();

			//while (manifest.BaseThemeName != null)
			//{
			//	stack.Add(manifest.ThemeName);
				
			//	if (!map.ContainsKey(manifest.BaseThemeName))
			//	{
			//		Debug.WriteLine("The base theme does not exist");
			//		return false;
			//	}

			//	if (stack.Contains(manifest.BaseThemeName, StringComparer.OrdinalIgnoreCase))
			//	{
			//		Debug.WriteLine("Circular reference");
			//		return false;
			//	}

			//	manifest = map[manifest.BaseThemeName];
			//}

			//return true;
		}

        #endregion

		#region Monitoring & Events

		private void StartMonitoring()
		{
			_monitorFiles = new FileSystemWatcher();
			_monitorFiles.Path = CommonHelper.MapPath(_themesBasePath);
			_monitorFiles.InternalBufferSize = 32768; // 32 instead of the default 8 KB
			_monitorFiles.Filter = "*.*";
			_monitorFiles.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite | NotifyFilters.FileName;
			_monitorFiles.IncludeSubdirectories = true;
			_monitorFiles.EnableRaisingEvents = true;
			_monitorFiles.Changed += (s, e) => OnThemeFileChanged(e.Name, e.FullPath, ThemeFileChangeType.Modified);
			_monitorFiles.Deleted += (s, e) => OnThemeFileChanged(e.Name, e.FullPath, ThemeFileChangeType.Deleted);
			_monitorFiles.Created += (s, e) => OnThemeFileChanged(e.Name, e.FullPath, ThemeFileChangeType.Created);
			_monitorFiles.Renamed += (s, e) => { 
				OnThemeFileChanged(e.OldName, e.OldFullPath, ThemeFileChangeType.Deleted);
				OnThemeFileChanged(e.Name, e.FullPath, ThemeFileChangeType.Created); 
			};

			_monitorFolders = new FileSystemWatcher();
			_monitorFolders.Path = CommonHelper.MapPath(_themesBasePath);
			_monitorFolders.Filter = "*";
			_monitorFolders.NotifyFilter = NotifyFilters.DirectoryName;
			_monitorFolders.IncludeSubdirectories = false;
			_monitorFolders.EnableRaisingEvents = true;
			_monitorFolders.Renamed += (s, e) => OnThemeFolderRenamed(e.Name, e.FullPath, e.OldName, e.OldFullPath);
			_monitorFolders.Deleted += (s, e) => OnThemeFolderDeleted(e.Name, e.FullPath);
		}

		private void OnThemeFileChanged(string name, string fullPath, ThemeFileChangeType changeType)
		{
			if (!_fileFilterPattern.IsMatch(Path.GetExtension(name)))
				return;

			var idx = name.IndexOf('\\');
			if (idx < 0)
			{
				// must be a subfolder of "~/Themes/"
				return;
			}

			var themeName = name.Substring(0, idx);
			var relativePath = name.Substring(themeName.Length + 1).Replace('\\', '/');
			var isConfigFile = relativePath.IsCaseInsensitiveEqual("theme.config");

			if (changeType == ThemeFileChangeType.Modified && !isConfigFile)
			{
				// Monitor changes only for root theme.config
				return;
			}

			BaseThemeChangedEventArgs baseThemeChangedArgs = null;

			if (isConfigFile)
			{			
				// config file changes always result in refreshing the corresponding theme manifest
				var di = new DirectoryInfo(Path.GetDirectoryName(fullPath));

				ThemeManifest oldBaseTheme = null;
				var oldManifest = this.GetThemeManifest(di.Name);
				if (oldManifest != null)
				{
					oldBaseTheme = oldManifest.BaseTheme;
				}

				try
				{
					var newManifest = ThemeManifest.Create(di.FullName, GetOrAddThemeManifest, _themesBasePath);
					if (newManifest != null)
					{
						this.AddThemeManifest(newManifest);

						if (!oldBaseTheme.Equals(newManifest.BaseTheme))
						{
							baseThemeChangedArgs = new BaseThemeChangedEventArgs
							{ 
								ThemeName = newManifest.ThemeName,
								BaseTheme = newManifest.BaseTheme != null ? newManifest.BaseTheme.ThemeName : null,
								OldBaseTheme = oldBaseTheme != null ? oldBaseTheme.ThemeName : null
							};
						}

						Debug.WriteLine("Changed theme manifest for '{0}'".FormatCurrent(name));
					}
					else
					{
						// something went wrong (most probably no 'theme.config'): remove the manifest
						TryRemoveManifest(di.Name);
					}
				}
				catch (Exception ex)
				{
					TryRemoveManifest(di.Name);
					Debug.WriteLine("ERR - Could not touch theme manifest '{0}': {1}".FormatCurrent(name, ex.Message));
				}
			}

			if (baseThemeChangedArgs != null)
			{
				RaiseBaseThemeChanged(baseThemeChangedArgs);
			}

			RaiseThemeFileChanged(new ThemeFileChangedEventArgs { 
				ChangeType = changeType,
				FullPath = fullPath,
				ThemeName = themeName,
				RelativePath = relativePath,
				IsConfigurationFile = isConfigFile
			});
		}

		private void OnThemeFolderRenamed(string name, string fullPath, string oldName, string oldFullPath)
		{
			TryRemoveManifest(oldName);

			try
			{
				var newManifest = GetOrAddThemeManifest(name);
				if (newManifest != null)
				{
					this.AddThemeManifest(newManifest);
					Debug.WriteLine("Changed theme manifest for '{0}'".FormatCurrent(name));
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("ERR - Could not touch theme manifest '{0}': {1}".FormatCurrent(name, ex.Message));
			}

			RaiseThemeFolderRenamed(new ThemeFolderRenamedEventArgs
			{
				FullPath = fullPath,
				Name = name,
				OldFullPath = oldFullPath,
				OldName = oldName
			});
		}

		private void OnThemeFolderDeleted(string name, string fullPath)
		{
			TryRemoveManifest(name);

			RaiseThemeFolderDeleted(new ThemeFolderDeletedEventArgs
			{
				FullPath = fullPath,
				Name = name
			});
		}

		public event EventHandler<ThemeFileChangedEventArgs> ThemeFileChanged;
		protected void RaiseThemeFileChanged(ThemeFileChangedEventArgs e)
		{
			if (ThemeFileChanged != null)
				ThemeFileChanged(this, e);
		}

		public event EventHandler<ThemeFolderRenamedEventArgs> ThemeFolderRenamed;
		protected void RaiseThemeFolderRenamed(ThemeFolderRenamedEventArgs e)
		{
			if (ThemeFolderRenamed != null)
				ThemeFolderRenamed(this, e);
		}

		public event EventHandler<ThemeFolderDeletedEventArgs> ThemeFolderDeleted;
		protected void RaiseThemeFolderDeleted(ThemeFolderDeletedEventArgs e)
		{
			if (ThemeFolderDeleted != null)
				ThemeFolderDeleted(this, e);
		}

		public event EventHandler<BaseThemeChangedEventArgs> BaseThemeChanged;
		protected void RaiseBaseThemeChanged(BaseThemeChangedEventArgs e)
		{
			if (BaseThemeChanged != null)
				BaseThemeChanged(this, e);
		}

		#endregion

		#region Disposable

		protected override void OnDispose(bool disposing)
		{
			if (disposing)
			{
				if (_monitorFiles != null)
				{
					_monitorFiles.Dispose();
					_monitorFiles = null;
				}

				if (_monitorFolders != null)
				{
					_monitorFolders.Dispose();
					_monitorFolders = null;
				}
			}
		}

		#endregion

	}

}
