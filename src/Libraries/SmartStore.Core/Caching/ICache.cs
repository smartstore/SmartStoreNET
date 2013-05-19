using System;
using System.Collections.Generic;

namespace SmartStore.Core.Caching
{
    
    /// <summary>
    /// Cache holder interface
    /// </summary>
    public interface ICache
    {

        /// <summary>
        /// Gets all entries in the cache
        /// </summary>
        IEnumerable<KeyValuePair<string, object>> Entries { get; }

        /// <summary>
        /// Gets a cache item associated with the specified key or adds the item
        /// if it doesn't exist in the cache.
        /// </summary>
        /// <typeparam name="T">The type of the item to get or add</typeparam>
        /// <param name="key">The cache item key</param>
        /// <param name="acquirer">Func which returns value to be added to the cache</param>
        /// <param name="cacheTime">Expiration time in minutes</param>
        /// <returns>Cached item value</returns>
        T Get<T>(string key, Func<T> acquirer, int? cacheTime);

        /// <summary>
        /// Gets a value indicating whether an item associated with the specified key exists in the cache
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>Result</returns>
        bool Contains(string key);

        /// <summary>
        /// Removes the value with the specified key from the cache
        /// </summary>
        /// <param name="key">/key</param>
        void Remove(string key);

    }

}
