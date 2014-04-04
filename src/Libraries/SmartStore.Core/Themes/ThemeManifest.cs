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

        public XmlElement ConfigurationNode { get; protected internal set; }

        /// <summary>
        /// Gets the virtual theme base path (e.g.: ~/Themes)
        /// </summary>
        public string Location { get; protected internal set; }

        /// <summary>
        /// Gets the physical path of the theme
        /// </summary>
        public string Path { get; protected internal set; }

        public string PreviewImageUrl { get; protected internal set; }

        public string PreviewText { get; protected internal set; }

        public bool SupportRtl { get; protected internal set; }

        public bool MobileTheme { get; protected internal set; }

		[ObjectSignature]
        public string ThemeName { get; protected internal set; }

        public string ThemeTitle { get; protected internal set; }

        public string Author { get; protected internal set; }

        public string Version { get; protected internal set; }

        public IDictionary<string, ThemeVariableInfo> Variables { get; internal set; }

        public Multimap<string, string> Selects { get; internal set; }

        internal string FullPath
        {
            get { return System.IO.Path.Combine(this.Path, "theme.config"); }
        }

        #endregion

        #region Helper

        ////private void MaterializeVariables() 
        ////{
        ////    // TODO: (MC) Nur Temp
        ////    var vars = this.Variables;

        ////    vars.Add("BodyBackground", new ThemeVariableInfo { Name = "BodyBackground", DefaultValue = "#fff", Type = ThemeVariableType.Color });
        ////    vars.Add("TextColor", new ThemeVariableInfo { Name = "TextColor", DefaultValue = "#555", Type = ThemeVariableType.Color });
        ////    vars.Add("LinkColor", new ThemeVariableInfo { Name = "LinkColor", DefaultValue = "#08c", Type = ThemeVariableType.Color });

        ////    vars.Add("SansFontFamily", new ThemeVariableInfo { Name = "SansFontFamily", DefaultValue = "'Segoe UI', Tahoma, 'Helvetica Neue', Helvetica, Arial, 'sans-serif'", Type = ThemeVariableType.String });
        ////    vars.Add("SerifFontFamily", new ThemeVariableInfo { Name = "SerifFontFamily", DefaultValue = "Georgia, 'Times New Roman', Times, serif", Type = ThemeVariableType.String });
        ////    vars.Add("MonoFontFamily", new ThemeVariableInfo { Name = "MonoFontFamily", DefaultValue = "Menlo, Monaco, Consolas, 'Courier New', monospace", Type = ThemeVariableType.String });
        ////    vars.Add("BaseFontSize", new ThemeVariableInfo { Name = "BaseFontSize", DefaultValue = "14px", Type = ThemeVariableType.String });
        ////    vars.Add("BaseLineHeight", new ThemeVariableInfo { Name = "BaseLineHeight", DefaultValue = "20px", Type = ThemeVariableType.String });

        ////    vars.Add("HeadingsFontFamily", new ThemeVariableInfo { Name = "HeadingsFontFamily", DefaultValue = "'Segoe UI Semibold', 'Segoe UI', Tahoma, 'Helvetica Neue', Helvetica, Arial, 'sans-serif'", Type = ThemeVariableType.String });
        ////    vars.Add("HeadingsFontWeight", new ThemeVariableInfo { Name = "HeadingsFontWeight", DefaultValue = "600", Type = ThemeVariableType.String }); // Select?
        ////    vars.Add("HeadingsColor", new ThemeVariableInfo { Name = "HeadingsColor", DefaultValue = "inherit", Type = ThemeVariableType.Color }); // String?

        ////    vars.Add("TableBackground", new ThemeVariableInfo { Name = "TableBackground", DefaultValue = "transparent", Type = ThemeVariableType.Color });
        ////    vars.Add("TableBackgroundAccent", new ThemeVariableInfo { Name = "TableBackgroundAccent", DefaultValue = "#f9f9f9", Type = ThemeVariableType.Color });
        ////    vars.Add("TableBackgroundHover", new ThemeVariableInfo { Name = "TableBackgroundHover", DefaultValue = "#f5f5f5", Type = ThemeVariableType.Color });
        ////    vars.Add("TableBorder", new ThemeVariableInfo { Name = "TableBorder", DefaultValue = "#ddd", Type = ThemeVariableType.Color });

        ////    vars.Add("BtnBackground", new ThemeVariableInfo { Name = "BtnBackground", DefaultValue = "#fff", Type = ThemeVariableType.Color });

        ////}

        #endregion

    }
}
