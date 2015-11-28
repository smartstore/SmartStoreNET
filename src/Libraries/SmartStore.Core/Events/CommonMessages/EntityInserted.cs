
namespace SmartStore.Core.Events
{
    /// <summary>
    /// A container for entities that have been inserted.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EntityInserted<T> : ComparableObject<T> where T : BaseEntity
    {

        public EntityInserted(T entity)
        {
            this.Entity = entity;
        }

		[ObjectSignature]
        public T Entity { get; private set; }
    }
}
