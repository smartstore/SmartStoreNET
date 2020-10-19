using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using SmartStore.Core;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Themes;
using SmartStore.Utilities.Threading;

namespace SmartStore.Web.Framework.Theming
{
    public class DefaultThemeFileResolver : DisposableObject, IThemeFileResolver
    {
        private readonly Dictionary<FileKey, InheritedThemeFileResult> _files = new Dictionary<FileKey, InheritedThemeFileResult>();
        private readonly IThemeRegistry _themeRegistry;
        private readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();

        public DefaultThemeFileResolver(IThemeRegistry themeRegistry)
        {
            _themeRegistry = themeRegistry;

            // listen to file monitoring events
            _themeRegistry.ThemeFolderDeleted += OnThemeFolderDeleted;
            _themeRegistry.ThemeFolderRenamed += OnThemeFolderRenamed;
            _themeRegistry.BaseThemeChanged += OnBaseThemeChanged;
            _themeRegistry.ThemeFileChanged += OnThemeFileChanged;
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
            using (_rwLock.GetWriteLock())
            {
                var keys = _files.Keys.Where(x => x.ThemeName.IsCaseInsensitiveEqual(themeName)).ToList();
                foreach (var key in keys)
                {
                    if (key.ThemeName.IsCaseInsensitiveEqual(themeName) || _themeRegistry.IsChildThemeOf(key.ThemeName, themeName))
                    {
                        // remove all cached pathes for this theme (also in all derived themes)
                        _files.Remove(key);
                    }
                }
            }
        }

        private void OnThemeFileChanged(object sender, ThemeFileChangedEventArgs e)
        {
            if (e.IsConfigurationFile)
                return;

            using (_rwLock.GetWriteLock())
            {
                // get keys with same relative path
                var keys = _files.Keys.Where(x => x.RelativePath.IsCaseInsensitiveEqual(e.RelativePath)).ToList();
                foreach (var key in keys)
                {
                    if (key.ThemeName.IsCaseInsensitiveEqual(e.ThemeName) || _themeRegistry.IsChildThemeOf(key.ThemeName, e.ThemeName))
                    {
                        // remove all cached pathes for this file/theme combination (also in all derived themes)
                        if (_files.TryGetValue(key, out var result))
                        {
                            _files.Remove(key);
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
        }

        protected override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                _themeRegistry.ThemeFileChanged -= OnThemeFileChanged;
                _themeRegistry.ThemeFolderDeleted -= OnThemeFolderDeleted;
                _themeRegistry.BaseThemeChanged -= OnBaseThemeChanged;
                _themeRegistry.ThemeFolderRenamed -= OnThemeFolderRenamed;
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
            Guard.NotEmpty(virtualPath, nameof(virtualPath));

            if (virtualPath[0] != '~')
            {
                virtualPath = VirtualPathUtility.ToAppRelative(virtualPath);
            }

            if (!ThemeHelper.PathIsInheritableThemeFile(virtualPath))
            {
                return null;
            }

            bool isBased = false;

            virtualPath = ThemeHelper.TokenizePath(virtualPath, out var requestedThemeName, out var relativePath, out var query);

            Func<InheritedThemeFileResult> nullOrFile = () =>
            {
                return isBased
                    ? new InheritedThemeFileResult { IsBased = true, OriginalVirtualPath = virtualPath, Query = query }
                    : null;
            };

            ThemeManifest currentTheme = ResolveTheme(requestedThemeName, relativePath, query, out isBased);

            if (currentTheme?.BaseTheme == null)
            {
                // dont't bother resolving files: the current theme is not inherited.
                // Let the current VPP do the work.
                return nullOrFile();
            }

            if (!currentTheme.ThemeName.Equals(requestedThemeName, StringComparison.OrdinalIgnoreCase))
            {
                if (!_themeRegistry.IsChildThemeOf(currentTheme.ThemeName, requestedThemeName))
                {
                    return nullOrFile();
                }
            }
            else if (isBased && currentTheme.BaseTheme != null)
            {
                // A file from the base theme has been requested
                currentTheme = currentTheme.BaseTheme;
            }

            var fileKey = new FileKey(currentTheme.ThemeName, relativePath, query);
            InheritedThemeFileResult result;

            using (_rwLock.GetUpgradeableReadLock())
            {
                if (!_files.TryGetValue(fileKey, out result))
                {
                    using (_rwLock.GetWriteLock())
                    {
                        // ALWAYS begin the search with the current working theme's location!
                        string actualLocation = LocateFile(currentTheme.ThemeName, relativePath, out var resultVirtualPath, out var resultPhysicalPath);

                        if (actualLocation != null)
                        {
                            result = new InheritedThemeFileResult
                            {
                                RelativePath = relativePath,
                                OriginalVirtualPath = virtualPath,
                                ResultVirtualPath = resultVirtualPath,
                                ResultPhysicalPath = resultPhysicalPath,
                                OriginalThemeName = requestedThemeName,
                                ResultThemeName = actualLocation,
                                IsBased = isBased,
                                Query = query
                            };
                        }

                        _files[fileKey] = result;
                    }
                }
            }

            if (result == null)
                return nullOrFile();

            return result;
        }

        private ThemeManifest ResolveTheme(string requestedThemeName, string relativePath, string query, out bool isBased)
        {
            isBased = false;
            ThemeManifest currentTheme;

            var isAdmin = EngineContext.Current.Resolve<IWorkContext>().IsAdmin;
            if (isAdmin)
            {
                currentTheme = _themeRegistry.GetThemeManifest(requestedThemeName);
            }
            else
            {
                var styleResult = ThemeHelper.IsStyleSheet(relativePath);
                var isPreprocessor = styleResult != null && styleResult.IsPreprocessor;
                if (isPreprocessor)
                {
                    // special consideration for SASS/LESS files: they can be validated
                    // in the backend. For validation, a "theme" query is appended 
                    // to the url. During validation we must work with the actual
                    // requested theme instead of dynamically resolving the working theme.
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

                if (isPreprocessor && query != null && query.StartsWith("base", StringComparison.OrdinalIgnoreCase))
                {
                    // special case to support SASS @import declarations
                    // within inherited SASS files. Snenario: an inheritor wishes to
                    // include the same file from it's base theme (e.g. custom.scss) just to tweak it
                    // a bit for his child theme. Without the 'base' query the resolution starting point
                    // for custom.scss would be the CURRENT theme's folder, and NOT the requested one's,
                    // which inevitably would result in a cyclic dependency.
                    currentTheme = _themeRegistry.GetThemeManifest(requestedThemeName);
                    isBased = true;
                }
            }

            return currentTheme;
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


        private class FileKey : Tuple<string, string, string>
        {
            public FileKey(string themeName, string relativePath, string query)
                : base(themeName.ToLower(), relativePath.ToLower(), query?.ToLower())
            {
            }

            public string ThemeName => base.Item1;

            public string RelativePath => base.Item2;

            public string Query => base.Item3;
        }

    }
}
