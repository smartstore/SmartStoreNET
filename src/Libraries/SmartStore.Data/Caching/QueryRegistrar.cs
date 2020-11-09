using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;

namespace SmartStore.Data.Caching
{
    internal class QueryRegistrar
    {
        private readonly ConcurrentDictionary<MetadataWorkspace, HashSet<string>> _queries = new ConcurrentDictionary<MetadataWorkspace, HashSet<string>>();

        public void AddQuery(MetadataWorkspace workspace, string sql)
        {
            Guard.NotNull(workspace, nameof(workspace));
            Guard.NotEmpty(sql, nameof(sql));

            var queries = _queries.GetOrAdd(workspace, new HashSet<string>());
            lock (queries)
            {
                queries.Add(sql);
            }
        }

        public bool RemoveQuery(MetadataWorkspace workspace, string sql)
        {
            Guard.NotNull(workspace, nameof(workspace));
            Guard.NotEmpty(sql, nameof(sql));

            HashSet<string> queries;
            if (_queries.TryGetValue(workspace, out queries))
            {
                lock (queries)
                {
                    return queries.Remove(sql);
                }
            }

            return false;
        }

        public bool ContainsQuery(MetadataWorkspace workspace, string sql)
        {
            Guard.NotNull(workspace, nameof(workspace));
            Guard.NotEmpty(sql, nameof(sql));

            HashSet<string> queries;
            if (_queries.TryGetValue(workspace, out queries))
            {
                return queries.Contains(sql);
            }

            return false;
        }
    }
}
