using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core.Caching;
using SmartStore.Core.Domain.Logging;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Domain.Tasks;
using SmartStore.Core.Infrastructure.DependencyManagement;

namespace SmartStore.Data.Caching
{
	public partial class EfDbCache : IDbCache
	{
		// Entity sets to be never cached or invalidated
		private static readonly HashSet<string> _toxicSets = new HashSet<string>
		{
			typeof(ScheduleTask).Name,
			typeof(Log).Name,
			typeof(ActivityLog).Name,
			typeof(QueuedEmail).Name
		};

		private const string KEYPREFIX = "efcache:*";
		private readonly object _lock = new object();

		private bool _enabled;

		private readonly ICacheManager _cache;
		private readonly Work<IRequestCache> _requestCache;

		public EfDbCache(ICacheManager innerCache, Work<IRequestCache> requestCache)
		{
			_cache = innerCache;
			_requestCache = requestCache;

			_enabled = true;
		}

		public bool Enabled
		{
			get
			{
				return _enabled;
			}
			set
			{
				if (_enabled == value)
					return;

				lock (_lock)
				{
					if (_enabled == false && value == true)
					{
						// When cache was disabled previously and gets enabled,
						// we should clear the cache, because no invalidation has been performed
						// during disabled state. We would deal with stale data otherwise.
						Clear();
					}
					_enabled = value;
				}
			}
		}

		private bool IsToxic(IEnumerable<string> entitySets)
		{
			return entitySets.Any(x => _toxicSets.Contains(x));
		}

		#region Request Scoped

		public virtual bool RequestTryGet(string key, out DbCacheEntry value)
		{
			value = null;

			if (!Enabled)
			{
				return false;
			}

			key = HashKey(key);

			value = _requestCache.Value.Get<DbCacheEntry>(key);
			return value != null;
		}

		public virtual DbCacheEntry RequestPut(string key, object value, string[] dependentEntitySets)
		{
			if (!Enabled || IsToxic(dependentEntitySets))
			{
				return null;
			}

			key = HashKey(key);

			var entry = new DbCacheEntry
			{
				Key = key,
				Value = value,
				EntitySets = dependentEntitySets,
				CachedOnUtc = DateTime.UtcNow
			};

			_requestCache.Value.Put(key, entry);

			foreach (var entitySet in entry.EntitySets)
			{
				var lookup = RequestGetLookupSet(entitySet);
				lookup.Add(key);
			}

			return entry;
		}

		public virtual void RequestInvalidateSets(IEnumerable<string> entitySets)
		{
			Guard.NotNull(entitySets, nameof(entitySets));

			if (!Enabled || !entitySets.Any() || IsToxic(entitySets))
				return;

			var sets = entitySets.Distinct().ToArray();

			var itemsToInvalidate = new HashSet<string>();

			foreach (var entitySet in sets)
			{
				var lookup = RequestGetLookupSet(entitySet, false);
				if (lookup != null)
				{
					itemsToInvalidate.UnionWith(lookup);
				}
			}

			foreach (var key in itemsToInvalidate)
			{
				RequestInvalidateItem(key);
			}
		}

		public virtual void RequestInvalidateItem(string key)
		{
			Guard.NotEmpty(key, nameof(key));

			var cache = _requestCache.Value;

			var entry = cache.Get<DbCacheEntry>(key);
			if (entry != null)
			{
				// remove item itself from cache
				cache.Remove(key);

				// remove this key in all lookups
				foreach (var set in entry.EntitySets)
				{
					var lookup = RequestGetLookupSet(set, false);
					if (lookup != null)
					{
						lookup.Remove(entry.Key);
					}
				}
			}
		}

		private HashSet<string> RequestGetLookupSet(string entitySet, bool create = true)
		{
			var key = GetLookupKeyFor(entitySet);

			if (create)
			{
				return _requestCache.Value.Get(key, () => new HashSet<string>());
			}
			else
			{
				return _requestCache.Value.Get<HashSet<string>>(key);
			}
		}

		#endregion

		public virtual bool TryGet(string key, out object value)
		{
			value = null;

			if (!Enabled)
			{
				return false;
			}

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

		public virtual DbCacheEntry Put(string key, object value, IEnumerable<string> dependentEntitySets, TimeSpan? duration)
		{
			if (!Enabled || IsToxic(dependentEntitySets))
			{
				return null;
			}

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

				return entry;
			}
		}

		public void Clear()
		{
			_cache.RemoveByPattern(KEYPREFIX);
			_requestCache.Value.RemoveByPattern(KEYPREFIX);
		}

		public virtual void InvalidateSets(IEnumerable<string> entitySets)
		{
			Guard.NotNull(entitySets, nameof(entitySets));

			if (!Enabled || !entitySets.Any() || IsToxic(entitySets))
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
			if (!Enabled)
			{
				return;
			}

			Guard.NotEmpty(key, nameof(key));

			lock (String.Intern(key))
			{
				InvalidateItemUnlocked(key);
			}
		}

		private void InvalidateItemUnlocked(string key)
		{
			if (!Enabled)
			{
				return;
			}

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
				try
				{
					return KEYPREFIX + "data:" + Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(key)));
				}
				catch
				{
					return KEYPREFIX + "data:" + key;
				}
			}
		}
	}
}
