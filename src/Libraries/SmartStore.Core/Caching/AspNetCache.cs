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
        private readonly HttpContextBase _context;

        public AspNetCache(HttpContextBase context)
        {
            this._context = context;
        }

        public IEnumerable<KeyValuePair<string, object>> Entries
        {
            get
            {
                if (_context is FakeHttpContext)
                    return Enumerable.Empty<KeyValuePair<string, object>>();

                return from entry in _context.Cache.Cast<DictionaryEntry>()
                       let key = entry.Key.ToString()
                       where key.StartsWith(REGION_NAME)
                       select new KeyValuePair<string, object>(
                           key.Substring(REGION_NAME.Length),
                           entry.Value);
            }
        }

		public object Get(string key)
        {
            if (_context is FakeHttpContext)
                return null;
            
			return _context.Cache.Get(BuildKey(key));
        }

		public void Set(string key, object value, int? cacheTime)
		{
			key = BuildKey(key);

			var absoluteExpiration = Cache.NoAbsoluteExpiration;
			if (cacheTime.GetValueOrDefault() > 0)
			{
				absoluteExpiration = DateTime.UtcNow + TimeSpan.FromMinutes(cacheTime.Value);
			}

			_context.Cache.Insert(key, value, null, absoluteExpiration, Cache.NoSlidingExpiration);
		}

        public bool Contains(string key)
        {
            if (_context is FakeHttpContext)
                return false;
            
            return _context.Cache.Get(BuildKey(key)) != null;
        }

        public void Remove(string key)
        {
            if (_context is FakeHttpContext)
                return;
            
            _context.Cache.Remove(BuildKey(key));
        }

        public static string BuildKey(string key)
        {
            return key.HasValue() ? REGION_NAME + key : null;
        }

		public bool IsSingleton
		{
			get { return false; }
		}

    }

}
