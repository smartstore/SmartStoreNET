using System.IO;
using SmartStore.Core.Plugins;
using SmartStore.Core.Themes;

namespace SmartStore.Core.Packaging
{
    public interface IPackageBuilder
    {
        Stream BuildPackage(PluginDescriptor pluginDescriptor);
        Stream BuildPackage(ThemeManifest themeManifest);
    }
}
