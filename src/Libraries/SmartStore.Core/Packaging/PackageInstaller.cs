using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NuGet;
using NuGetPackageManager = NuGet.PackageManager;
using SmartStore.Core.Logging;
using Log = SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.Core.Themes;
using SmartStore.Core.IO.VirtualPath;
using SmartStore.Core;

namespace SmartStore.Core.Packaging
{

	public class PackageInstaller : IPackageInstaller
	{
		private readonly IVirtualPathProvider _virtualPathProvider;
		private readonly IPluginFinder _pluginFinder;
		private readonly IThemeRegistry _themeRegistry;
		private readonly IFolderUpdater _folderUpdater;
		private readonly INotifier _notifier;
		private readonly Log.ILogger _logger;

		public PackageInstaller(
			IVirtualPathProvider virtualPathProvider,
			IPluginFinder pluginFinder,
			IThemeRegistry themeRegistry,
			IFolderUpdater folderUpdater,
			INotifier notifier,
			Log.ILogger logger)
		{
			_virtualPathProvider = virtualPathProvider;
			_pluginFinder = pluginFinder;
			_themeRegistry = themeRegistry;
			_folderUpdater = folderUpdater;
			_notifier = notifier;
			_logger = logger;
		}

		//public PackageInfo Install(string packageId, string version, string location, string applicationFolder)
		//{
		//	// instantiates the appropriate package repository
		//	IPackageRepository packageRepository = PackageRepositoryFactory.Default.CreateRepository(location);

		//	// gets an IPackage instance from the repository
		//	var packageVersion = String.IsNullOrEmpty(version) ? null : new SemanticVersion(version);
		//	var package = packageRepository.FindPackage(packageId, packageVersion);
		//	if (package == null)
		//	{
		//		throw new ArgumentException("The specified package could not be found, id:{0} version:{1}".FormatCurrent(packageId, version.IsEmpty() ? "No version" : version));
		//	}

		//	return InstallPackage(package, packageRepository, location, applicationFolder);
		//}

		public PackageInfo Install(Stream packageStream, string location, string applicationPath)
		{
			Guard.ArgumentNotNull(() => packageStream);
			
			IPackage package = null;
			try
			{
				package = new ZipPackage(packageStream);

			}
			catch (Exception ex)
			{
				throw new SmartException("Package could not be created from stream.", ex);
			}
			
			// instantiates the appropriate package repository
			var packageRepository = new NullSourceRepository();
			return InstallPackage(package, packageRepository, location, applicationPath);
		}

		//public PackageInfo Install(IPackage package, string location, string applicationPath)
		//{
		//	// instantiates the appropriate package repository
		//	var packageRepository = new NullSourceRepository();
		//	return InstallPackage(package, packageRepository, location, applicationPath);
		//}

		protected PackageInfo InstallPackage(IPackage package, IPackageRepository packageRepository, string location, string applicationPath)
		{
			// TODO: (pkg) Localization

			bool previousInstalled;

			// 1. See if extension was previous installed and backup its folder if so
			try
			{
				previousInstalled = BackupExtensionFolder(package.ExtensionFolder(), package.ExtensionId());
			}
			catch (Exception exception)
			{
				throw new SmartException("Unable to backup existing local package directory.", exception);
			}

			if (previousInstalled)
			{
				// 2. If extension is installed, need to un-install first
				try
				{
					UninstallExtensionIfNeeded(package);
				}
				catch (Exception exception)
				{
					throw new SmartException("Unable to un-install local package before updating.", exception);
				}
			}

			var packageInfo = ExecuteInstall(package, packageRepository, location, applicationPath);

			// check if the new package is compatible with current SmartStore version
			var descriptor = package.GetExtensionDescriptor(packageInfo.Type);

			if (descriptor != null)
			{
				if (!PluginManager.IsAssumedCompatible(descriptor.MinAppVersion))
				{
					if (previousInstalled)
					{
						// restore the previous version
						RestoreExtensionFolder(package.ExtensionFolder(), package.ExtensionId());
					}
					else
					{
						// just uninstall the new package
						Uninstall(package.Id, _virtualPathProvider.MapPath("~\\"));
					}

					var msg = "The package is not compatible the current app version {0}. Please update SmartStore.NET or install another version of this package.".FormatInvariant(SmartStoreVersion.CurrentFullVersion);
					_logger.Error(msg);
					throw new SmartException(msg);
				}
			}

			return packageInfo;
		}

		/// <summary>
		/// Executes a package installation.
		/// </summary>
		/// <param name="package">The package to install.</param>
		/// <param name="packageRepository">The repository for the package.</param>
		/// <param name="sourceLocation">The source location.</param>
		/// <param name="targetPath">The path where to install the package.</param>
		/// <returns>The package information.</returns>
		protected PackageInfo ExecuteInstall(IPackage package, IPackageRepository packageRepository, string sourceLocation, string targetPath)
		{
			// this logger is used to render NuGet's log on the notifier
			var logger = new NugetLogger(_logger);

			var project = new FileBasedProjectSystem(targetPath) { Logger = logger };

			IPackageRepository referenceRepository;
			if (package.IsTheme())
			{
				referenceRepository = new ThemeReferenceRepository(project, packageRepository, _themeRegistry);
			}
			else
			{
				referenceRepository = new PluginReferenceRepository(project, packageRepository, _pluginFinder);
			}

			var projectManager = new ProjectManager(
				packageRepository,
				new DefaultPackagePathResolver(targetPath),
				project,
				referenceRepository
				) { Logger = logger };

			// add the package to the project
			projectManager.AddPackageReference(package, true, false);

			return new PackageInfo
			{
				Id = package.Id,
				Name = package.Title ?? package.Id,
				Version = package.Version.ToString(),
				Type = package.IsTheme() ? "Theme" : "Plugin",
				Path = targetPath
			};
		}

		public void Uninstall(string packageId, string applicationFolder)
		{
			// TODO: (pkg) Localization

			string extensionFullPath = string.Empty;

			if (packageId.StartsWith(PackagingUtils.GetExtensionPrefix("Theme")))
			{
				extensionFullPath = _virtualPathProvider.MapPath("~/Themes/" + packageId.Substring(PackagingUtils.GetExtensionPrefix("Theme").Length));
			}
			else if (packageId.StartsWith(PackagingUtils.GetExtensionPrefix("Plugin")))
			{
				extensionFullPath = _virtualPathProvider.MapPath("~/Plugins/" + packageId.Substring(PackagingUtils.GetExtensionPrefix("Plugin").Length));
			}

			if (string.IsNullOrEmpty(extensionFullPath) || !System.IO.Directory.Exists(extensionFullPath))
			{
				throw new SmartException("Package not found: {0}".FormatCurrentUI(packageId));
			}

			// If the package was not installed through nuget we still need to try to uninstall it by removing its directory
			if (Directory.Exists(extensionFullPath))
			{
				Directory.Delete(extensionFullPath, true);
			}
		}

		private bool RestoreExtensionFolder(string extensionFolder, string extensionId)
		{
			var virtualSource = _virtualPathProvider.Combine("~", extensionFolder, extensionId);
			var source = new DirectoryInfo(_virtualPathProvider.MapPath(virtualSource));

			if (source.Exists)
			{
				var tempPath = _virtualPathProvider.Combine("~/App_Data", "_Backup", extensionFolder, extensionId);
				string localTempPath = null;
				for (int i = 0; i < 1000; i++)
				{
					localTempPath = _virtualPathProvider.MapPath(tempPath) + (i == 0 ? "" : "." + i.ToString());
					if (!System.IO.Directory.Exists(localTempPath))
					{
						System.IO.Directory.CreateDirectory(localTempPath);
						break;
					}
					localTempPath = null;
				}

				if (localTempPath == null)
				{
					throw new SmartException("Backup folder {0} has too many backups subfolder (limit is 1.000)".FormatInvariant(tempPath));
				}

				var backupFolder = new DirectoryInfo(localTempPath);
				_folderUpdater.Restore(backupFolder, source);
				_notifier.Information("Successfully restored local package to local folder '{0}'.".FormatInvariant(virtualSource));

				return true;
			}

			return false;
		}

		private bool BackupExtensionFolder(string extensionFolder, string extensionId)
		{
			var source = new DirectoryInfo(_virtualPathProvider.MapPath(_virtualPathProvider.Combine("~", extensionFolder, extensionId)));

			if (source.Exists)
			{
				var tempPath = _virtualPathProvider.Combine("~/App_Data", "_Backup", extensionFolder, extensionId);
				string localTempPath = null;
				for (int i = 0; i < 1000; i++)
				{
					localTempPath = _virtualPathProvider.MapPath(tempPath) + (i == 0 ? "" : "." + i.ToString());
					if (!System.IO.Directory.Exists(localTempPath))
					{
						System.IO.Directory.CreateDirectory(localTempPath);
						break;
					}
					localTempPath = null;
				}

				if (localTempPath == null)
				{
					throw new SmartException("Backup folder {0} has too many backups subfolder (limit is 1.000)".FormatInvariant(tempPath));
				}

				var backupFolder = new DirectoryInfo(localTempPath);
				_folderUpdater.Backup(source, backupFolder);
				_notifier.Information("Successfully backed up local package to local folder '{0}'".FormatInvariant(backupFolder.Name));

				return true;
			}

			return false;
		}

		private void UninstallExtensionIfNeeded(IPackage package)
		{
			// Nuget requires to un-install the currently installed packages if the new
			// package is the same version or an older version
			try
			{
				Uninstall(package.Id, _virtualPathProvider.MapPath("~\\"));
				_notifier.Information("Successfully un-installed local package {0}".FormatInvariant(package.ExtensionId()));
			}
			catch { }
		}
	}

}
