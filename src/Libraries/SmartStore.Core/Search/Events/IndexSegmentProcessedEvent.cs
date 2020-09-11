using System.Collections.Generic;

namespace SmartStore.Core.Search
{
    public class IndexSegmentProcessedEvent
    {
        public IndexSegmentProcessedEvent(string scope, IEnumerable<IIndexOperation> operations, bool isRebuild)
        {
            Guard.NotEmpty(scope, nameof(scope));
            Guard.NotNull(operations, nameof(operations));

            Scope = scope;
            IsRebuild = isRebuild;
            Operations = operations;
            Metadata = new Dictionary<string, object>();
        }

        public string Scope
        {
            get;
            private set;
        }

        public bool IsRebuild
        {
            get;
            private set;
        }

        public IEnumerable<IIndexOperation> Operations
        {
            get;
            private set;
        }

        public IDictionary<string, object> Metadata
        {
            get;
            private set;
        }
    }
}
