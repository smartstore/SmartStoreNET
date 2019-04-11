using System;
using SmartStore.Core.Plugins;

namespace SmartStore.Core.Events
{
	public class EventConsumerMetadata
	{
		public bool ExecuteAsync { get; set; }
		public bool IsActive { get; set; }
		public Type ContainerType { get; set; }
		public PluginDescriptor PluginDescriptor { get; set; }
	}
}
