using SmartStore.Core.Themes;
namespace SmartStore.Web.Framework.Themes
{
    /// <summary>
    /// Work context
    /// </summary>
    public interface IThemeContext
    {
        /// <summary>
        /// Get or set current theme for desktops (e.g. Alpha)
        /// </summary>
        string WorkingDesktopTheme { get; set; }

        /// <summary>
        /// Get current theme for mobile (e.g. Mobile)
        /// </summary>
        string WorkingMobileTheme { get; }

        /// <summary>
        /// Gets or sets the manifest of the current working theme
        /// </summary>
		ThemeManifest CurrentTheme { get; set; }
    }
}
