using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using NuGet;
using Log = SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.Core.Themes;

namespace SmartStore.Core.Packaging
{

	public class PackageManager : IPackageManager
	{
		private readonly IPluginFinder _pluginFinder;
		private readonly IThemeRegistry _themeRegistry;
		private readonly IPackageBuilder _packageBuilder;
		private readonly IPackageInstaller _packageInstaller;
		private readonly Log.ILogger _logger;

		public PackageManager(
			IPluginFinder pluginFinder,
			IThemeRegistry themeRegistry,
			IPackageBuilder packageBuilder,
			IPackageInstaller packageInstaller,
			Log.ILogger logger)
		{
			_pluginFinder = pluginFinder;
			_themeRegistry = themeRegistry;
			_packageBuilder = packageBuilder;
			_packageInstaller = packageInstaller;
			_logger = logger;
		}

		private PackageInfo DoInstall(Func<PackageInfo> installer)
		{
			try
			{
				return installer();
			}
			catch (SmartException)
			{
				throw;
			}
			catch (Exception exception)
			{
				var message = "There was an error installing the requested package. " +
					"This can happen if the server does not have write access to the '~/Plugins' or '~/Themes' folder of the web site. " +
					"If the site is running in shared hosted environement, adding write access to these folders sometimes needs to be done manually through the Hoster control panel. " +
					"Once Themes and Plugins have been installed, it is recommended to remove write access to these folders.";
				throw new SmartException(message, exception);
			}
		}

		public PackageInfo Install(Stream packageStream, string location, string applicationPath)
		{
			return DoInstall(() => _packageInstaller.Install(packageStream, location, applicationPath));
		}

		public void Uninstall(string packageId, string applicationPath)
		{
			_packageInstaller.Uninstall(packageId, applicationPath);
		}

		public PackagingResult BuildPluginPackage(string pluginName)
		{
			var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName(pluginName);
			if (pluginDescriptor == null)
			{
				return null;
			}
			return new PackagingResult
			{
				ExtensionType = "Plugin",
				PackageName = pluginDescriptor.FolderName,
				PackageVersion = pluginDescriptor.Version.ToString(),
				PackageStream = _packageBuilder.BuildPackage(pluginDescriptor)
			};
		}

		public PackagingResult BuildThemePackage(string themeName)
		{
			var themeManifest = _themeRegistry.GetThemeManifest(themeName);
			if (themeName == null)
			{
				return null;
			}
			return new PackagingResult
			{
				ExtensionType = "Theme",
				PackageName = themeManifest.ThemeName,
				PackageVersion = themeManifest.Version,
				PackageStream = _packageBuilder.BuildPackage(themeManifest)
			};
		}
	}

}
