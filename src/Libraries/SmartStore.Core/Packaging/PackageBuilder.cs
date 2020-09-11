using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using NuGet;
using SmartStore.Core.IO;
using SmartStore.Core.Plugins;
using SmartStore.Core.Themes;
using NuGetPackageBuilder = NuGet.PackageBuilder;

namespace SmartStore.Core.Packaging
{
    public class PackageBuilder : IPackageBuilder
    {
        private readonly IApplicationEnvironment _env;

        public PackageBuilder(IApplicationEnvironment env)
        {
            this._env = env;
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
                _ignoredThemeExtensions.Contains(Path.GetExtension(filePath).NullEmpty() ?? "");
        }


        public Stream BuildPackage(PluginDescriptor pluginDescriptor)
        {
            return BuildPackage(pluginDescriptor.ConvertToExtensionDescriptor());
        }

        public Stream BuildPackage(ThemeManifest themeManifest)
        {
            return BuildPackage(themeManifest.ConvertToExtensionDescriptor());
        }

        private Stream BuildPackage(ExtensionDescriptor extensionDescriptor)
        {
            var context = new BuildContext();
            BeginPackage(context);
            try
            {
                EstablishPaths(context, _env.WebRootFolder, extensionDescriptor.Id, extensionDescriptor.ExtensionType);
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

        private static void EstablishPaths(BuildContext context, IVirtualFolder webRootFolder, string extensionName, string extensionType = "Plugin")
        {
            context.SourceFolder = webRootFolder;
            if (extensionType.IsCaseInsensitiveEqual("theme"))
            {
                context.SourcePath = "Themes/" + extensionName + "/";
                context.TargetPath = "\\Content\\Themes\\" + extensionName + "\\";
            }
            else
            {
                context.SourcePath = "Plugins/" + extensionName + "/";
                context.TargetPath = "\\Content\\Plugins\\" + extensionName + "\\";
            }
        }

        private static void EmbedFiles(BuildContext context)
        {
            var basePath = context.SourcePath;
            foreach (var path in context.SourceFolder.ListFiles(context.SourcePath, true))
            {
                // skip ignores files
                if (IgnoreFile(path))
                {
                    continue;
                }
                // full virtual path given but we need the relative path so it can be put into
                // the package that way (the package itself is the logical base path).
                // Get it by stripping the basePath off including the slash.
                var relativePath = path.Replace(basePath, "");
                EmbedVirtualFile(context, relativePath);
            }
        }

        private static void EmbedVirtualFile(BuildContext context, string relativePath)
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

            public IVirtualFolder SourceFolder { get; set; }
            public string SourcePath { get; set; }
            public string TargetPath { get; set; }
        }

        #endregion

        #region Nested type: VirtualPackageFile

        private class VirtualPackageFile : IPackageFile
        {
            private readonly IVirtualFolder _webRootFolder;
            private readonly string _relativePath;
            private readonly string _packagePath;

            public VirtualPackageFile(IVirtualFolder webRootFolder, string relativePath, string packagePath)
            {
                _webRootFolder = webRootFolder;
                _relativePath = relativePath;
                _packagePath = packagePath;
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Supposed to return an open stream.")]
            public Stream GetStream()
            {
                var stream = new MemoryStream();
                _webRootFolder.CopyFile(_relativePath, stream);
                stream.Seek(0, SeekOrigin.Begin);
                return stream;
            }

            public string Path => _packagePath;

            public string EffectivePath => _packagePath;

            public FrameworkName TargetFramework => null;

            public IEnumerable<FrameworkName> SupportedFrameworks => Enumerable.Empty<FrameworkName>();
        }

        #endregion
    }
}
