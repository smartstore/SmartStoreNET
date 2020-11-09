using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using SmartStore.Collections;

namespace SmartStore.Core.Themes
{
    public class ThemeManifest : ComparableObject<ThemeManifest>
    {
        internal ThemeManifest()
        {
        }

        #region Methods

        public static ThemeManifest Create(string themePath, string virtualBasePath = "~/Themes/")
        {
            Guard.NotEmpty(themePath, nameof(themePath));

            var folderData = CreateThemeFolderData(themePath, virtualBasePath);
            if (folderData != null)
            {
                return Create(folderData);
            }

            return null;
        }

        internal static ThemeManifest Create(ThemeFolderData folderData)
        {
            Guard.NotNull(folderData, nameof(folderData));

            var materializer = new ThemeManifestMaterializer(folderData);
            var manifest = materializer.Materialize();

            return manifest;
        }

        internal static ThemeFolderData CreateThemeFolderData(string themePath, string virtualBasePath = "~/Themes/")
        {
            if (themePath.IsEmpty())
                return null;

            virtualBasePath = virtualBasePath.EnsureEndsWith("/");

            var themeDirectory = new DirectoryInfo(themePath);

            var isSymLink = themeDirectory.IsSymbolicLink(out var finalPathName);
            if (isSymLink)
            {
                themeDirectory = new DirectoryInfo(finalPathName);
            }

            var themeConfigFile = new FileInfo(System.IO.Path.Combine(themeDirectory.FullName, "theme.config"));

            if (themeConfigFile.Exists)
            {
                var doc = new XmlDocument();
                doc.Load(themeConfigFile.FullName);

                Guard.Against<SmartException>(doc.DocumentElement == null, "The theme configuration document must have a root element.");

                var root = doc.DocumentElement;

                var baseTheme = root.GetAttribute("baseTheme").TrimSafe().NullEmpty();
                if (baseTheme != null && baseTheme.IsCaseInsensitiveEqual(themeDirectory.Name))
                {
                    // Don't let theme point to itself!
                    baseTheme = null;
                }

                return new ThemeFolderData
                {
                    FolderName = themeDirectory.Name,
                    FullPath = themeDirectory.FullName,
                    IsSymbolicLink = isSymLink,
                    Configuration = doc,
                    VirtualBasePath = virtualBasePath,
                    BaseTheme = baseTheme
                };
            }

            return null;
        }

        #endregion

        #region Properties

        public XmlElement ConfigurationNode
        {
            get;
            protected internal set;
        }

        /// <summary>
        /// Gets the virtual theme base path (e.g.: ~/Themes)
        /// </summary>
        public string Location
        {
            get;
            protected internal set;
        }

        /// <summary>
        /// Determines whether the theme directory is a symbolic link to another target.
        /// </summary>
        public bool IsSymbolicLink
        {
            get;
            protected internal set;
        }

        /// <summary>
        /// Gets the physical path of the theme. In case of a symbolic link,
        /// returns the link's target path.
        /// </summary>
        public string Path
        {
            get;
            protected internal set;
        }

        public string PreviewImageUrl
        {
            get;
            protected internal set;
        }

        public string PreviewText
        {
            get;
            protected internal set;
        }

        [ObjectSignature]
        public string ThemeName
        {
            get;
            protected internal set;
        }

        public string BaseThemeName
        {
            get;
            internal set;
        }

        public ThemeManifest BaseTheme
        {
            get;
            internal set;
        }

        public string ThemeTitle
        {
            get;
            protected internal set;
        }

        public string Author
        {
            get;
            protected internal set;
        }

        public string Url
        {
            get;
            protected internal set;
        }

        public string Version
        {
            get;
            protected internal set;
        }

        private IDictionary<string, ThemeVariableInfo> _variables;
        public IDictionary<string, ThemeVariableInfo> Variables
        {
            get
            {
                if (this.BaseTheme == null)
                {
                    return _variables;
                }

                var baseVars = this.BaseTheme.Variables;
                var mergedVars = new Dictionary<string, ThemeVariableInfo>(baseVars, StringComparer.OrdinalIgnoreCase);
                var newVars = new List<ThemeVariableInfo>();

                foreach (var localVar in _variables)
                {
                    if (mergedVars.ContainsKey(localVar.Key))
                    {
                        // Overridden var in child: update existing.
                        var baseVar = mergedVars[localVar.Key];
                        mergedVars[localVar.Key] = new ThemeVariableInfo
                        {
                            Name = baseVar.Name,
                            Type = baseVar.Type,
                            SelectRef = baseVar.SelectRef,
                            DefaultValue = localVar.Value.DefaultValue,
                            Manifest = localVar.Value.Manifest
                        };
                    }
                    else
                    {
                        // New var in child: add to temp list.
                        newVars.Add(localVar.Value);
                    }
                }

                var merged = new Dictionary<string, ThemeVariableInfo>(StringComparer.OrdinalIgnoreCase);

                foreach (var newVar in newVars)
                {
                    // New child theme vars must come first in final list
                    // to avoid wrong references in existing vars
                    merged.Add(newVar.Name, newVar);
                }

                foreach (var kvp in mergedVars)
                {
                    merged.Add(kvp.Key, kvp.Value);
                }

                return merged;
            }
            internal set => _variables = value;
        }

        private Multimap<string, string> _selects;
        public Multimap<string, string> Selects
        {
            get
            {
                if (this.BaseTheme == null)
                {
                    return _selects;
                }

                var baseSelects = this.BaseTheme.Selects;
                var merged = new Multimap<string, string>();
                baseSelects.Each(x => merged.AddRange(x.Key, x.Value));
                foreach (var localSelect in _selects)
                {
                    if (!merged.ContainsKey(localSelect.Key))
                    {
                        // New Select in child: add to list.
                        merged.AddRange(localSelect.Key, localSelect.Value);
                    }
                    else
                    {
                        // Do nothing: we don't support overriding Selects
                    }
                }

                return merged;
            }
            internal set => _selects = value;
        }

        private ThemeManifestState _state;
        public ThemeManifestState State
        {
            get
            {
                if (_state == ThemeManifestState.Active)
                {
                    // active state does not mean, that it actually IS active: check state of base themes!
                    var baseTheme = this.BaseTheme;
                    while (baseTheme != null)
                    {
                        if (baseTheme.State != ThemeManifestState.Active)
                        {
                            return baseTheme.State;
                        }
                        baseTheme = baseTheme.BaseTheme;
                    }
                }

                return _state;
            }
            protected internal set => _state = value;
        }

        internal string FullPath => System.IO.Path.Combine(this.Path, "theme.config");

        public override string ToString()
        {
            return "{0} (Parent: {1}, State: {2})".FormatInvariant(ThemeName, BaseThemeName ?? "-", State.ToString());
        }

        #endregion

        #region Dispose

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                BaseTheme = null;
                if (_variables != null)
                {
                    foreach (var pair in _variables)
                    {
                        pair.Value.Dispose();
                    }
                    _variables.Clear();
                }
            }
        }

        ~ThemeManifest()
        {
            Dispose(false);
        }

        #endregion
    }

    public enum ThemeManifestState
    {
        MissingBaseTheme = -1,
        Active = 0,
    }
}
