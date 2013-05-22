using System;
namespace SmartStore.Core.Caching
{
    /// <summary>
    /// Represents a null cache
    /// </summary>
    public partial class NullCache : ICacheManager
    {

        public T Get<T>(string key, Func<T> acquirer, int? cacheTime = 60)
        {
            return acquirer();
        }

        /// <summary>
        /// Gets a value indicating whether the value associated with the specified key is cached
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>Result</returns>
        public bool Contains(string key)
        {
            return false;
        }

        /// <summary>
        /// Removes the value with the specified key from the cache
        /// </summary>
        /// <param name="key">/key</param>
        public void Remove(string key)
        {
        }

        /// <summary>
        /// Removes items by pattern
        /// </summary>
        /// <param name="pattern">pattern</param>
        public void RemoveByPattern(string pattern)
        {
        }

        /// <summary>
        /// Clear all cache data
        /// </summary>
        public void Clear()
        {
        }
    }
}