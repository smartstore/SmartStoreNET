using SmartStore.Core.Configuration;

namespace SmartStore.DevTools
{
    public class ProfilerSettings : ISettings
    {
        public bool EnableMiniProfilerInPublicStore { get; set; }
        public bool DisplayWidgetZones { get; set; }
        public bool DisplayMachineName { get; set; }
    }
}