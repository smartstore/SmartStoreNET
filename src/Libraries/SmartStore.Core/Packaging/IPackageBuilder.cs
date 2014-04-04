using System;
using System.IO;
using SmartStore.Core.Themes;
using SmartStore.Core.Plugins;

namespace SmartStore.Core.Packaging
{
	public interface IPackageBuilder
	{
		Stream BuildPackage(PluginDescriptor pluginDescriptor);
		Stream BuildPackage(ThemeManifest themeManifest);
	}
}
