using System;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using SmartStore.Core.Plugins;
using SmartStore.Utilities;

namespace SmartStore.Data.Caching
{
    public sealed class EfDbModelStore : DefaultDbModelStore
    {
        public EfDbModelStore(string location)
            : base(location)
        {
        }

        public override DbCompiledModel TryLoad(Type contextType)
        {
            string path = GetFilePath(contextType);

            if (File.Exists(path))
            {
                var cachedModelCreatedOn = File.GetLastWriteTimeUtc(path);
                var localAssemblyFile = FindLocalAssemblyFile(contextType.Assembly);
                if (localAssemblyFile == null || !localAssemblyFile.Exists || localAssemblyFile.LastWriteTimeUtc > cachedModelCreatedOn)
                {
                    File.Delete(path);
                    Debug.WriteLine("Cached db model obsolete. Re-creating cached db model edmx.");
                }
            }
            else
            {
                Debug.WriteLine("No cached db model found. Creating cached db model edmx.");
            }

            return base.TryLoad(contextType);
        }

        private FileInfo FindLocalAssemblyFile(Assembly assembly)
        {
            // ASP.NET loads assemblies from its dynamic temp folder,
            // where DLLs get redeployed on every app startup, which
            // makes it impossible for us to check for updated DLLs.
            // Therefore we'll find passed assembly in the app folder for datetime checking.
            var file = new FileInfo(assembly.Location);

            if (file.Name.IsCaseInsensitiveEqual("SmartStore.Data.dll"))
            {
                return new FileInfo(Path.Combine(CommonHelper.MapPath("~/bin"), file.Name));
            }

            var pluginSystemName = Path.GetFileNameWithoutExtension(file.Name);
            var pluginDescriptor = PluginFinder.Current.GetPluginDescriptorBySystemName(pluginSystemName, false);

            return pluginDescriptor?.Assembly?.OriginalFile;
        }
    }
}
