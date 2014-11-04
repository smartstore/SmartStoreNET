using System;
using System.Collections.Generic;

namespace SmartStore.Core.Themes
{
    public partial interface IThemeRegistry
    {
		
		/// <summary>
		/// Gets all registered theme manifests
		/// </summary>
		/// <returns>A collection of manifests</returns>
		ICollection<ThemeManifest> GetThemeManifests();
		
		/// <summary>
		/// Gets a single theme manifest by theme name
		/// </summary>
		/// <param name="themeName">The name of the theme to get a manifest for</param>
		/// <returns>A <c>ThemeManifest</c> instance or <c>null</c>, if theme is not registered</returns>
		ThemeManifest GetThemeManifest(string themeName);

		/// <summary>
		/// Gets a value indicating whether a theme is registered
		/// </summary>
		/// <param name="themeName">The theme name to check</param>
		/// <returns><c>true</c> if theme exists, <c>false</c> otherwise</returns>
        bool ThemeManifestExists(string themeName);

		/// <summary>
		/// Registers a theme manifest
		/// </summary>
		/// <param name="manifest">The theme manifest to register</param>
		/// <remarks>If an equal theme exists already, it gets removed first.</remarks>
		void AddThemeManifest(ThemeManifest manifest);

		/// <summary>
		/// Gets a value indicating whether a theme is a child of another theme
		/// </summary>
		/// <param name="themeName">The name of the theme to test</param>
		/// <param name="baseTheme">The name of the base theme</param>
		/// <returns><c>true</c> when <paramref name="themeName"/> is based on <paramref name="baseTheme"/>, <c>false</c> othwerise</returns>
		/// <remarks>
		/// This method walks up the complete hierarchy chain of <paramref name="themeName"/> to determine the result.
		/// </remarks>
		bool IsChildThemeOf(string themeName, string baseTheme);

		/// <summary>
		/// Event raised when an inheritable (static) theme file has been created or deleted,
		/// OR when the <c>theme.config</c> file has been modified.
		/// </summary>
		event EventHandler<ThemeFileChangedEventArgs> ThemeFileChanged;

		/// <summary>
		/// Event raised when a theme folder has been renamed.
		/// </summary>
		event EventHandler<ThemeFolderRenamedEventArgs> ThemeFolderRenamed;

		/// <summary>
		/// Event raised when a theme folder has been deleted.
		/// </summary>
		event EventHandler<ThemeFolderDeletedEventArgs> ThemeFolderDeleted;

		/// <summary>
		/// Event raised when a theme's base theme changes.
		/// </summary>
		event EventHandler<BaseThemeChangedEventArgs> BaseThemeChanged;
    }


	public class ThemeFileChangedEventArgs : EventArgs
	{
		public string FullPath { get; set; }
		public string ThemeName { get; set; }
		public string RelativePath { get; set; }
		public bool IsConfigurationFile { get; set; }
		public ThemeFileChangeType ChangeType { get; set; }
	}

	public class ThemeFolderRenamedEventArgs : EventArgs
	{
		public string FullPath { get; set; }
		public string Name { get; set; }
		public string OldFullPath { get; set; }
		public string OldName { get; set; }
	}

	public class ThemeFolderDeletedEventArgs : EventArgs
	{
		public string FullPath { get; set; }
		public string Name { get; set; }
	}

	public class BaseThemeChangedEventArgs : EventArgs
	{
		public string ThemeName { get; set; }
		public string BaseTheme { get; set; }
		public string OldBaseTheme { get; set; }
	}

	public enum ThemeFileChangeType
	{
		Created,
		Deleted,
		Modified
	}
}
