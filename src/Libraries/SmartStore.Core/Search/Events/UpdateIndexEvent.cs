using System.Collections.Generic;

namespace SmartStore.Core.Search.Events
{
    /// <summary>
    /// Publish event to index <see cref="EntityIds">entities with IDs</c> at next indexing.
    /// </summary>
    public class UpdateIndexEvent
    {
        public UpdateIndexEvent(string scope, IEnumerable<int> entityIds)
        {
            Guard.NotEmpty(scope, nameof(scope));
            Guard.NotNull(entityIds, nameof(entityIds));

            Scope = scope;
            EntityIds = entityIds;
        }

        public string Scope
        {
            get;
            private set;
        }

        public IEnumerable<int> EntityIds
        {
            get;
            private set;
        }
    }
}
