using System;

namespace SmartStore.Core.Events
{
	public class EventConsumerMetadata
	{
		public bool ExecuteAsync { get; set; }
		public bool IsActive { get; set; }
	}
}
