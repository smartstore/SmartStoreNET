
namespace SmartStore.Core.Events
{
    public interface IConsumer<T> : IConsumer
    {
        void HandleEvent(T message);
    }
}
