using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartStore.Core.Caching
{
    /// <summary>
    /// Cache manager interface
    /// </summary>
    public interface ICacheManager
    {
		/// <summary>
		/// Gets a cache item associated with the specified key
		/// </summary>
		/// <typeparam name="T">The type of the item to get</typeparam>
		/// <param name="key">The cache item key</param>
		/// <returns>Cached item value or <c>null</c> if item with specified key does not exist in the cache</returns>
		T Get<T>(string key);

		/// <summary>
		/// Gets a cache item associated with the specified key or adds the item
		/// if it doesn't exist in the cache.
		/// </summary>
		/// <typeparam name="T">The type of the item to get or add</typeparam>
		/// <param name="key">The cache item key</param>
		/// <param name="acquirer">Func which returns value to be added to the cache</param>
		/// <param name="duration">Absolute expiration time</param>
		/// <returns>Cached item value</returns>
		T Get<T>(string key, Func<T> acquirer, TimeSpan? duration = null);

		/// <summary>
		/// Gets a cache item associated with the specified key or - if it doesn't exist in the cache -  
		/// adds the item obtained from the asynchronous acquirer .
		/// </summary>
		/// <typeparam name="T">The type of the item to get or add</typeparam>
		/// <param name="key">The cache item key</param>
		/// <param name="acquirer">Func which returns value to be added to the cache</param>
		/// <param name="duration">Absolute expiration time</param>
		/// <returns>Cached item value</returns>
		Task<T> GetAsync<T>(string key, Func<Task<T>> acquirer, TimeSpan? duration = null);

		/// <summary>
		/// Gets a hashset associated with the specified key or. If key does not exist,
		/// a new set is created and put to cache automatically
		/// </summary>
		/// <param name="key">The set cache item key</param>
		/// <returns>Cached item value</returns>
		ISet GetHashSet(string key);

		/// <summary>
		/// Adds a cache item with the specified key
		/// </summary>
		/// <param name="key">Key</param>
		/// <param name="value">Value</param>
		/// <param name="duration">Absolute expiration time</param>
		void Put(string key, object value, TimeSpan? duration = null);

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
		/// Scans for all keys matching the input pattern 
		/// </summary>
		/// <param name="pattern">A key pattern. Can be <c>null</c>.</param>
		/// <returns>An array of matching key names</returns>
		/// <remarks>
		/// Supported glob-style patterns:
		/// - h?llo matches hello, hallo and hxllo
		/// - h*llo matches hllo and heeeello
		/// - h[ae]llo matches hello and hallo, but not hillo
		/// - h[^e]llo matches hallo, hbllo, ... but not hello
		/// - h[a-b]llo matches hallo and hbllo
		/// </remarks>
		IEnumerable<string> Keys(string pattern);

		/// <summary>
		/// Removes all entries with keys matching the input pattern
		/// </summary>
		/// <param name="pattern">Glob pattern</param>
		/// <returns>Count of removed cache items</returns>
		/// <remarks>
		/// Supported glob-style patterns:
		/// - h?llo matches hello, hallo and hxllo
		/// - h*llo matches hllo and heeeello
		/// - h[ae]llo matches hello and hallo, but not hillo
		/// - h[^e]llo matches hallo, hbllo, ... but not hello
		/// - h[a-b]llo matches hallo and hbllo
		/// </remarks>
		int RemoveByPattern(string pattern);

        /// <summary>
        /// Clear all cache data
        /// </summary>
        void Clear();

		/// <summary>
		/// Gets a value indicating whether the cache is distributed (e.g. Redis)
		/// </summary>
		bool IsDistributedCache { get; }
    }
}
