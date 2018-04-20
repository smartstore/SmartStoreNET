
namespace SmartStore.Core.Events
{
    public interface IConsumer<T>
    {
        void HandleEvent(T message);
    }
}
