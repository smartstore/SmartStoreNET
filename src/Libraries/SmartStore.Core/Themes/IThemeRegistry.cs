using System.Collections.Generic;

namespace SmartStore.Core.Themes
{
    public partial interface IThemeRegistry
    {
        ThemeManifest GetThemeManifest(string themeName);

        IList<ThemeManifest> GetThemeManifests();

        bool ThemeManifestExists(string themeName);
    }
}
