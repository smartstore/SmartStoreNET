namespace SmartStore.Core.Events
{
    /// <summary>
    /// Responsible for invoking event message handler methods.
    /// </summary>
    public interface IConsumerInvoker
    {
        void Invoke<TMessage>(ConsumerDescriptor descriptor, IConsumer consumer, ConsumeContext<TMessage> envelope) where TMessage : class;
    }
}
