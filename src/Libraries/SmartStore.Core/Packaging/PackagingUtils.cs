using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using NuGet;
using SmartStore.Core.Plugins;
using SmartStore.Core.Themes;

namespace SmartStore.Core.Packaging
{

	public static class PackagingUtils
	{
		
		public static string GetExtensionPrefix(string extensionType)
		{
			return string.Format("SmartStore.{0}.", extensionType);
		}

		public static string BuildPackageId(string extensionName, string extensionType)
		{
			return GetExtensionPrefix(extensionType) + extensionName;
		}



		internal static bool IsTheme(this IPackage package)
		{
			return IsTheme(package.Id);
		}

		public static bool IsTheme(this PackageInfo info)
		{
			return IsTheme(info.Id);
		}

		internal static string ExtensionFolder(this IPackage package)
		{
			return ExtensionFolder(package.IsTheme());
		}

		internal static string ExtensionId(this IPackage package)
		{
			return ExtensionId(package.IsTheme(), package.Id);
		}

		public static string ExtensionId(this PackageInfo info)
		{
			return ExtensionId(info.IsTheme(), info.Id);
		}

		public static bool IsTheme(string packageId)
		{
			return packageId.StartsWith(GetExtensionPrefix("Theme"));
		}

		private static string ExtensionFolder(bool isTheme)
		{
			return isTheme ? "Themes" : "Plugins";
		}

		private static string ExtensionId(bool isTheme, string packageId)
		{
			return isTheme ?
				packageId.Substring(GetExtensionPrefix("Theme").Length) :
				packageId.Substring(GetExtensionPrefix("Plugin").Length);
		}

		internal static ExtensionDescriptor ConvertToExtensionDescriptor(this PluginDescriptor pluginDescriptor)
		{
			// TODO: (pkg) Add Icons to extension manifests
			var descriptor = new ExtensionDescriptor
			{
				ExtensionType = "Plugin",
				Location = "~/Plugins",
				Path = pluginDescriptor.PhysicalPath,
				Id = pluginDescriptor.FolderName,
				Author = pluginDescriptor.Author,
				MinAppVersion = pluginDescriptor.MinAppVersion,
				Version = pluginDescriptor.Version,
				Name = pluginDescriptor.FriendlyName,
				Description = pluginDescriptor.Description,
				WebSite = pluginDescriptor.Url, // TODO: (pkg) Add author url to plugin manifests,
				Tags = string.Empty // TODO: (pkg) Add tags to plugin manifests,
			};

			return descriptor;
		}

		internal static ExtensionDescriptor ConvertToExtensionDescriptor(this ThemeManifest themeManifest)
		{
			var descriptor = new ExtensionDescriptor
			{
				ExtensionType = "Theme",
				Location = "~/Themes",
				Path = themeManifest.Path,
				Id = themeManifest.ThemeName,
				Author = themeManifest.Author.HasValue() ? themeManifest.Author : "[Unknown]",
				MinAppVersion = SmartStoreVersion.Version, // TODO: (pkg) Add SupportedVersion to theme manifests
				Version = new Version(themeManifest.Version),
				Name = themeManifest.ThemeTitle,
				Description = string.Empty, // TODO: (pkg) Add description to theme manifests
				WebSite = themeManifest.Url,
				Tags = string.Empty // TODO: (pkg) Add tags to theme manifests,
			};

			return descriptor;
		}

		internal static ExtensionDescriptor GetExtensionDescriptor(this IPackage package, string extensionType)
		{
			bool isTheme = extensionType.IsCaseInsensitiveEqual("Theme");

			IPackageFile packageFile = package.GetFiles().FirstOrDefault(file =>
			{
				var fileName = Path.GetFileName(file.Path);
				return fileName != null && fileName.Equals(isTheme ? "theme.config" : "Description.txt", StringComparison.OrdinalIgnoreCase);
			});

			ExtensionDescriptor descriptor = null;

			if (packageFile != null)
			{
				var filePath = packageFile.EffectivePath;
				if (filePath.HasValue())
				{
					filePath = Path.Combine(HostingEnvironment.MapPath("~/"), filePath);
					if (isTheme)
					{
						var themeManifest = ThemeManifest.Create(Path.GetDirectoryName(filePath));
						if (themeManifest != null)
						{
							descriptor = themeManifest.ConvertToExtensionDescriptor();
						}
					}
					else // is a Plugin
					{
						var pluginDescriptor = PluginFileParser.ParsePluginDescriptionFile(filePath);
						if (pluginDescriptor != null)
						{
							descriptor = pluginDescriptor.ConvertToExtensionDescriptor();
						}
					}
				}
			}

			return descriptor;
		}
	}

}
