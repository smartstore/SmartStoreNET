using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SmartStore.Core.Plugins
{
	public class LoadPluginResult
	{
		public FileInfo DescriptionFile { get; set; }
		public PluginDescriptor Descriptor { get; set; }
		public bool IsIncompatible { get; set; }
		public bool Success { get; set; }
	}
}
