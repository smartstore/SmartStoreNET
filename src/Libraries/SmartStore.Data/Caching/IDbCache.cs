using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartStore.Data.Caching
{
    /// <summary>
    /// Interface to be implemented by cache implementations.
    /// </summary>
    public interface IDbCache
    {
        /// <summary>
        /// Controls whether query caching is enabled or idle
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Tries to the get cached entry by key.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The retrieved value.</param>
        /// <returns>A value of <c>true</c> if entry was found in the cache, <c>false</c> otherwise.</returns>
        bool TryGet(string key, out object value);

        /// <summary>
        /// Adds the specified entry to the cache.
        /// </summary>
        /// <param name="key">The entry key.</param>
        /// <param name="value">The entry value.</param>
        /// <param name="dependentEntitySets">The list of dependent entity sets.</param>
        /// <param name="duration">The absolute expiration.</param>
        DbCacheEntry Put(string key, object value, IEnumerable<string> dependentEntitySets, TimeSpan? duration);

        /// <summary>
        /// Invalidates all cache entries which are dependent on any of the specified entity sets.
        /// </summary>
        /// <param name="entitySets">The entity sets.</param>
        void InvalidateSets(IEnumerable<string> entitySets);

        /// <summary>
        /// Invalidates cache entry with a given key.
        /// </summary>
        /// <param name="key">The cache key.</param>
        void InvalidateItem(string key);

        /// <summary>
        /// Deletes all items from static and request cache
        /// </summary>
        void Clear();

        /// <summary>
        /// Tries to the get cached entry by key from (HTTP) request scope
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The retrieved value.</param>
        /// <returns>A value of <c>true</c> if entry was found in the cache, <c>false</c> otherwise.</returns>
        bool RequestTryGet(string key, out DbCacheEntry value);

        /// <summary>
        /// Adds the specified entry to the request scoped cache.
        /// </summary>
        /// <param name="key">The entry key.</param>
        /// <param name="value">The entry value.</param>
        /// <param name="dependentEntitySets">The list of dependent entity sets.</param>
        DbCacheEntry RequestPut(string key, object value, string[] dependentEntitySets);

        /// <summary>
        /// Invalidates all request scoped cache entries which are dependent on any of the specified entity sets.
        /// </summary>
        /// <param name="entitySets">The entity sets.</param>
        void RequestInvalidateSets(IEnumerable<string> entitySets);

        /// <summary>
        /// Invalidates request scoped cache entry with a given key.
        /// </summary>
        /// <param name="key">The cache key.</param>
        void RequestInvalidateItem(string key);
    }

    public class NullDbCache : IDbCache
    {
        public bool Enabled
        {
            get;
            set;
        }

        public bool TryGet(string key, out object value)
        {
            value = null;
            return false;
        }

        public DbCacheEntry Put(string key, object value, IEnumerable<string> dependentEntitySets, TimeSpan? duration)
        {
            return new DbCacheEntry { Key = key, Value = value, CachedOnUtc = DateTime.UtcNow, EntitySets = dependentEntitySets.ToArray(), Duration = duration };
        }

        public void InvalidateSets(IEnumerable<string> entitySets)
        {
        }

        public void InvalidateItem(string key)
        {
        }

        public void Clear()
        {
        }

        public bool RequestTryGet(string key, out DbCacheEntry value)
        {
            value = null;
            return false;
        }

        public DbCacheEntry RequestPut(string key, object value, string[] dependentEntitySets)
        {
            return new DbCacheEntry { Key = key, Value = value, CachedOnUtc = DateTime.UtcNow, EntitySets = dependentEntitySets.ToArray() };
        }

        public void RequestInvalidateSets(IEnumerable<string> entitySets)
        {
        }

        public void RequestInvalidateItem(string key)
        {
        }
    }
}
