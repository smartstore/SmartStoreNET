using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Hosting;
using SmartStore.Core.Data;
using SmartStore.Core.Plugins;
using SmartStore.Utilities;

namespace SmartStore.Web.Framework.Theming.Assets
{
    public sealed class ModuleImport
    {
        public string VirtualPath { get; internal set; }
        public string PhysicalPath { get; internal set; }
        public PluginDescriptor PluginDescriptor { get; internal set; }
        public bool IsAdmin { get; internal set; }

        public override string ToString()
        {
            return VirtualPath;
        }

        public override int GetHashCode()
        {
            return VirtualPath.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ModuleImport other))
                return false;

            return string.Equals(this.VirtualPath, other.VirtualPath, StringComparison.OrdinalIgnoreCase);
        }
    }

    public class ModuleImportsVirtualFile : VirtualFile, IFileDependencyProvider
    {
        private static readonly HashSet<ModuleImport> _adminImports;
        private static readonly HashSet<ModuleImport> _publicImports;

        static ModuleImportsVirtualFile()
        {
            _adminImports = new HashSet<ModuleImport>();
            _publicImports = new HashSet<ModuleImport>();

            if (DataSettings.DatabaseIsInstalled())
            {
                CollectModuleImports();
            }
        }

        private static void CollectModuleImports()
        {
            var installedPlugins = PluginManager.ReferencedPlugins.Where(x => x.Installed);
            var root = PluginManager.PluginsLocation;

            foreach (var plugin in installedPlugins)
            {
                var contentDir = Path.Combine(plugin.PhysicalPath, "Content");
                if (!Directory.Exists(contentDir))
                    continue;

                TryAddImport(plugin, _publicImports, "public.scss");
                TryAddImport(plugin, _adminImports, "admin.scss");
            }

            void TryAddImport(PluginDescriptor plugin, HashSet<ModuleImport> imports, string name)
            {
                var physicalPath = Path.Combine(plugin.PhysicalPath, "Content", name);
                if (File.Exists(physicalPath))
                {
                    imports.Add(new ModuleImport
                    {
                        PhysicalPath = physicalPath,
                        VirtualPath = $"{root}/{plugin.FolderName}/Content/{name}",
                        PluginDescriptor = plugin,
                        IsAdmin = name.Contains("admin")
                    });
                }
            }
        }

        public static ModuleImport[] PublicImports => _publicImports.ToArray();

        public static ModuleImport[] AdminImports => _adminImports.ToArray();

        public ModuleImportsVirtualFile(string virtualPath, bool isAdmin)
            : base(virtualPath)
        {
            IsAdmin = isAdmin;
        }

        public override bool IsDirectory => false;

        public bool IsAdmin
        {
            get;
            private set;
        }

        public override Stream Open()
        {
            var sb = new StringBuilder();

            var imports = IsAdmin ? _adminImports : _publicImports;
            foreach (var imp in imports)
            {
                sb.AppendLine($"@import '{imp.VirtualPath}';");
            }

            return GenerateStreamFromString(sb.ToString());
        }

        public void HashCombine(HashCodeCombiner combiner)
        {
            Guard.NotNull(combiner, nameof(combiner));

            var imports = IsAdmin ? _adminImports : _publicImports;
            foreach (var imp in imports)
            {
                combiner.Add(new FileInfo(imp.PhysicalPath));
            }
        }

        private Stream GenerateStreamFromString(string value)
        {
            var stream = new MemoryStream();

            using (var writer = new StreamWriter(stream, Encoding.Unicode, 1024, true))
            {
                writer.Write(value);
                writer.Flush();
                stream.Seek(0, SeekOrigin.Begin);
                return stream;
            }
        }

        public void AddFileDependencies(ICollection<string> mappedPaths, ICollection<string> cacheKeys)
        {
            var imports = IsAdmin ? _adminImports : _publicImports;
            foreach (var imp in imports)
            {
                mappedPaths.Add(imp.PhysicalPath);
            }
        }
    }
}