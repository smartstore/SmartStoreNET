using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using SmartStore.Collections;
using SmartStore.Core.Caching;
using SmartStore.Utilities.Threading;

namespace SmartStore.Data.Caching
{
	internal class EfCacheImpl : EFCache.ICache
	{
		private const string KEYPREFIX = "efcache:";
		private readonly Multimap<string, string> _entitySetToKey = new Multimap<string, string>((values) => new HashSet<string>(values ?? Enumerable.Empty<string>()));
		
		private readonly ICacheManager _cache;
		
		public EfCacheImpl(ICacheManager innerCache)
		{
			_cache = innerCache;
		}
		
		public bool GetItem(string key, out object value)
		{
			value = null;
			key = HashKey(key);

			if (_cache.Contains(key))
			{
				value = _cache.Get<object>(key);
				return true;
			}

			return false;
		}

		public void PutItem(string key, object value, IEnumerable<string> dependentEntitySets, TimeSpan slidingExpiration, DateTimeOffset absoluteExpiration)
		{
			key = HashKey(key);

			lock (String.Intern(key))
			{
				var now = DateTimeOffset.Now;
				var expiresInMinutes = Math.Max(1, Math.Min(int.MaxValue, (absoluteExpiration - now).TotalMinutes));
				_cache.Set(key, value, TimeSpan.FromMinutes(expiresInMinutes));

				foreach (var s in dependentEntitySets)
				{
					_entitySetToKey[s].Add(key);
				}
			}
		}

		public void InvalidateItem(string key)
		{
			key = HashKey(key);

			lock (String.Intern(key))
			{
				_cache.Remove(key);

				foreach (var p in _entitySetToKey)
					p.Value.Remove(key);
			}
		}

		public void InvalidateSets(IEnumerable<string> entitySets)
		{
			var keysToRemove = new HashSet<string>();

			lock (_entitySetToKey)
			{
				foreach (var entitySet in entitySets)
				{
					if (_entitySetToKey.ContainsKey(entitySet))
					{
						keysToRemove.AddRange(_entitySetToKey[entitySet]);
						
					}
				}

				foreach (var k in keysToRemove)
					_cache.Remove(k);

				foreach (var s in entitySets)
					_entitySetToKey.RemoveAll(s);
			}
		}

		private static string HashKey(string key)
		{
			// Looking up large Keys can be expensive (comparing Large Strings), so if keys are large, hash them, otherwise if keys are short just use as-is
			if (key.Length <= 128)
				return KEYPREFIX + key;

			using (var sha = new SHA1CryptoServiceProvider())
			{
				key = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(key)));
				return KEYPREFIX + key;
			}
		}
	}
}
