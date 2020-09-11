namespace SmartStore.Core.Events
{
    /// <summary>
    /// Wrapper/Envelope for event message objects. For now, only wraps
    /// the message. There may be other context data in future.
    /// </summary>
    /// <typeparam name="TMessage">Type of message.</typeparam>
    public class ConsumeContext<TMessage>
    {
        public ConsumeContext(TMessage message)
        {
            Message = message;
        }

        public TMessage Message
        {
            get;
            private set;
        }
    }
}
