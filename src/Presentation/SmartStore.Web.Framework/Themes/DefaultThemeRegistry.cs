using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using SmartStore.Core;
using SmartStore.Core.Configuration;
using SmartStore.Core.Infrastructure;
using SmartStore.Utilities.Threading;
using SmartStore.Core.Themes;
using SmartStore.Utilities;
using SmartStore.Core.Caching;
using SmartStore.Core.IO.WebSite;
using SmartStore.Core.Events;
using SmartStore.Web.Framework.Events;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace SmartStore.Web.Framework.Themes
{
    public partial class DefaultThemeRegistry : DisposableObject, IThemeRegistry
    {
		#region Fields

		internal const string THEME_MANIFESTS_ALL_KEY = "sm.theme-manifests.all";

		private readonly SmartStoreConfig _cfg;
		private readonly IEventPublisher _eventPublisher;
		private readonly ConcurrentDictionary<string, ThemeManifest> _themes = new ConcurrentDictionary<string, ThemeManifest>(StringComparer.InvariantCultureIgnoreCase);

		private FileSystemWatcher _watcherCfg;
		private FileSystemWatcher _watcherFolders;

		#endregion

		#region Constructors

		public DefaultThemeRegistry(
			SmartStoreConfig cfg,
			IEventPublisher eventPublisher)
        {
			this._cfg = cfg;
			this._eventPublisher = eventPublisher;

			// load all themes initially
			LoadThemes();

			// start FS watcher
			WatchConfigFiles();
			WatchFolders();
        }

		#endregion 
        
		#region Watcher

		private void WatchConfigFiles()
		{
			_watcherCfg = new FileSystemWatcher();

			_watcherCfg.Path = CommonHelper.MapPath(_cfg.ThemeBasePath);
			_watcherCfg.Filter = "theme.config";
			_watcherCfg.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
			_watcherCfg.IncludeSubdirectories = true;
			_watcherCfg.EnableRaisingEvents = true;

			_watcherCfg.Changed += (s, e) => ThemeConfigChanged(e.Name, e.FullPath);
			_watcherCfg.Deleted += (s, e) => ThemeConfigChanged(e.Name, e.FullPath);
			_watcherCfg.Created += (s, e) => ThemeConfigChanged(e.Name, e.FullPath);
			_watcherCfg.Renamed += (s, e) => ThemeConfigChanged(e.Name, e.FullPath);
		}

		private void WatchFolders()
		{
			_watcherFolders = new FileSystemWatcher();

			_watcherFolders.Path = CommonHelper.MapPath(_cfg.ThemeBasePath);
			_watcherFolders.Filter = "*";
			_watcherFolders.NotifyFilter = NotifyFilters.DirectoryName;
			_watcherFolders.IncludeSubdirectories = false;
			_watcherFolders.EnableRaisingEvents = true;

			_watcherFolders.Renamed += (s, e) => ThemeFolderRenamed(e.Name, e.FullPath, e.OldName, e.OldFullPath);
			_watcherFolders.Deleted += (s, e) => TryRemoveManifest(e.Name);
		}

		private void ThemeConfigChanged(string name, string fullPath)
		{
			var di = new DirectoryInfo(Path.GetDirectoryName(fullPath));
			try
			{
				var newManifest = ThemeManifest.Create(di.FullName);
				if (newManifest != null)
				{
					this.AddThemeManifest(newManifest);
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

		private void ThemeFolderRenamed(string name, string fullPath, string oldName, string oldFullPath)
		{
			TryRemoveManifest(oldName);
			
			var di = new DirectoryInfo(fullPath);
			try
			{
				var newManifest = ThemeManifest.Create(di.FullName);
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
			if (_themes.TryGetValue(themeName, out value))
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
			_themes.TryAdd(manifest.ThemeName, manifest);
		}

		private bool TryRemoveManifest(string themeName)
		{
			bool result;
			ThemeManifest existing;
			if (result = _themes.TryRemove(themeName, out existing))
			{
				_eventPublisher.Publish(new ThemeTouched(themeName));
			}
			return result;
		}

		private void LoadThemes()
		{
			var folder = EngineContext.Current.Resolve<IWebSiteFolder>();
			var virtualBasePath = _cfg.ThemeBasePath;
			foreach (var path in folder.ListDirectories(virtualBasePath))
			{
				try
				{
					var manifest = ThemeManifest.Create(CommonHelper.MapPath(path), virtualBasePath);
					if (manifest != null)
					{
						_themes.TryAdd(manifest.ThemeName, manifest);
					}
				}
				catch (Exception ex)
				{
					Debug.WriteLine("ERR - unable to create manifest for theme '{0}': {1}".FormatCurrent(path, ex.Message));
				}
			}
		}

        #endregion


		protected override void OnDispose(bool disposing)
		{
			if (disposing)
			{
				_watcherCfg.Dispose();
				_watcherFolders.Dispose();

				_watcherCfg = null;
				_watcherFolders = null;
			}
		}
	}

}
