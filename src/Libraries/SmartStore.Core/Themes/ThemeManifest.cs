using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml;
using SmartStore.Collections;
using System.IO;
using SmartStore.Utilities;
using System.Web;

namespace SmartStore.Core.Themes
{

	internal class ThemeFolderData
	{
		public string FolderName { get; set; }
		public string FullPath { get; set; }
		public string VirtualBasePath { get; set; }
		public XmlDocument Configuration { get; set; }
		public ThemeManifest BaseTheme { get; set; }
	}

	public class ThemeManifest : ComparableObject<ThemeManifest>
    {

        #region Ctor

        internal ThemeManifest()
        {
        }

		internal static ThemeFolderData CreateThemeFolderDataByName(string themeName, Func<string, ThemeManifest> baseThemeResolver, string virtualBasePath = "~/Themes")
		{
			if (themeName.IsEmpty())
				return null;

			virtualBasePath = virtualBasePath.EnsureEndsWith("/");
			var themePath = CommonHelper.MapPath(VirtualPathUtility.Combine(virtualBasePath, themeName));

			if (!Directory.Exists(themePath))
				return null;

			return CreateThemeFolderData(themePath, baseThemeResolver, virtualBasePath);
		}

		internal static ThemeFolderData CreateThemeFolderData(string themePath, Func<string, ThemeManifest> baseThemeResolver, string virtualBasePath = "~/Themes")
		{
			if (themePath.IsEmpty() || virtualBasePath.IsEmpty())
				return null;

			virtualBasePath = virtualBasePath.EnsureEndsWith("/");
			var themeDirectory = new DirectoryInfo(themePath);
			var themeConfigFile = new FileInfo(System.IO.Path.Combine(themeDirectory.FullName, "theme.config"));

			if (themeConfigFile.Exists)
			{
				var doc = new XmlDocument();
				doc.Load(themeConfigFile.FullName);

				Guard.Against<SmartException>(doc.DocumentElement == null, "The theme configuration document must have a root element.");

				var root = doc.DocumentElement;

				ThemeManifest baseTheme = null;
				var baseThemeName = root.GetAttribute("baseTheme").TrimSafe().NullEmpty();
				if (baseThemeName != null && baseThemeName.IsCaseInsensitiveEqual(themeDirectory.Name))
				{
					// Don't let theme point to itself!
					baseThemeName = null;
				}

				if (baseThemeName.HasValue())
				{
					if (baseThemeResolver == null) 
					{
						throw Error.Argument("baseThemeResolver", "Argument 'baseThemeResolver' cannot be null if theme is a child theme (path of affected theme: {0})", themePath);
					}

					baseTheme = baseThemeResolver(baseThemeName);
					if (baseTheme == null)
					{
						throw Error.Application("The base theme '{0}' for theme '{1}' does not exist or could not be created.", baseThemeName, themeDirectory.Name);
					}
				}

				return new ThemeFolderData 
				{ 
					FolderName = themeDirectory.Name,
					FullPath = themeDirectory.FullName,
					Configuration = doc,
					VirtualBasePath = virtualBasePath,
					BaseTheme = baseTheme
				};
			}

			return null;
		}

		public static ThemeManifest Create(string themePath, Func<string, ThemeManifest> baseThemeResolver, string virtualBasePath = "~/Themes")
		{
			Guard.ArgumentNotEmpty(() => themePath);
			Guard.ArgumentNotEmpty(() => virtualBasePath);

			var folderData = CreateThemeFolderData(themePath, baseThemeResolver, virtualBasePath);
			if (folderData != null)
			{
				return Create(folderData);
			}

			return null;
		}

        internal static ThemeManifest Create(ThemeFolderData folderData)
        {
			var materializer = new ThemeManifestMaterializer(folderData);
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
