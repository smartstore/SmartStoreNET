using System;
using System.Text.RegularExpressions;
using System.Linq;
using System.Runtime.Caching;

namespace SmartStore.Core.Caching
{
    public partial class MemoryCacheManager : DisposableObject, ICacheManager
    {
		// Wwe put a special string into cache if value is null,
		// otherwise our 'Contains()' would always return false,
		// which is bad if we intentionally wanted to save NULL values.
		private const string FakeNull = "__[NULL]__";

		private readonly MemoryCache _cache;

		public MemoryCacheManager()
		{
			_cache = new MemoryCache("SmartStore");
		}

		private bool TryGet<T>(string key, out T value)
		{
			value = default(T);

			object obj = _cache.Get(key);

			if (obj != null)
			{
				if (obj.Equals(FakeNull))
				{
					return true;
				}

				value = (T)obj;
				return true;
			}

			return false;
		}

		public T Get<T>(string key)
		{
			T value;
			TryGet(key, out value);
			
			return value;
		}

        public T Get<T>(string key, Func<T> acquirer, TimeSpan? duration = null)
        {
			T value;

			if (TryGet(key, out value))
			{
				return value;
			}

			lock (_cache)
			{
				// atomic operation must be outer locked
				if (!TryGet(key, out value))
				{
					value = acquirer();
					Set(key, value, duration);
					return value;
				}
			}

			return value;
		}

		public void Set(string key, object value, TimeSpan? duration = null)
		{
			_cache.Set(key, value ?? FakeNull, GetCacheItemPolicy(duration));
		}

        public bool Contains(string key)
        {
			return _cache.Contains(key);
        }

        public void Remove(string key)
        {
			_cache.Remove(key);
        }

		public string[] Keys(string pattern)
		{
			Guard.ArgumentNotEmpty(() => pattern);

			var keys = _cache.AsParallel().Select(x => x.Key);

			if (pattern.IsEmpty() || pattern == "*")
			{
				return keys.ToArray();
			}

			var matcher = CreateMatcher(pattern);

			return keys.Where(x => matcher.IsMatch(x)).ToArray();
		}

		public void RemoveByPattern(string pattern)
        {
            var keysToRemove = Keys(pattern);

			lock (_cache)
			{
				// lock atomic operation
				foreach (string key in keysToRemove)
				{
					_cache.Remove(key);
				}
			}
        }

        public void Clear()
        {
			RemoveByPattern("*");
        }

		private static Regex CreateMatcher(string pattern)
		{
			return new Regex(pattern, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
		}

		private CacheItemPolicy GetCacheItemPolicy(TimeSpan? duration)
		{
			var absoluteExpiration = ObjectCache.InfiniteAbsoluteExpiration;

			if (duration.HasValue)
			{
				absoluteExpiration = DateTime.UtcNow + duration.Value;
			}

			var cacheItemPolicy = new CacheItemPolicy
			{
				AbsoluteExpiration = absoluteExpiration,
				SlidingExpiration = ObjectCache.NoSlidingExpiration
			};

			return cacheItemPolicy;
		}


		protected override void OnDispose(bool disposing)
		{
			if (disposing)
				_cache.Dispose();
		}
	}
}