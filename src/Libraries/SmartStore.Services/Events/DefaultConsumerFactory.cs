using System;
using System.Linq;
using System.Collections.Generic;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Events;

namespace SmartStore.Services.Events
{

	public class DefaultConsumerFactory<T> : IConsumerFactory<T>
	{
		private readonly IEnumerable<Lazy<IConsumer<T>, EventConsumerMetadata>> _consumers;

		public DefaultConsumerFactory(IEnumerable<Lazy<IConsumer<T>, EventConsumerMetadata>> consumers)
		{
			this._consumers = consumers;
		}

		public IEnumerable<IConsumer<T>> GetConsumers(bool? resolveAsyncs = null)
		{
			foreach (var consumer in _consumers)
			{
				var isActive = consumer.Metadata.IsActive;
				var isAsync = consumer.Metadata.ExecuteAsync;
				if (isActive && (resolveAsyncs == null || (resolveAsyncs.Value == isAsync)))
				{
					yield return consumer.Value;
				}
			}
		}


		public bool HasAsyncConsumer
		{
			get 
			{
				return _consumers.Any(c => c.Metadata.IsActive == true && c.Metadata.ExecuteAsync == true); 
			}
		}
	}

}
