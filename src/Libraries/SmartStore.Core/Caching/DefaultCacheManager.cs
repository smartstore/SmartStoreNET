using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using SmartStore.Utilities;
using SmartStore.Utilities.Threading;

namespace SmartStore.Core.Caching
{

	public static class ICacheManagerExtensions
	{
		public static T Get<T>(this ICacheManager cacheManager, string key)
		{
			return cacheManager.Get<T>(key, () => { return default(T); });
		}
	}

    public partial class CacheManager<TCache> : ICacheManager where TCache : ICache
    {
		private readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();
		private readonly ICache _cache;

        public CacheManager(Func<Type, ICache> fn)
        {
            this._cache = fn(typeof(TCache));
        }

        public T Get<T>(string key, Func<T> acquirer, int? cacheTime = null)
        {
			Guard.ArgumentNotEmpty(() => key);
			
			if (_cache.Contains(key))
			{
				return (T)_cache.Get(key);
			}
			else
			{
				using (GetReadLock())
				{
					if (!_cache.Contains(key))
					{
						var value = acquirer();
						this.Set(key, value, cacheTime);

						return value;
					}
				}

				return (T)_cache.Get(key);
			}
        }

		public void Set(string key, object value, int? cacheTime = null)
		{
			Guard.ArgumentNotEmpty(() => key);
			
			if (value == null)
				return;

			using (GetWriteLock())
			{
				_cache.Set(key, value, cacheTime);
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
			
			_cache.Remove(key);
        }

        public void RemoveByPattern(string pattern)
        {
			Guard.ArgumentNotEmpty(() => pattern);
			
			var regex = new Regex(pattern, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var keysToRemove = new List<String>();

            foreach (var item in _cache.Entries)
            {
                if (regex.IsMatch(item.Key))
                {
                    keysToRemove.Add(item.Key);
                }
            }

            foreach (string key in keysToRemove)
            {
                _cache.Remove(key);
            }
        }

        public void Clear()
        {
            var keysToRemove = new List<string>();
            foreach (var item in _cache.Entries)
            {
                keysToRemove.Add(item.Key);
            }

            foreach (string key in keysToRemove)
            {
                _cache.Remove(key);
            }
        }

		private IDisposable GetReadLock()
		{
			return _cache.IsSingleton ? _rwLock.GetUpgradeableReadLock() : ActionDisposable.Empty;
		}

		private IDisposable GetWriteLock()
		{
			return _cache.IsSingleton ? _rwLock.GetWriteLock() : ActionDisposable.Empty;
		}

	}
}