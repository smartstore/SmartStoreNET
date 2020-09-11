using System.Data.Entity.Core.Metadata.Edm;

namespace SmartStore.Data.Caching
{
    public sealed class SingletonQueries
    {
        public static readonly SingletonQueries Current = new SingletonQueries();

        private readonly QueryRegistrar _cachedQueries = new QueryRegistrar();

        private SingletonQueries()
        {
        }

        public void AddCachedQuery(MetadataWorkspace workspace, string sql)
        {
            _cachedQueries.AddQuery(workspace, sql);
        }

        public bool RemoveCachedQuery(MetadataWorkspace workspace, string sql)
        {
            return _cachedQueries.RemoveQuery(workspace, sql);
        }

        public bool IsQueryCached(MetadataWorkspace workspace, string sql)
        {
            return _cachedQueries.ContainsQuery(workspace, sql);
        }
    }
}
