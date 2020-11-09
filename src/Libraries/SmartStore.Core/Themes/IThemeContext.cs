
namespace SmartStore.Core.Themes
{
    /// <summary>
    /// Theme context
    /// </summary>
    public interface IThemeContext
    {
        /// <summary>
        /// Gets or sets the name of the current theme (e.g. Flex)
        /// </summary>
        string WorkingThemeName { get; set; }

        /// <summary>
        /// Sets a theme override to be used for the current request
        /// </summary>
        /// <param name="theme">The theme override or <c>null</c> to remove the override</param>
        void SetRequestTheme(string theme);

        /// <summary>
        /// Sets a theme override to be used for the current user's session (e.g. for preview mode)
        /// </summary>
        /// <param name="theme">The theme override or <c>null</c> to remove the override</param>
        void SetPreviewTheme(string theme);

        /// <summary>
        /// Gets the theme override for the current request
        /// </summary>
        /// <returns>The theme override or <c>null</c></returns>
        string GetRequestTheme();

        /// <summary>
        /// Gets the theme override for the current session
        /// </summary>
        /// <returns>The theme override or <c>null</c></returns>
        string GetPreviewTheme();

        /// <summary>
        /// Gets or sets the manifest of the current working theme
        /// </summary>
		ThemeManifest CurrentTheme { get; }
    }
}
