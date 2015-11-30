using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using SmartStore.Core.Fakes;

namespace SmartStore.Core.Caching
{
    
    public partial class AspNetCache : ICache
    {
        private const string REGION_NAME = "$$SmartStoreNET$$";

		// AspNetCache object does not have a ContainsKey() method:
		// Therefore we put a special string into cache if value is null,
		// otherwise our 'Contains()' would always return false,
		// which is bad if we intentionally wanted to save NULL values.
		private const string FAKE_NULL = "__[NULL]__";

        public IEnumerable<KeyValuePair<string, object>> Entries
        {
            get
            {
                if (HttpRuntime.Cache == null)
                    return Enumerable.Empty<KeyValuePair<string, object>>();

				return from entry in HttpRuntime.Cache.Cast<DictionaryEntry>()
                       let key = entry.Key.ToString()
                       where key.StartsWith(REGION_NAME)
                       select new KeyValuePair<string, object>(
                           key.Substring(REGION_NAME.Length),
                           entry.Value);
            }
        }

		public object Get(string key)
        {
			if (HttpRuntime.Cache == null)
                return null;

			var value = HttpRuntime.Cache.Get(BuildKey(key));

			if (value.Equals(FAKE_NULL))
				return null;

			return value;
        }

		public void Set(string key, object value, int? cacheTime)
		{
			if (HttpRuntime.Cache == null)
				return;
			
			key = BuildKey(key);

			var absoluteExpiration = Cache.NoAbsoluteExpiration;
			if (cacheTime.HasValue)
			{
				var span = cacheTime.Value == 0 ? TimeSpan.FromMilliseconds(10) : TimeSpan.FromMinutes(cacheTime.Value);
				absoluteExpiration = DateTime.UtcNow + TimeSpan.FromMinutes(cacheTime.Value);
			}

			HttpRuntime.Cache.Insert(key, value ?? FAKE_NULL, null, absoluteExpiration, Cache.NoSlidingExpiration);
		}

        public bool Contains(string key)
        {
			if (HttpRuntime.Cache == null)
                return false;

			return HttpRuntime.Cache.Get(BuildKey(key)) != null;
        }

        public void Remove(string key)
        {
			if (HttpRuntime.Cache == null)
                return;

			HttpRuntime.Cache.Remove(BuildKey(key));
        }

        public static string BuildKey(string key)
        {
            return key.HasValue() ? REGION_NAME + key : null;
        }

		public bool IsSingleton
		{
			// because Asp.NET Cache is thread-safe by itself,
			// no need to mess up with locks.
			get { return false; }
		}

    }

}
