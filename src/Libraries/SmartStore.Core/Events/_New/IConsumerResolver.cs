using System;

namespace SmartStore.Core.Events
{
	public interface IConsumerResolver
	{
		IConsumer Resolve(ConsumerDescriptor descriptor);
	}
}
