using SmartStore.Core.Themes;
namespace SmartStore.Web.Framework.Themes
{
    /// <summary>
    /// Work context
    /// </summary>
    public interface IThemeContext
    {
        /// <summary>
        /// Get or set current theme for desktops (e.g. darkOrange)
        /// </summary>
        string WorkingDesktopTheme { get; set; }

        /// <summary>
        /// Get current theme for mobile (e.g. Mobile)
        /// </summary>
        string WorkingMobileTheme { get; }

        /// <summary>
        /// Gets the manifest of the current active theme
        /// </summary>
        ThemeManifest CurrentTheme { get; }
    }
}
