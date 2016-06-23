using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Linq;
using SmartStore.Utilities;
using SmartStore.Utilities.Threading;

namespace SmartStore.Core.Caching
{
    public partial class CacheManager<TCache> : ICacheManager where TCache : ICache
    {
		private readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();
		private readonly ICache _cache;

		// Wwe put a special string into cache if value is null,
		// otherwise our 'Contains()' would always return false,
		// which is bad if we intentionally wanted to save NULL values.
		private const string FakeNull = "__[NULL]__";

		public CacheManager(Func<Type, ICache> fn)
        {
            this._cache = fn(typeof(TCache));
        }

		public T Get<T>(string key)
		{
			Guard.ArgumentNotNull(() => key);

			if (_cache.Contains(key))
			{
				return GetExisting<T>(key);
			}

			return default(T);
		}

        public T Get<T>(string key, Func<T> acquirer, int? cacheTime = null)
        {
			Guard.ArgumentNotEmpty(() => key);
			
			if (_cache.Contains(key))
			{
				return GetExisting<T>(key);
			}

			using (EnterReadLock())
			{
				if (!_cache.Contains(key))
				{
					var value = acquirer();
					this.Set(key, value, cacheTime);

					return value;
				}
			}

			return GetExisting<T>(key);
		}

		private T GetExisting<T>(string key)
		{
			var value = _cache.Get(key);

			if (value.Equals(FakeNull))
				return default(T);

			return (T)_cache.Get(key);
		}

		public void Set(string key, object value, int? cacheTime = null)
		{
			Guard.ArgumentNotEmpty(() => key);

			using (EnterWriteLock())
			{
				_cache.Set(key, value ?? FakeNull, cacheTime);
			}
		}

        public bool Contains(string key)
        {
			Guard.ArgumentNotEmpty(() => key);
			
			return _cache.Contains(key);
        }

        public void Remove(string key)
        {
			Guard.ArgumentNotEmpty(() => key);

			using (EnterWriteLock())
			{
				_cache.Remove(key);
			}
        }

		public IEnumerable<string> Keys(string pattern)
		{
			var keys = _cache.Entries.Select(x => x.Key);

			if (pattern.IsEmpty())
			{
				return keys;
			}

			var matcher = CreateMatcher(pattern);

			return keys.Where(x => matcher.IsMatch(x));
		}

		public void RemoveByPattern(string pattern)
        {
			Guard.ArgumentNotEmpty(() => pattern);

            var keysToRemove = Keys(pattern).ToArray();

			using (EnterWriteLock())
			{
				foreach (string key in keysToRemove)
				{
					_cache.Remove(key);
				}
			}
        }

        public void Clear()
        {
            var keysToRemove = Keys(null).ToArray();

			using (EnterWriteLock())
			{
				foreach (string key in keysToRemove)
				{
					_cache.Remove(key);
				}
			}
        }

		private static Regex CreateMatcher(string pattern)
		{
			return new Regex(pattern, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
		}

		private IDisposable EnterReadLock()
		{
			return _cache.IsSingleton ? _rwLock.GetUpgradeableReadLock() : ActionDisposable.Empty;
		}

		public IDisposable EnterWriteLock()
		{
			return _cache.IsSingleton ? _rwLock.GetWriteLock() : ActionDisposable.Empty;
		}

	}
}