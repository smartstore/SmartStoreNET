using System;
using System.Collections.Generic;

namespace SmartStore.Core.Events
{
	public interface IConsumerRegistry
	{
		IEnumerable<ConsumerDescriptor> GetConsumers(object message);
	}
}
