using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using SmartStore.Collections;
using SmartStore.Core.Events;

namespace SmartStore.Services.Events
{
    public class ConsumerRegistry : IConsumerRegistry
    {
        private readonly Multimap<Type, ConsumerDescriptor> _descriptorMap = new Multimap<Type, ConsumerDescriptor>();

        public ConsumerRegistry(IEnumerable<Lazy<IConsumer, EventConsumerMetadata>> consumers)
        {
            foreach (var consumer in consumers)
            {
                var metadata = consumer.Metadata;

                if (!metadata.IsActive)
                    continue;

                var methods = FindMethods(metadata);
                var messageTypes = new Dictionary<Type, MethodInfo>();

                foreach (var method in methods)
                {
                    var descriptor = new ConsumerDescriptor(metadata)
                    {
                        IsAsync = method.ReturnType == typeof(Task),
                        FireForget = method.HasAttribute<FireForgetAttribute>(false)
                    };

                    //if (descriptor.IsAsync && descriptor.FireForget)
                    //{
                    //	throw new NotSupportedException($"An asynchronous message consumer method cannot be called as fire & forget. Method: '{method}'.");
                    //}

                    if (method.ReturnType != typeof(Task) && method.ReturnType != typeof(void))
                    {
                        throw new NotSupportedException($"A message consumer method's return type must either be 'void' or '${typeof(Task).FullName}'. Method: '{method}'.");
                    }

                    if (method.Name.EndsWith("Async") && !descriptor.IsAsync)
                    {
                        throw new NotSupportedException($"A synchronous message consumer method name should not end with 'Async'. Method: '{method}'.");
                    }

                    var parameters = method.GetParameters();
                    if (parameters.Length == 0)
                    {
                        throw new NotSupportedException($"A message consumer method must have at least one parameter identifying the message to consume. Method: '{method}'.");
                    }

                    if (parameters.Any(x => x.ParameterType.IsByRef || x.IsOut || x.IsOptional))
                    {
                        throw new NotSupportedException($"'out', 'ref' and optional parameters are not allowed in consumer methods. Method: '{method}'.");
                    }

                    var p = parameters[0];
                    var messageType = p.ParameterType;

                    if (messageType.IsGenericType && messageType.GetGenericTypeDefinition() == typeof(ConsumeContext<>))
                    {
                        messageType = messageType.GetGenericArguments()[0];
                        descriptor.WithEnvelope = true;
                    }

                    if (messageTypes.TryGetValue(messageType, out var method2))
                    {
                        // We won't allow methods with different signatures, but same message type: there can only be one!
                        throw new AmbigousConsumerException(method2, method);
                    }

                    messageTypes.Add(messageType, method);

                    if (messageType.IsPublic && (messageType.IsClass || messageType.IsInterface))
                    {
                        // The method signature is valid: add to dictionary.
                        descriptor.MessageParameter = p;
                        descriptor.Parameters = parameters.Skip(1).ToArray();
                        descriptor.MessageType = messageType;
                        descriptor.Method = method;

                        _descriptorMap.Add(messageType, descriptor);
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
