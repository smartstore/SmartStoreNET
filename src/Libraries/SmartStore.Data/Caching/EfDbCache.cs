using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core.Caching;
using SmartStore.Core.Infrastructure.DependencyManagement;

namespace SmartStore.Data.Caching
{
	public partial class EfDbCache : IDbCache
	{
		private const string KEYPREFIX = "efcache:";
		private readonly object _lock = new object();

		private readonly ICacheManager _cache;
		private readonly Work<IRequestCache> _requestCache;

		public EfDbCache(ICacheManager innerCache, Work<IRequestCache> requestCache)
		{
			_cache = innerCache;
			_requestCache = requestCache;
		}

		public virtual bool TryGet(string key, out object value)
		{
			value = null;

			key = HashKey(key);
			var now = DateTime.UtcNow;

			var entry = _cache.Get<DbCacheEntry>(key);

			if (entry != null)
			{
				if (entry.HasExpired(now))
				{
					lock (String.Intern(key))
					{
						InvalidateItemUnlocked(entry);
					}
				}
				else
				{
					value = entry.Value;
					return true;
				}
			}

			return false;
		}

		public virtual void Put(string key, object value, IEnumerable<string> dependentEntitySets, TimeSpan? duration)
		{
			key = HashKey(key);

			lock (String.Intern(key))
			{
				var entitySets = dependentEntitySets.Distinct().ToArray();
				var entry =  new DbCacheEntry
				{
					Key = key,
					Value = value,
					EntitySets = entitySets,
					CachedOnUtc = DateTime.UtcNow,
					Duration = duration
				};

				_cache.Put(key, entry);

				foreach (var entitySet in entitySets)
				{
					var lookup = GetLookupSet(entitySet);
					lookup.Add(key);
				}
			}
		}

		public void Clear()
		{
			_cache.RemoveByPattern(KEYPREFIX);
		}

		public virtual void InvalidateSets(IEnumerable<string> entitySets)
		{
			Guard.NotNull(entitySets, nameof(entitySets));

			if (!entitySets.Any())
				return;

			var sets = entitySets.Distinct().ToArray();

			lock (_lock)
			{
				var itemsToInvalidate = new HashSet<string>();

				foreach (var entitySet in sets)
				{
					var lookup = GetLookupSet(entitySet, false);
					if (lookup != null)
					{
						itemsToInvalidate.UnionWith(lookup);
					}
				}

				foreach (var key in itemsToInvalidate)
				{
					InvalidateItemUnlocked(key);
				}
			}
		}

		public virtual void InvalidateItem(string key)
		{
			Guard.NotEmpty(key, nameof(key));

			lock (String.Intern(key))
			{
				InvalidateItemUnlocked(key);
			}
		}

		private void InvalidateItemUnlocked(string key)
		{
			if (_cache.Contains(key))
			{
				var entry = _cache.Get<DbCacheEntry>(key);
				if (entry != null)
				{
					InvalidateItemUnlocked(entry);
				}
			}
		}

		protected void InvalidateItemUnlocked(DbCacheEntry entry)
		{
			// remove item itself from cache
			_cache.Remove(entry.Key);

			// remove this key in all lookups
			foreach (var set in entry.EntitySets)
			{
				var lookup = GetLookupSet(set, false);
				if (lookup != null)
				{
					lookup.Remove(entry.Key);
				}
			}
		}

		private ISet GetLookupSet(string entitySet, bool create = true)
		{
			var key = GetLookupKeyFor(entitySet);

			if (create)
			{
				return _cache.GetHashSet(key);
			}
			else
			{
				if (_cache.Contains(key))
				{
					return _cache.GetHashSet(key);
				}
			}

			return null;	
		}

		private string GetLookupKeyFor(string entitySet)
		{
			return KEYPREFIX + "lookup:" + entitySet;
		}

		private static string HashKey(string key)
		{
			// Looking up large Keys can be expensive (comparing Large Strings), so if keys are large, hash them, otherwise if keys are short just use as-is
			if (key.Length <= 128)
				return KEYPREFIX + "data:" + key;

			using (var sha = new SHA1CryptoServiceProvider())
			{
				key = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(key)));
				return KEYPREFIX + "data:" + key;
			}
		}
	}
}
