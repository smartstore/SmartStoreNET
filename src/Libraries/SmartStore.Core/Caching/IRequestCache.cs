using System;
using System.Collections.Generic;

namespace SmartStore.Core.Caching
{
    /// <summary>
    /// Request cache interface
    /// </summary>
    public interface IRequestCache
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
		/// <returns>Cached item value</returns>
		T Get<T>(string key, Func<T> acquirer);

		/// <summary>
		/// Adds a cache item with the specified key
		/// </summary>
		/// <param name="key">Key</param>
		/// <param name="value">Value</param>
		void Put(string key, object value);

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

	/// <summary>
	/// For testing purposes
	/// </summary>
	public class NullRequestCache : IRequestCache
	{
		private static readonly IRequestCache s_instance = new NullRequestCache();

		public static IRequestCache Instance
		{
			get { return s_instance; }
		}

		public void Clear()
		{
		}

		public bool Contains(string key)
		{
			return false;
		}

		public T Get<T>(string key)
		{
			return default(T);
		}

		public T Get<T>(string key, Func<T> acquirer)
		{
			if (acquirer == null)
			{
				return default(T);
			}
			return acquirer();
		}

		public void Remove(string key)
		{
		}

		public void RemoveByPattern(string pattern)
		{
		}

		public void Put(string key, object value)
		{
		}
	}
}
