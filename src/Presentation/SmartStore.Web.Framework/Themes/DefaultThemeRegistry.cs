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

namespace SmartStore.Web.Framework.Themes
{
    public partial class DefaultThemeRegistry : IThemeRegistry
    {
		#region Fields

        private readonly ReaderWriterLockSlim _rwLock; // codehint: sm-add

        private readonly IList<ThemeManifest> _themeManifests = new List<ThemeManifest>();
        private string _virtualBasePath = string.Empty;
        private string _basePath = string.Empty;

		#endregion

		#region Constructors

        public DefaultThemeRegistry()
        {
            _rwLock = new ReaderWriterLockSlim();
            
            var config = EngineContext.Current.Resolve<SmartStoreConfig>();
            var webHelper = EngineContext.Current.Resolve<IWebHelper>();

            _virtualBasePath = config.ThemeBasePath;
            _basePath = webHelper.MapPath(config.ThemeBasePath);
            LoadConfigurations();
        }

		#endregion 
        
        #region IThemeRegistry
        
        public ThemeManifest GetThemeManifest(string themeName)
        {
            return _themeManifests.SingleOrDefault(x => x.ThemeName.Equals(themeName, StringComparison.InvariantCultureIgnoreCase));
        }

        public IList<ThemeManifest> GetThemeManifests()
        {
            return _themeManifests;
        }

        public bool ThemeManifestExists(string themeName)
        {
            if (themeName.IsEmpty())
                return false;
            return GetThemeManifests().Any(configuration => configuration.ThemeName.Equals(themeName, StringComparison.InvariantCultureIgnoreCase));
        }

        #endregion

        #region Utility

        private void LoadConfigurations()
        {
            using (_rwLock.GetWriteLock())
            {
                // TODO: Use IFileStorage?
                foreach (string themeName in Directory.GetDirectories(_basePath))
                {
                    var manifest = CreateThemeManifest(themeName);
                    if (manifest != null)
                    {
                        _themeManifests.Add(manifest);
                    }
                }
            }
        }

        private ThemeManifest CreateThemeManifest(string themePath)
        {
            var themeDirectory = new DirectoryInfo(themePath);
            var themeConfigFile = new FileInfo(Path.Combine(themeDirectory.FullName, "theme.config"));

            if (themeConfigFile.Exists)
            {
                var doc = new XmlDocument();
                doc.Load(themeConfigFile.FullName);
                return ThemeManifest.Create(
                    themeDirectory.Name, 
                    _virtualBasePath, 
                    themeDirectory.FullName, 
                    doc);
            }

            return null;
        }

        #endregion
    }
}
