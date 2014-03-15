using System;

namespace SmartStore.Services.Events
{
	public class EventConsumerMetadata
	{
		public bool ExecuteAsync { get; set; }
		public bool IsActive { get; set; }
	}
}
