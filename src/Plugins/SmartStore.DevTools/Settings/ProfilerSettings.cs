using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SmartStore.Core.Configuration;
using SmartStore.Web.Framework;

namespace SmartStore.DevTools
{
	public class ProfilerSettings : ISettings
	{
        [SmartResourceDisplayName("Plugins.Developer.DevTools.EnableMiniProfilerInPublicStore")]
        public bool EnableMiniProfilerInPublicStore { get; set; }

        [SmartResourceDisplayName("Plugins.Developer.DevTools.DisplayWidgetZones")]
        public bool DisplayWidgetZones { get; set; }

        [SmartResourceDisplayName("Plugins.Developer.DevTools.DisplayMachineName")]
        public bool DisplayMachineName { get; set; }
	}
}