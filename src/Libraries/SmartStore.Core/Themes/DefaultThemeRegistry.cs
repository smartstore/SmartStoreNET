using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using SmartStore.Collections;
using SmartStore.Core.Events;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.IO.WebSite;
using SmartStore.Utilities;

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

		public DefaultThemeRegistry(IEventPublisher eventPublisher, bool? enableMonitoring, string themesBasePath, bool autoLoadThemes)
        {
			this._enableMonitoring = enableMonitoring ?? CommonHelper.GetAppSetting<bool>("sm:MonitorThemesFolder", true);
			this._themesBasePath = themesBasePath.NullEmpty() ?? CommonHelper.GetAppSetting<string>("sm:ThemesBasePath", "~/Themes/").EnsureEndsWith("/");
			this._eventPublisher = eventPublisher;

			if (autoLoadThemes)
			{
				// load all themes initially
				LoadThemes();
			}

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

			ThemeManifest manifest;
			if (_themes.TryGetValue(themeName, out manifest))
			{
				return manifest.State == ThemeManifestState.Active;
			}

			return false;
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

		public ICollection<ThemeManifest> GetThemeManifests()
		{
			return _themes.Values.AsReadOnly();
		}

		public void AddThemeManifest(ThemeManifest manifest)
		{
			Guard.ArgumentNotNull(() => manifest);

			TryRemoveManifest(manifest.ThemeName);
			
			ThemeManifest baseManifest = null;
			if (manifest.BaseThemeName != null)
			{
				if (!_themes.TryGetValue(manifest.BaseThemeName, out baseManifest))
				{
					throw new SmartException("Theme '{0}' is derived from '{1}', which does not exist. Please deploy theme '{1}' first.".FormatCurrent(manifest.ThemeName, manifest.BaseThemeName));
				}
			}

			manifest.BaseTheme = baseManifest;
			_themes.TryAdd(manifest.ThemeName, manifest);
		}

		private bool TryRemoveManifest(string themeName)
		{
			bool result;
			ThemeManifest existing;
			if (result = _themes.TryRemove(themeName, out existing))
			{
				_eventPublisher.Publish(new ThemeTouchedEvent(themeName));

				existing.BaseTheme = null;

				//// remove all derived themes also
				//var children = GetChildrenOf(themeName, true);
				//foreach (var child in children)
				//{
				//	child.BaseTheme = null;
				//	_eventPublisher.Publish(new ThemeTouchedEvent(child.ThemeName));
				//}
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

		public IEnumerable<ThemeManifest> GetChildrenOf(string themeName, bool deep = true)
		{
			Guard.ArgumentNotEmpty(() => themeName);

			if (!ThemeManifestExists(themeName))
				Enumerable.Empty<ThemeManifest>();

			var derivedThemes = _themes.Values.Where(x => x.BaseTheme != null && !x.ThemeName.IsCaseInsensitiveEqual(themeName));
			if (!deep)
			{
				derivedThemes = derivedThemes.Where(x => x.BaseTheme.ThemeName.IsCaseInsensitiveEqual(themeName));
			}
			else
			{
				derivedThemes = derivedThemes.Where(x => IsChildThemeOf(x.ThemeName, themeName));
			}

			return derivedThemes;
		}

		private void LoadThemes()
		{
			var folder = EngineContext.Current.Resolve<IWebSiteFolder>();
			var folderDatas = new List<ThemeFolderData>();
			var dirs = folder.ListDirectories(_themesBasePath);

			// create folder (meta)datas first
			foreach (var path in dirs)
			{
				try
				{
					var folderData = ThemeManifest.CreateThemeFolderData(CommonHelper.MapPath(path), _themesBasePath);
					if (folderData != null)
					{
						folderDatas.Add(folderData);
					}
				}
				catch (Exception ex)
				{
					Debug.WriteLine("ERR - unable to create folder data for folder '{0}': {1}".FormatCurrent(path, ex.Message));
				}
			}

			// perform topological sort (BaseThemes first...)
			IEnumerable<ThemeFolderData> sortedThemeFolders;
			try
			{
				sortedThemeFolders = folderDatas.ToArray().SortTopological(StringComparer.OrdinalIgnoreCase).Cast<ThemeFolderData>();
			}
			catch (CyclicDependencyException)
			{
				throw new CyclicDependencyException("Cyclic theme dependencies detected. Please check the 'baseTheme' attribute of your themes and ensure that they do not reference themselves (in)directly.");
			}
			catch
			{
				throw;
			}

			// create theme manifests
			foreach (var themeFolder in sortedThemeFolders)
			{
				try
				{
					var manifest = ThemeManifest.Create(themeFolder);
					if (manifest != null)
					{
						//_themes.TryAdd(manifest.ThemeName, manifest);
						AddThemeManifest(manifest);
					}
				}
				catch (Exception ex)
				{
					Debug.WriteLine("ERR - unable to create manifest for theme '{0}': {1}".FormatCurrent(themeFolder.FolderName, ex.Message));
				}
			}
		}

		//private bool ValidateThemeInheritance(ThemeManifest manifest, IDictionary<string, ThemeManifest> map)
		//{
		//	return true;
		//	//var stack = new List<string>();

		//	//while (manifest.BaseThemeName != null)
		//	//{
		//	//	stack.Add(manifest.ThemeName);
				
		//	//	if (!map.ContainsKey(manifest.BaseThemeName))
		//	//	{
		//	//		Debug.WriteLine("The base theme does not exist");
		//	//		return false;
		//	//	}

		//	//	if (stack.Contains(manifest.BaseThemeName, StringComparer.OrdinalIgnoreCase))
		//	//	{
		//	//		Debug.WriteLine("Circular reference");
		//	//		return false;
		//	//	}

		//	//	manifest = map[manifest.BaseThemeName];
		//	//}

		//	//return true;
		//}

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
					if ((new FileInfo(fullPath)).IsFileLocked())
					{
						return;
					}

					var newManifest = ThemeManifest.Create(di.FullName, _themesBasePath);
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
				var newManifest = GetThemeManifest(name);
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
