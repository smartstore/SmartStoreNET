using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SmartStore.Core.Configuration;

namespace SmartStore.DevTools
{
	public class ProfilerSettings : ISettings
	{
		public bool EnableMiniProfilerInPublicStore { get; set; }

        public bool DisplayWidgetZones { get; set; }

	}
}