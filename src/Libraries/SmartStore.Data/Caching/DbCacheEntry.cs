using System;
using System.Collections.Generic;

namespace SmartStore.Data.Caching
{
    [Serializable]
    public class DbCacheEntry
    {
        public string Key { get; set; }
        public DateTime CachedOnUtc { get; set; }
        public TimeSpan? Duration { get; set; }
        public string[] EntitySets { get; set; }
        public object Value { get; set; }

        public bool HasExpired(DateTime utcNow)
        {
            if (Duration.HasValue)
            {
                return utcNow > (CachedOnUtc + Duration.Value);
            }

            return false;
        }
    }

    [Serializable]
    public class CachedRecords
    {
        public List<object[]> Records { get; set; }
        public int RecordsAffected { get; set; }
        public ColumnMetadata[] TableMetadata { get; set; }
    }

    [Serializable]
    public class ColumnMetadata
    {
        public string Name { get; set; }
        public string DataTypeName { get; set; }
        public Type DataType { get; set; }
    }
}
