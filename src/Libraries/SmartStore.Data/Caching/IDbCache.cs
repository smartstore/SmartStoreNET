using System;
using System.Collections.Generic;

namespace SmartStore.Data.Caching
{
	/// <summary>
	/// Interface to be implemented by cache implementations.
	/// </summary>
	public interface IDbCache
	{
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
		void Put(string key, object value, IEnumerable<string> dependentEntitySets, TimeSpan? duration);

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
		/// Deletes all items from cache
		/// </summary>
		void Clear();
	}

	internal class NullDbCache : IDbCache
	{
		public bool TryGet(string key, out object value)
		{
			value = null;
			return false;
		}

		public void Put(string key, object value, IEnumerable<string> dependentEntitySets, TimeSpan? duration)
		{
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
	}
}
