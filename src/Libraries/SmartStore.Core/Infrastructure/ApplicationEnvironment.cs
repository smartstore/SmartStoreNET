using System;
using SmartStore.Core.Data;
using SmartStore.Core.IO;
using SmartStore.Core.Logging;
using SmartStore.Utilities;

namespace SmartStore.Core
{
    public class ApplicationEnvironment : IApplicationEnvironment
    {
        public ApplicationEnvironment(IVirtualPathProvider vpp, ILogger logger)
        {
            WebRootFolder = new VirtualFolder("~/", vpp, logger);
            AppDataFolder = new VirtualFolder("~/App_Data/", vpp, logger);
            ThemesFolder = new VirtualFolder(CommonHelper.GetAppSetting<string>("sm:ThemesBasePath", "~/Themes/"), vpp, logger);
            PluginsFolder = new VirtualFolder("~/Plugins/", vpp, logger);

            if (DataSettings.Current.IsValid())
            {
                TenantFolder = new VirtualFolder(DataSettings.Current.TenantPath, vpp, logger);
            }
        }

        public virtual string MachineName => Environment.MachineName;

        public virtual string EnvironmentIdentifier =>
                // use the current host and the process id as two servers could run on the same machine
                Environment.MachineName + "-" + System.Diagnostics.Process.GetCurrentProcess().Id;

        public virtual IVirtualFolder WebRootFolder { get; private set; }
        public virtual IVirtualFolder AppDataFolder { get; private set; }
        public virtual IVirtualFolder ThemesFolder { get; private set; }
        public virtual IVirtualFolder PluginsFolder { get; private set; }
        public virtual IVirtualFolder TenantFolder { get; private set; }
    }
}
