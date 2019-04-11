using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Collections;
using SmartStore.ComponentModel;
using SmartStore.Core.Events;
using SmartStore.Core.Infrastructure.DependencyManagement;

namespace SmartStore.Services.Events
{
	public class ConsumerRegistry : IConsumerRegistry
	{
		private readonly static Multimap<Type, ConsumerDescriptor> _descriptorMap
			= new Multimap<Type, ConsumerDescriptor>();

		public ConsumerRegistry(IEnumerable<Lazy<IConsumer, EventConsumerMetadata>> consumers)
		{
			foreach (var consumer in consumers)
			{
				var metadata = consumer.Metadata;

				if (!metadata.IsActive)
					continue;

				var methods = FindMethods(metadata);

				foreach (var method in methods)
				{
					var descriptor = new ConsumerDescriptor
					{
						ContainerType = metadata.ContainerType,
						PluginDescriptor = metadata.PluginDescriptor,
						IsAsync = method.ReturnType == typeof(Task)
					};
					
					if (method.ReturnType != typeof(Task) && method.ReturnType != typeof(void))
					{
						// TODO: better message
						throw new NotSupportedException("A message consumer method's return type must either be 'void' or '{0}'. Method: {1}".FormatInvariant(typeof(Task).FullName));
					}

					if (method.Name.EndsWith("Async") && !descriptor.IsAsync)
					{
						// TODO: better message
						throw new NotSupportedException("A synchronous message consumer method name should not end on 'Async'.");
					}

					var parameters = method.GetParameters();
					if (parameters.Length != 1)
					{
						// TODO: better message
						throw new NotSupportedException("A message consumer method must have exactly one parameter identifying the message to consume.");
					}

					var p = parameters[0];
					if (p.IsRetval || p.IsOut || p.IsOptional)
					{
						// TODO: message
						throw new NotSupportedException();
					}

					var type = p.ParameterType;

					if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ConsumeContext<>))
					{
						type = type.GetGenericArguments()[0];
						descriptor.WithEnvelope = true;
					}

					// TODO: MyEvent and ConsumeContext<MyEvent> must throw "ambigous" exception

					if (type.IsPublic && (type.IsClass || type.IsInterface))
					{
						// The method signature is valid: add to dictionary.
						descriptor.MessageType = type;
						descriptor.Invoker = new FastInvoker(method);
						_descriptorMap.Add(type, descriptor);
					}
					else
					{
						// TODO: message
						throw new NotSupportedException();
					}
				}	
			}
		}

		private IEnumerable<MethodInfo> FindMethods(EventConsumerMetadata metadata)
		{
			var methods = metadata.ContainerType.GetMethods(BindingFlags.Public | BindingFlags.Instance);

			var validNames = new HashSet<string>(new[] { "Handle", "HandleEvent", "Consume", "HandleAsync", "HandleEventAsync", "ConsumeAsync" });

			foreach (var method in methods)
			{
				if (validNames.Contains(method.Name))
				{
					yield return method;
				}
			}
		}

		public virtual IEnumerable<ConsumerDescriptor> GetConsumers(object message)
		{
			Guard.NotNull(message, nameof(message));

			var type = message.GetType();
			if (_descriptorMap.ContainsKey(type))
			{
				return _descriptorMap[type];
			}

			return Enumerable.Empty<ConsumerDescriptor>();
		}
	}
}
