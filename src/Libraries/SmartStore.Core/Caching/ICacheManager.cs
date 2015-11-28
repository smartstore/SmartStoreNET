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

		/// <summary>
		/// Adds a cache item with the specified key
		/// </summary>
		/// <param name="key">Key</param>
		/// <param name="value">Value</param>
		/// <param name="cacheTime">Cache time in minutes</param>
		void Set(string key, object value, int? cacheTime = null);

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

		/// <summary>
		/// Returns a wrapped sync lock for the underlying <c>ICache</c> implementation
		/// </summary>
		/// <returns>The disposable sync lock</returns>
		/// <remarks>
		/// This method internally wraps either a <c>ReaderWriterLockSlim</c> or an empty noop action
		/// dependending on the scope of the underlying <c>ICache</c> implementation.
		/// The static (singleton) cache always returns the <c>ReaderWriterLockSlim</c> instance
		/// which is used to sync read/write access to cache items.
		/// This method is useful if you want to modify a cache item's value, thus must lock access
		/// to the cache during the update.
		/// </remarks>
		IDisposable EnterWriteLock();
    }
}
