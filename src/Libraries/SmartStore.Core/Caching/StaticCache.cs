using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

namespace SmartStore.Core.Caching
{
    
    public partial class StaticCache : ICache
    {
        private readonly static object s_lock = new object();
		private ObjectCache _cache;

        protected ObjectCache Cache
        {
            get
            {
				if (_cache == null)
				{
					_cache = new MemoryCache("SmartStore");
				}
				return _cache;
            }
        }

        public IEnumerable<KeyValuePair<string, object>> Entries
        {
            get
            {
				return Cache;
            }
        }

        public T Get<T>(string key, Func<T> acquirer, int? cacheTime)
        {
            if (Cache.Contains(key))
            {
                return (T)Cache.Get(key);
            }
            else
            {
				lock (s_lock)
                { 
                    if (!Cache.Contains(key))
                    {
						var value = acquirer();
                        if (value != null)
                        {
                            var cacheItem = new CacheItem(key, value);
                            CacheItemPolicy policy = null;
                            if (cacheTime.GetValueOrDefault() > 0)
                            {
                                policy = new CacheItemPolicy { AbsoluteExpiration = DateTime.Now + TimeSpan.FromMinutes(cacheTime.Value) };
                            }

                            Cache.Add(cacheItem, policy);
                        }

                        return value;
                    }
                }

                return (T)Cache.Get(key);
            }
        }

        public bool Contains(string key)
        {
            return Cache.Contains(key);
        }

        public void Remove(string key)
        {
            Cache.Remove(key);
        }

    }

}
