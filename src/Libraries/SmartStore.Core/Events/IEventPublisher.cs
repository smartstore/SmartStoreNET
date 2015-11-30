
namespace SmartStore.Core.Events
{
    public interface IEventPublisher
    {
        void Publish<T>(T eventMessage);
    }

	public static class IEventPublisherExtensions
	{
		public static void EntityInserted<T>(this IEventPublisher eventPublisher, T entity) where T : BaseEntity
		{
			eventPublisher.Publish(new EntityInserted<T>(entity));
		}

		public static void EntityUpdated<T>(this IEventPublisher eventPublisher, T entity) where T : BaseEntity
		{
			eventPublisher.Publish(new EntityUpdated<T>(entity));
		}

		public static void EntityDeleted<T>(this IEventPublisher eventPublisher, T entity) where T : BaseEntity
		{
			eventPublisher.Publish(new EntityDeleted<T>(entity));
		}
	}
}
