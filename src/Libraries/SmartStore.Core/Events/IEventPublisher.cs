
namespace SmartStore.Core.Events
{
    public interface IEventPublisher
    {
        void Publish<T>(T eventMessage);
    }
}
