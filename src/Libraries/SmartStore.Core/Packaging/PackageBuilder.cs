using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using SmartStore.Core.Plugins;
using SmartStore.Core.Themes;
using NuGet;
using NuGetPackageBuilder = NuGet.PackageBuilder;
using System.Xml.Linq;
using SmartStore.Core.IO.WebSite;
using System.Runtime.Versioning;
using System.Net.Mime;
using SmartStore.Core.IO.VirtualPath;

namespace SmartStore.Core.Packaging
{

	public class PackageBuilder : IPackageBuilder
	{
		private readonly IWebSiteFolder _webSiteFolder;
		private readonly IVirtualPathProvider _virtualPathProvider;

		public PackageBuilder(IWebSiteFolder webSiteFolder, IVirtualPathProvider virtualPathProvider)
		{
			this._webSiteFolder = webSiteFolder;
			this._virtualPathProvider = virtualPathProvider;
		}

		private static readonly string[] _ignoredThemeExtensions = new[] {
            "obj", "pdb", "exclude"
        };

		private static readonly string[] _ignoredThemePaths = new[] {
            "/obj/"
        };

		private static bool IgnoreFile(string filePath)
		{
			return String.IsNullOrEmpty(filePath) ||
				_ignoredThemePaths.Any(filePath.Contains) ||
				_ignoredThemeExtensions.Contains(Path.GetExtension(filePath) ?? "");
		}


		public Stream BuildPackage(PluginDescriptor pluginDescriptor)
		{
			return BuildPackage(PackagingUtils.ConvertToExtensionDescriptor(pluginDescriptor));
		}

		public Stream BuildPackage(ThemeManifest themeManifest)
		{
			return BuildPackage(PackagingUtils.ConvertToExtensionDescriptor(themeManifest));
		}

		private Stream BuildPackage(ExtensionDescriptor extensionDescriptor)
		{
			var context = new BuildContext();
			BeginPackage(context);
			try
			{
				EstablishPaths(context, _webSiteFolder, extensionDescriptor.Location, extensionDescriptor.Id, extensionDescriptor.ExtensionType);
				SetCoreProperties(context, extensionDescriptor);
				EmbedFiles(context);
			}
			finally
			{
				EndPackage(context);
			}

			if (context.Stream.CanSeek)
			{
				context.Stream.Seek(0, SeekOrigin.Begin);
			}

			return context.Stream;
		}

		private static void BeginPackage(BuildContext context)
		{
			context.Stream = new MemoryStream();
			context.Builder = new NuGetPackageBuilder();
		}

		private static void EndPackage(BuildContext context)
		{
			context.Builder.Save(context.Stream);
		}

		private static void SetCoreProperties(BuildContext context, ExtensionDescriptor extensionDescriptor)
		{
			context.Builder.Id = PackagingUtils.BuildPackageId(extensionDescriptor.Id, extensionDescriptor.ExtensionType);
			context.Builder.Version = new SemanticVersion(extensionDescriptor.Version);
			context.Builder.Title = extensionDescriptor.Name ?? extensionDescriptor.Id;
			context.Builder.Description = extensionDescriptor.Description.NullEmpty() ?? "No Description";
			context.Builder.Authors.Add(extensionDescriptor.Author ?? "Unknown");

			if (Uri.IsWellFormedUriString(extensionDescriptor.WebSite, UriKind.Absolute))
			{
				context.Builder.ProjectUrl = new Uri(extensionDescriptor.WebSite);
			}
		}

		private static void EstablishPaths(BuildContext context, IWebSiteFolder webSiteFolder, string locationPath, string extensionName, string extensionType = "Plugin")
		{
			context.SourceFolder = webSiteFolder;
			if (extensionType.IsCaseInsensitiveEqual("theme"))
			{
				context.SourcePath = "~/Themes/" + extensionName + "/";
				context.TargetPath = "\\Content\\Themes\\" + extensionName + "\\";
			}
			else
			{
				context.SourcePath = "~/Plugins/" + extensionName + "/";
				context.TargetPath = "\\Content\\Plugins\\" + extensionName + "\\";
			}
		}

		private static void EmbedFiles(BuildContext context)
		{
			var basePath = context.SourcePath;
			foreach (var virtualPath in context.SourceFolder.ListFiles(context.SourcePath, true))
			{
				// skip ignores files
				if (IgnoreFile(virtualPath))
				{
					continue;
				}
				// full virtual path given but we need the relative path so it can be put into
				// the package that way (the package itself is the logical base path).
				// Get it by stripping the basePath off including the slash.
				var relativePath = virtualPath.Replace(basePath, "");
				EmbedVirtualFile(context, relativePath, MediaTypeNames.Application.Octet);
			}
		}

		private static void EmbedVirtualFile(BuildContext context, string relativePath, string contentType)
		{
			var file = new VirtualPackageFile(
				context.SourceFolder,
				context.SourcePath + relativePath,
				context.TargetPath + relativePath);
			context.Builder.Files.Add(file);
		}


		#region Nested type: BuildContext

		private class BuildContext
		{
			public Stream Stream { get; set; }
			public NuGetPackageBuilder Builder { get; set; }

			public IWebSiteFolder SourceFolder { get; set; }
			public string SourcePath { get; set; }
			public string TargetPath { get; set; }

			public XDocument Project { get; set; }
		}

		#endregion

		#region Nested type: VirtualPackageFile

		private class VirtualPackageFile : IPackageFile
		{
			private readonly IWebSiteFolder _webSiteFolder;
			private readonly string _virtualPath;
			private readonly string _packagePath;

			public VirtualPackageFile(IWebSiteFolder webSiteFolder, string virtualPath, string packagePath)
			{
				_webSiteFolder = webSiteFolder;
				_virtualPath = virtualPath;
				_packagePath = packagePath;
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Supposed to return an open stream.")]
			public Stream GetStream()
			{
				var stream = new MemoryStream();
				_webSiteFolder.CopyFileTo(_virtualPath, stream);
				stream.Seek(0, SeekOrigin.Begin);
				return stream;
			}

			public string Path
			{
				get { return _packagePath; }
			}

			public string EffectivePath
			{
				get { return _packagePath; }
			}

			public FrameworkName TargetFramework
			{
				get { return null; }
			}

			public IEnumerable<FrameworkName> SupportedFrameworks
			{
				get { return Enumerable.Empty<FrameworkName>(); }
			}
		}

		#endregion

	}

}
