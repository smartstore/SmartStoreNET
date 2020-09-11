using System;
using System.Reflection;
using SmartStore.Core.Plugins;

namespace SmartStore.Core.Events
{
    /// <summary>
    /// Contains metadata about a consumer / event handler method.
    /// </summary>
    public class ConsumerDescriptor
    {
        public ConsumerDescriptor()
        {
        }

        public ConsumerDescriptor(EventConsumerMetadata metadata)
        {
            ContainerType = metadata.ContainerType;
            PluginDescriptor = metadata.PluginDescriptor;
        }

        public bool WithEnvelope { get; set; }
        public bool IsAsync { get; set; }
        public bool FireForget { get; set; }
        public PluginDescriptor PluginDescriptor { get; set; }

        public Type MessageType { get; set; }
        public Type ContainerType { get; set; }

        public MethodInfo Method { get; set; }
        public ParameterInfo MessageParameter { get; set; }

        /// <summary>
        /// All method parameters except <see cref="MessageParameter"/>
        /// or empty array if <see cref="MessageParameter"/> is the only parameter.
        /// </summary>
        public ParameterInfo[] Parameters { get; set; }

        public override string ToString()
        {
            return "MessageType: {0} - Consumer: {1}.{2}".FormatCurrent(MessageType.Name, ContainerType.FullName, Method.Name);
        }
    }
}
