using System;
using System.IO;
using SmartStore.Core;
using SmartStore.Core.Infrastructure.DependencyManagement;
using SmartStore.Core.IO;
using SmartStore.Core.Logging;
using SmartStore.Core.Packaging;
using SmartStore.Core.Plugins;
using SmartStore.Core.Themes;

namespace SmartStore.Packager
{	
	internal class PackageCreator
	{
		private readonly IVirtualPathProvider _vpp;
		private readonly IPackageBuilder _packageBuilder;
		private readonly string _rootPath;
		private readonly string _outputPath;
		
		public PackageCreator(string rootPath, string outputPath)
		{
			_rootPath = rootPath;
			_outputPath = outputPath;

			_vpp = new RootedVirtualPathProvider(rootPath);
			_packageBuilder = new PackageBuilder(new ApplicationEnvironment(_vpp, NullLogger.Instance));
		}

		public FileInfo CreatePluginPackage(string path)
		{
			string virtualPath = _vpp.Combine(path, "Description.txt");

			if (!_vpp.FileExists(virtualPath))
			{
				return null;
			}

			var descriptor = PluginFileParser.ParsePluginDescriptionFile(_vpp.MapPath(virtualPath));

			if (descriptor != null)
			{
				return CreatePluginPackage(descriptor);
			}

			return null;
		}

		public FileInfo CreatePluginPackage(PluginDescriptor descriptor)
		{
			var result = new PackagingResult
			{
				ExtensionType = "Plugin",
				PackageName = descriptor.FolderName,
				PackageVersion = descriptor.Version.ToString(),
				PackageStream = _packageBuilder.BuildPackage(descriptor)
			};

			return SavePackageFile(result);
		}

		public FileInfo CreateThemePackage(string virtualPath)
		{
			//string virtualPath = "~/Themes/{0}".FormatInvariant(themeName);

			var manifest = ThemeManifest.Create(_vpp.MapPath(virtualPath));

			if (manifest != null)
			{
				return CreateThemePackage(manifest);
			}

			return null;
		}

		public FileInfo CreateThemePackage(ThemeManifest manifest)
		{
			var result = new PackagingResult
			{
				ExtensionType = "Theme",
				PackageName = manifest.ThemeName,
				PackageVersion = manifest.Version,
				PackageStream = _packageBuilder.BuildPackage(manifest)
			};

			return SavePackageFile(result);
		}

		private FileInfo SavePackageFile(PackagingResult result)
		{
			var fileName = string.Format("{0}{1}.{2}.nupkg",
				PackagingUtils.GetExtensionPrefix(result.ExtensionType),
				result.PackageName,
				result.PackageVersion);

			if (!Directory.Exists(_outputPath))
			{
				Directory.CreateDirectory(_outputPath);
			}

			fileName = Path.Combine(_outputPath, fileName);

			using (var stream = File.Create(fileName))
			{
				result.PackageStream.CopyTo(stream);
			}

			var fileInfo = new FileInfo(fileName);

			return fileInfo;
		}
	}
}
