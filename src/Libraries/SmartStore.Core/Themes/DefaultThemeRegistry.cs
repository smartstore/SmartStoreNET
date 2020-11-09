using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using SmartStore.Collections;
using SmartStore.Core.Events;
using SmartStore.Core.Logging;
using SmartStore.Utilities;

namespace SmartStore.Core.Themes
{
    public partial class DefaultThemeRegistry : DisposableObject, IThemeRegistry
    {
        private readonly bool _enableMonitoring;
        private readonly string _themesBasePath;
        private readonly IEventPublisher _eventPublisher;
        private readonly IApplicationEnvironment _env;
        private readonly ConcurrentDictionary<string, ThemeManifest> _themes = new ConcurrentDictionary<string, ThemeManifest>(StringComparer.InvariantCultureIgnoreCase);
        private readonly ConcurrentDictionary<EventThrottleKey, Timer> _eventQueue = new ConcurrentDictionary<EventThrottleKey, Timer>();

        private readonly Regex _fileFilterPattern = new Regex(@"^\.(config|png|gif|jpg|jpeg|css|scss|js|cshtml|svg|json)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private FileSystemWatcher _monitorFolders;
        private FileSystemWatcher _monitorFiles;

        public DefaultThemeRegistry(IEventPublisher eventPublisher, IApplicationEnvironment env, bool? enableMonitoring, string themesBasePath, bool autoLoadThemes)
        {
            _enableMonitoring = enableMonitoring ?? CommonHelper.GetAppSetting("sm:MonitorThemesFolder", true);
            _eventPublisher = eventPublisher;
            _env = env;
            _themesBasePath = themesBasePath.NullEmpty() ?? _env.ThemesFolder.RootPath;

            Logger = NullLogger.Instance;

            if (autoLoadThemes)
            {
                // load all themes initially
                ReloadThemes();
            }

            CreateFileSystemWatchers();

            // start FS watcher
            this.StartMonitoring(false);
        }

        public ILogger Logger { get; set; }

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

        public ICollection<ThemeManifest> GetThemeManifests(bool includeHidden = false)
        {
            var allThemes = _themes.Values;

            if (includeHidden)
            {
                return allThemes.AsReadOnly();
            }
            else
            {
                return allThemes.Where(x => x.State == ThemeManifestState.Active).AsReadOnly();
            }
        }

        public void AddThemeManifest(ThemeManifest manifest)
        {
            AddThemeManifestInternal(manifest, false);
        }

        private void AddThemeManifestInternal(ThemeManifest manifest, bool isInit)
        {
            Guard.NotNull(manifest, nameof(manifest));

            if (!isInit)
            {
                TryRemoveManifest(manifest.ThemeName);
            }

            ThemeManifest baseManifest = null;
            if (manifest.BaseThemeName != null)
            {
                if (!_themes.TryGetValue(manifest.BaseThemeName, out baseManifest))
                {
                    manifest.State = ThemeManifestState.MissingBaseTheme;
                }
            }

            manifest.BaseTheme = baseManifest;
            var added = _themes.TryAdd(manifest.ThemeName, manifest);
            if (added && !isInit)
            {
                // post process
                var children = GetChildrenOf(manifest.ThemeName, false);
                foreach (var child in children)
                {
                    child.BaseTheme = manifest;
                    child.State = ThemeManifestState.Active;
                }
            }
        }

        [SuppressMessage("ReSharper", "AssignmentInConditionalExpression")]
        private bool TryRemoveManifest(string themeName)
        {
            bool result;
            ThemeManifest existing;
            if (result = _themes.TryRemove(themeName, out existing))
            {
                _eventPublisher.Publish(new ThemeTouchedEvent(themeName));

                existing.BaseTheme = null;

                // set all direct children as broken
                var children = GetChildrenOf(themeName, false);
                foreach (var child in children)
                {
                    child.BaseTheme = null;
                    child.State = ThemeManifestState.MissingBaseTheme;
                    _eventPublisher.Publish(new ThemeTouchedEvent(child.ThemeName));
                }
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

            while (current.BaseThemeName != null)
            {
                if (baseTheme.Equals(current.BaseThemeName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (!_themes.TryGetValue(current.BaseThemeName, out current))
                {
                    return false;
                }
                //currentBaseName = current.BaseThemeName;
            }

            return false;
        }

        public IEnumerable<ThemeManifest> GetChildrenOf(string themeName, bool deep = true)
        {
            Guard.NotEmpty(themeName, nameof(themeName));

            if (!ThemeManifestExists(themeName))
                return Enumerable.Empty<ThemeManifest>();

            var derivedThemes = _themes.Values.Where(x => x.BaseThemeName != null && !x.ThemeName.IsCaseInsensitiveEqual(themeName));
            if (!deep)
            {
                derivedThemes = derivedThemes.Where(x => x.BaseThemeName.IsCaseInsensitiveEqual(themeName));
            }
            else
            {
                derivedThemes = derivedThemes.Where(x => IsChildThemeOf(x.ThemeName, themeName));
            }

            return derivedThemes;
        }

        public void ReloadThemes()
        {
            _themes.Clear();

            var folder = _env.ThemesFolder;
            var folderDatas = new List<ThemeFolderData>();
            var dirs = folder.ListDirectories("");

            // create folder (meta)datas first
            foreach (var path in dirs)
            {
                try
                {
                    var folderData = ThemeManifest.CreateThemeFolderData(folder.MapPath(path), _themesBasePath);
                    if (folderData != null)
                    {
                        folderDatas.Add(folderData);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Unable to create folder data for folder '{0}'".FormatCurrent(path));
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
                var ex = new CyclicDependencyException("Cyclic theme dependencies detected. Please check the 'baseTheme' attribute of your themes and ensure that they do not reference themselves (in)directly.");
                Logger.Error(ex);
                throw ex;
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
                        AddThemeManifestInternal(manifest, true);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Unable to create manifest for theme '{0}'".FormatCurrent(themeFolder.FolderName));
                }
            }
        }

        #endregion

        #region Monitoring & Events

        private void CreateFileSystemWatchers()
        {
            _monitorFiles = new FileSystemWatcher();
            _monitorFiles.Path = CommonHelper.MapPath(_themesBasePath);
            _monitorFiles.InternalBufferSize = 32768; // 32 instead of the default 8 KB
            _monitorFiles.Filter = "*.*";
            _monitorFiles.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite | NotifyFilters.FileName;
            _monitorFiles.IncludeSubdirectories = true;
            _monitorFiles.Changed += (s, e) => OnThemeFileChanged(e.Name, e.FullPath, ThemeFileChangeType.Modified);
            _monitorFiles.Deleted += (s, e) => OnThemeFileChanged(e.Name, e.FullPath, ThemeFileChangeType.Deleted);
            _monitorFiles.Created += (s, e) => OnThemeFileChanged(e.Name, e.FullPath, ThemeFileChangeType.Created);
            _monitorFiles.Renamed += (s, e) =>
            {
                OnThemeFileChanged(e.OldName, e.OldFullPath, ThemeFileChangeType.Deleted);
                OnThemeFileChanged(e.Name, e.FullPath, ThemeFileChangeType.Created);
            };

            _monitorFolders = new FileSystemWatcher();
            _monitorFolders.Path = CommonHelper.MapPath(_themesBasePath);
            _monitorFolders.Filter = "*";
            _monitorFolders.NotifyFilter = NotifyFilters.DirectoryName;
            _monitorFolders.IncludeSubdirectories = false;
            _monitorFolders.Renamed += (s, e) => OnThemeFolderRenamed(e.Name, e.FullPath, e.OldName, e.OldFullPath);
            _monitorFolders.Deleted += (s, e) => OnThemeFolderDeleted(e.Name, e.FullPath);
        }

        public void StartMonitoring(bool force)
        {
            var shouldStart = force || _enableMonitoring;

            if (shouldStart && !_monitorFiles.EnableRaisingEvents)
                _monitorFiles.EnableRaisingEvents = true;
            if (shouldStart && !_monitorFolders.EnableRaisingEvents)
                _monitorFolders.EnableRaisingEvents = true;
        }

        public void StopMonitoring()
        {
            if (_monitorFiles.EnableRaisingEvents)
                _monitorFiles.EnableRaisingEvents = false;
            if (_monitorFolders.EnableRaisingEvents)
                _monitorFolders.EnableRaisingEvents = false;
        }

        private bool ShouldThrottleEvent(EventThrottleKey key)
        {
            Timer timer;
            if (_eventQueue.TryGetValue(key, out timer))
            {
                // do nothing. The same event was published a tick ago.
                return true;
            }

            _eventQueue[key] = new Timer(RemoveFromEventQueue, key, 500, Timeout.Infinite);
            return false;
        }

        private void RemoveFromEventQueue(object key)
        {
            Timer timer;
            if (_eventQueue.TryRemove((EventThrottleKey)key, out timer))
            {
                timer.Dispose();
            }
        }

        private void OnThemeFileChanged(string name, string fullPath, ThemeFileChangeType changeType)
        {
            // Enable event throttling by allowing the very same event to be published only all 500 ms.
            var throttleKey = new EventThrottleKey(name, changeType);
            if (ShouldThrottleEvent(throttleKey))
            {
                return;
            }

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

                string oldBaseThemeName = null;
                var oldManifest = this.GetThemeManifest(di.Name);
                if (oldManifest != null)
                {
                    oldBaseThemeName = oldManifest.BaseThemeName;
                }

                try
                {
                    // FS watcher in conjunction with some text editors fires change events twice and locks the file.
                    // Let's wait max. 250ms till the lock is gone (hopefully).
                    var fi = new FileInfo(fullPath);
                    fi.WaitForUnlock(250);

                    var newManifest = ThemeManifest.Create(di.FullName, _themesBasePath);
                    if (newManifest != null)
                    {
                        this.AddThemeManifestInternal(newManifest, false);

                        if (!oldBaseThemeName.IsCaseInsensitiveEqual(newManifest.BaseThemeName))
                        {
                            baseThemeChangedArgs = new BaseThemeChangedEventArgs
                            {
                                ThemeName = newManifest.ThemeName,
                                BaseTheme = newManifest.BaseTheme != null ? newManifest.BaseTheme.ThemeName : null,
                                OldBaseTheme = oldBaseThemeName
                            };
                        }

                        Logger.Debug("Changed theme manifest for '{0}'".FormatCurrent(name));
                    }
                    else
                    {
                        // something went wrong (most probably no 'theme.config'): remove the manifest
                        TryRemoveManifest(di.Name);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Could not touch theme manifest '{0}': {1}".FormatCurrent(name, ex.Message));
                    TryRemoveManifest(di.Name);
                }
            }

            if (baseThemeChangedArgs != null)
            {
                RaiseBaseThemeChanged(baseThemeChangedArgs);
            }

            RaiseThemeFileChanged(new ThemeFileChangedEventArgs
            {
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
                    this.AddThemeManifestInternal(newManifest, false);
                    Logger.Debug("Changed theme manifest for '{0}'".FormatCurrent(name));
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Could not touch theme manifest '{0}'".FormatCurrent(name));
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
                    _monitorFiles.EnableRaisingEvents = false;
                    _monitorFiles.Dispose();
                    _monitorFiles = null;
                }

                if (_monitorFolders != null)
                {
                    _monitorFolders.EnableRaisingEvents = false;
                    _monitorFolders.Dispose();
                    _monitorFolders = null;
                }
            }
        }

        #endregion

        private class EventThrottleKey : Tuple<string, ThemeFileChangeType>
        {
            public EventThrottleKey(string name, ThemeFileChangeType changeType)
                : base(name, changeType)
            {
            }
        }
    }

}
