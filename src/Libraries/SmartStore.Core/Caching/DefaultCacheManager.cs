using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SmartStore.Core.Caching
{

    public partial class DefaultCacheManager : ICacheManager
    {
        private readonly ICache _cache;

        public DefaultCacheManager(ICache cache)
        {
            this._cache = cache;
        }

        public T Get<T>(string key, Func<T> acquirer, int? cacheTime = 60)
        {
            return _cache.Get(key, acquirer, cacheTime);
        }

        //public T Get<T>(string key)
        //{
        //    return default(T);
        //}

        //public void Set(string key, object data, int cacheTime)
        //{
        //    // ...
        //}

        public bool Contains(string key)
        {
            return _cache.Contains(key);
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }

        public void RemoveByPattern(string pattern)
        {
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
    }
}