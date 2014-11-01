using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml;
using SmartStore.Collections;
using System.IO;

namespace SmartStore.Core.Themes
{
    
	public class ThemeManifest : ComparableObject<ThemeManifest>
    {

        #region Ctor

        internal ThemeManifest()
        {
        }

		public static ThemeManifest Create(string themePath, string virtualBasePath = "~/Themes")
		{
			Guard.ArgumentNotEmpty(() => themePath);
			Guard.ArgumentNotEmpty(() => virtualBasePath);

			virtualBasePath = virtualBasePath.EnsureEndsWith("/");
			var themeDirectory = new DirectoryInfo(themePath);
			var themeConfigFile = new FileInfo(System.IO.Path.Combine(themeDirectory.FullName, "theme.config"));

			if (themeConfigFile.Exists)
			{
				var doc = new XmlDocument();
				doc.Load(themeConfigFile.FullName);
				return ThemeManifest.Create(
					themeDirectory.Name,
					virtualBasePath,
					themeDirectory.FullName,
					doc);
			}

			return null;
		}

        public static ThemeManifest Create(string themeName, string virtualPath, string path, XmlDocument doc)
        {
            var materializer = new ThemeManifestMaterializer(themeName, virtualPath, path, doc);
            return materializer.Materialize();
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
        /// Gets the physical path of the theme
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

        public bool SupportRtl 
		{ 
			get; 
			protected internal set; 
		}

        public bool MobileTheme 
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
			protected internal set;
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

        public string Version 
		{ 
			get; 
			protected internal set; 
		}

        public IDictionary<string, ThemeVariableInfo> Variables 
		{ 
			get; 
			internal set; 
		}

        public Multimap<string, string> Selects { get; internal set; }

        internal string FullPath
        {
            get { return System.IO.Path.Combine(this.Path, "theme.config"); }
        }

        #endregion

	}

}
