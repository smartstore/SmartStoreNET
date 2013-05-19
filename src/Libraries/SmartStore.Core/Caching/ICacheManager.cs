using System;
namespace SmartStore.Core.Caching
{
    /// <summary>
    /// Cache manager interface
    /// </summary>
    public interface ICacheManager
    {
        /// <summary>
        /// Gets a cache item associated with the specified key or adds the item
        /// if it doesn't exist in the cache.
        /// </summary>
        /// <typeparam name="T">The type of the item to get or add</typeparam>
        /// <param name="key">The cache item key</param>
        /// <param name="acquirer">Func which returns value to be added to the cache</param>
        /// <param name="cacheTime">Expiration time in minutes</param>
        /// <returns>Cached item value</returns>
        T Get<T>(string key, Func<T> acquirer, int? cacheTime = null);
        
        ///// <summary>
        ///// Gets or sets the value associated with the specified key.
        ///// </summary>
        ///// <typeparam name="T">Type</typeparam>
        ///// <param name="key">The key of the value to get.</param>
        ///// <returns>The value associated with the specified key.</returns>
        //T Get<T>(string key);

        ///// <summary>
        ///// Adds the specified key and object to the cache.
        ///// </summary>
        ///// <param name="key">key</param>
        ///// <param name="data">Data</param>
        ///// <param name="cacheTime">Cache time</param>
        //void Set(string key, object data, int cacheTime);

        /// <summary>
        /// Gets a value indicating whether the value associated with the specified key is cached
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>Result</returns>
        bool Contains(string key);

        /// <summary>
        /// Removes the value with the specified key from the cache
        /// </summary>
        /// <param name="key">/key</param>
        void Remove(string key);

        /// <summary>
        /// Removes items by pattern
        /// </summary>
        /// <param name="pattern">pattern</param>
        void RemoveByPattern(string pattern);

        /// <summary>
        /// Clear all cache data
        /// </summary>
        void Clear();
    }
}
