using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using SmartStore.Utilities;
using SmartStore.Utilities.Threading;
using SmartStore.Core.Logging;
using Log = SmartStore.Core.Logging;
using NuGet;
using NuGetPackageManager = NuGet.PackageManager;
using SmartStore.Core.Data;

namespace SmartStore.Core.Packaging
{
	
	internal sealed class AppUpdater : DisposableObject
	{
		private const string UpdatePackagePath = "~/App_Data/Update";
		
		private static readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();
		private TraceLogger _logger;

		public bool TryUpdate()
		{
			//if (!DataSettings.DatabaseIsInstalled())
			//	return false;
			
			// NEVER EVER (!!!) make an attempt to auto-update in a dev environment!!!!!!!
			if (CommonHelper.IsDevEnvironment)
				return false;
			
			using (_rwLock.GetUpgradeableReadLock())
			{
				var package = FindPackage();

				if (package == null)
					return false;

				if (!ValidatePackage(package))
					return false;

				if (!CheckEnvironment())
					return false;

				using (_rwLock.GetWriteLock())
				{
					var info = ExecuteUpdate(package);

					FinalizeUpdate();

					return true;
				}
			}
		}

		private TraceLogger CreateLogger(IPackage package)
		{
			var logFile = Path.Combine(CommonHelper.MapPath(UpdatePackagePath, false), "Updater.{0}.log".FormatInvariant(package.Version.ToString()));
			return new TraceLogger(logFile);
		}

		private IPackage FindPackage()
		{
			var dir = CommonHelper.MapPath(UpdatePackagePath, false);
			var files = Directory.GetFiles(dir, "SmartStore.*.nupkg", SearchOption.TopDirectoryOnly);

			// TODO: allow more than one package in folder and return newest
			if (files == null || files.Length == 0 || files.Length > 1)
				return null;

			IPackage package = null;

			try
			{
				package = new ZipPackage(files[0]);
				_logger = CreateLogger(package);
				_logger.Information("Found update package '{0}'".FormatInvariant(package.GetFullName()));
				return package;
			}
			catch { }

			return null;
		}

		private bool ValidatePackage(IPackage package)
		{
			if (package.Id != "SmartStore")
				return false;
			
			var currentVersion = new SemanticVersion(SmartStoreVersion.FullVersion);
			return package.Version > currentVersion;
		}

		private bool CheckEnvironment()
		{
			return true;
		}

		private PackageInfo ExecuteUpdate(IPackage package)
		{
			var appPath = CommonHelper.MapPath("~/");
			//var appPath = "D:\\_temp\\AppUpdater";
			
			var logger = new NugetLogger(_logger);

			var project = new FileBasedProjectSystem(appPath) { Logger = logger };

			var nullRepository = new NullSourceRepository();

			var projectManager = new ProjectManager(
				nullRepository,
				new DefaultPackagePathResolver(appPath),
				project,
				nullRepository
				) { Logger = logger };

			// Perform the update
			projectManager.AddPackageReference(package, true, false);

			return new PackageInfo
			{
				Id = package.Id,
				Name = package.Title ?? package.Id,
				Version = package.Version.ToString(),
				Type = "App",
				Path = appPath
			};
		}

		private void FinalizeUpdate()
		{
		}


		protected override void OnDispose(bool disposing)
		{
			if (disposing)
			{
				if (_logger != null)
				{
					_logger.Dispose();
					_logger = null;
				}
			}
		}
	}

}
