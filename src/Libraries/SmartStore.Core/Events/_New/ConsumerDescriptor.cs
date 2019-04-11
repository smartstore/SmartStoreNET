using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.ComponentModel;
using SmartStore.Core.Plugins;

namespace SmartStore.Core.Events
{
	public class ConsumerDescriptor
	{
		public bool WithEnvelope { get; set; }
		public bool IsAsync { get; set; }
		public PluginDescriptor PluginDescriptor { get; set; }

		public Type MessageType { get; set; }
		public Type ContainerType { get; set; }
		public FastInvoker Invoker { get; set; }
	}
}
