using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

namespace SmartStore.Core.Caching
{
    
    public partial class StaticCache : ICache
    {
        private const string REGION_NAME = "$$SmartStoreNET$$";
        private readonly static object s_lock = new object();

        protected ObjectCache Cache
        {
            get
            {
                return MemoryCache.Default;
            }
        }

        public IEnumerable<KeyValuePair<string, object>> Entries
        {
            get
            {
                return from entry in Cache
                       where entry.Key.StartsWith(REGION_NAME)
                       select new KeyValuePair<string, object>(
                           entry.Key.Substring(REGION_NAME.Length), 
                           entry.Value);
            }
        }

        public T Get<T>(string key, Func<T> acquirer, int? cacheTime)
        {
            key = BuildKey(key);

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
            return Cache.Contains(BuildKey(key));
        }

        public void Remove(string key)
        {
            Cache.Remove(BuildKey(key));
        }

        private string BuildKey(string key)
        {
            return key.HasValue() ? REGION_NAME + key : null;
        }

    }

}
