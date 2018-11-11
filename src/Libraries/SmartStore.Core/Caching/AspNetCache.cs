﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

namespace SmartStore.Core.Caching
{
    
    public partial class AspNetCache : ICache
    {
        private const string RegionName = "$$SmartStoreNET$$";

        public IEnumerable<KeyValuePair<string, object>> Entries
        {
            get
            {
                if (HttpRuntime.Cache == null)
                    return Enumerable.Empty<KeyValuePair<string, object>>();

				return from entry in HttpRuntime.Cache.Cast<DictionaryEntry>()
                       let key = entry.Key.ToString()
                       where key.StartsWith(RegionName)
                       select new KeyValuePair<string, object>(
                           key.Substring(RegionName.Length),
                           entry.Value);
            }
        }

		public object Get(string key)
        {
			if (HttpRuntime.Cache == null)
                return null;

			return HttpRuntime.Cache.Get(BuildKey(key));
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

			HttpRuntime.Cache.Insert(key, value, null, absoluteExpiration, Cache.NoSlidingExpiration);
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
            return key.HasValue() ? RegionName + key : null;
        }

		public bool IsSingleton
		{
			// because Asp.NET Cache is thread-safe by itself,
			// no need to mess up with locks.
			get { return false; }
		}

    }

}
