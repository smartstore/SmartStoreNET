using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SmartStore.Core.Caching;
using SmartStore.Core.Infrastructure;

namespace SmartStore.DevTools.OutputCache
{
	public class MemoryOutputCacheProvider
	{
		private static readonly ConcurrentDictionary<string, HashSet<string>> _keysLookup;

		private readonly HttpContextBase _httpContext;

		static MemoryOutputCacheProvider()
		{
			_keysLookup = new ConcurrentDictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
		}

		public MemoryOutputCacheProvider()
		{
			_httpContext = EngineContext.Current.Resolve<HttpContextBase>();
		}

		public OutputCacheItem Get(string key)
		{
			return _httpContext.Cache.Get(key) as OutputCacheItem;
		}

		public void Set(string key, OutputCacheItem item)
		{
			_httpContext.Cache.Add(
				key,
				item,
				null,
				item.ValidUntilUtc,
				System.Web.Caching.Cache.NoSlidingExpiration,
				System.Web.Caching.CacheItemPriority.Normal,
				null);

			// add cache key to tag lookup
			if (item.Tags != null && item.Tags.Length > 0)
			{
				foreach (var tag in item.Tags)
				{
					AddCacheKeyToLookup("tag:" + tag, item.CacheKey);
				}
			}

			// add cache key to route lookup
			AddCacheKeyToLookup("route:" + item.RouteKey, item.CacheKey);
		}

		private void AddCacheKeyToLookup(string lookupKey, string cacheKey)
		{
			var set = _keysLookup.GetOrAdd(lookupKey, x => new HashSet<string>());
			lock (set)
			{
				set.Add(cacheKey);
			}
		}

		public bool Exists(string key)
		{
			return _httpContext.Cache.Get(key) != null;
		}

		public void Remove(params string[] keys)
		{
			RemoveInternal(keys, true);
		}

		public void RemoveAll()
		{
			var items = All(0, 100).ToList();
			while (items.Any())
			{
				var cacheKeys = items.Select(x => x.CacheKey).ToArray();
				RemoveInternal(cacheKeys, false);
				items = All(0, 100).ToList();
			}

			_keysLookup.Clear();
		}

		public void RemoveInternal(string[] keys, bool removeFromLookup)
		{
			foreach (var key in keys)
			{
				_httpContext.Cache.Remove(key);
			}

			if (removeFromLookup)
			{
				foreach (var set in _keysLookup.Values)
				{
					lock (set)
					{
						keys.Each(x => set.Remove(x));
					}
				}
			}
		}

		public IEnumerable<OutputCacheItem> All(int skip, int count)
		{
			return _httpContext.Cache.AsParallel()
				.Cast<DictionaryEntry>()
				.Select(x => x.Value)
				.OfType<OutputCacheItem>()
				.Skip(skip)
				.Take(count);
		}

		public int Count()
		{
			return _httpContext.Cache.AsParallel()
				.Cast<DictionaryEntry>()
				.Select(x => x.Value)
				.OfType<OutputCacheItem>()
				.Count();
		}

		public int InvalidateByRoute(params string[] routes)
		{
			int count = 0;

			foreach (var route in routes)
			{
				count += InvalidateByLookupKey("route:" + route);
			}

			return count;
		}

		public int InvalidateByTag(params string[] tags)
		{
			int count = 0;

			foreach (var tag in tags)
			{
				count += InvalidateByLookupKey("tag:" + tag);
			}

			return count;
		}

		private int InvalidateByLookupKey(string lookupKey)
		{
			HashSet<string> set;
			if (_keysLookup.TryRemove(lookupKey, out set))
			{
				RemoveInternal(set.ToArray(), true);
				return set.Count;
			}

			return 0;
		}
	}
}