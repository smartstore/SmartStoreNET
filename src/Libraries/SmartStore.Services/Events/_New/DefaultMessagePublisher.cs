using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.ComponentModel;
using SmartStore.Core.Events;
using SmartStore.Core.Logging;

namespace SmartStore.Services.Events
{
	public class DefaultMessagePublisher : IMessagePublisher
	{
		private readonly IConsumerRegistry _registry;
		private readonly IConsumerResolver _resolver;

		public DefaultMessagePublisher(IConsumerRegistry registry, IConsumerResolver resolver)
		{
			_registry = registry;
			_resolver = resolver;

			Logger = NullLogger.Instance;
		}

		public ILogger Logger { get; set; }

		public void Publish<T>(T message) where T : class
		{
			var descriptors = _registry.GetConsumers(message);

			if (!descriptors.Any())
			{
				return;
			}

			var envelopeType = typeof(ConsumeContext<>).MakeGenericType(typeof(T));
			var envelope = (ConsumeContext<T>)FastActivator.CreateInstance(envelopeType, message);

			foreach (var descriptor in descriptors)
			{
				var consumer = _resolver.Resolve(descriptor);
				if (consumer != null)
				{
					InvokeConsumer(descriptor, envelope, consumer);
				}
			}
		}

		private void InvokeConsumer<T>(ConsumerDescriptor descriptor, ConsumeContext<T> envelope, IConsumer consumer) where T : class
		{
			var p = descriptor.WithEnvelope ? (object)envelope : envelope.Message;

			try
			{
				descriptor.Invoker.Invoke(consumer, p);
			}
			catch (Exception ex)
			{
				Logger.Error(ex);
				throw;
			}
		}
	}
}
