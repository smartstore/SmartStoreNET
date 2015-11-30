
using System.Collections.Generic;
namespace SmartStore.Core.Events
{
    /// <summary>
    /// A container for entities that are updated.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EntityUpdated<T> : ComparableObject<T> where T : BaseEntity
    {

        public EntityUpdated(T entity)
        {
            this.Entity = entity;
        }

		[ObjectSignature]
        public T Entity { get; private set; }

    }
}
