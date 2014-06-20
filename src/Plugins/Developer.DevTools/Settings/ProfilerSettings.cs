using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SmartStore.Core.Configuration;

namespace SmartStore.Plugin.Developer.DevTools
{
	public class ProfilerSettings : ISettings
	{
		public bool EnableMiniProfilerInPublicStore { get; set; }
	}
}