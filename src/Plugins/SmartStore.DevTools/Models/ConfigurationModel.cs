using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.DevTools.Models
{
    public class ConfigurationModel : ModelBase
    {
        [SmartResourceDisplayName("Plugins.Developer.DevTools.EnableMiniProfilerInPublicStore")]
        public bool EnableMiniProfilerInPublicStore { get; set; }

        [SmartResourceDisplayName("Plugins.Developer.DevTools.DisplayWidgetZones")]
        public bool DisplayWidgetZones { get; set; }

        [SmartResourceDisplayName("Plugins.Developer.DevTools.DisplayMachineName")]
        public bool DisplayMachineName { get; set; }
    }
}