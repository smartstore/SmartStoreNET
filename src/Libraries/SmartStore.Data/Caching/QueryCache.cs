using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Objects;
using SmartStore.Core;
using System.Reflection;
using System.Data.Common;
using SmartStore.Core.Caching;
using SmartStore.Core.Infrastructure.DependencyManagement;
using System.Security.Cryptography;

namespace SmartStore.Data.Caching
{
	public partial class QueryCache
	{
		const string CachePrefix = "efcache:";

		private readonly ICacheManager _cache;
		private readonly Work<IRequestCache> _requestCache;

		public QueryCache(ICacheManager cache, Work<IRequestCache> requestCache)
		{
			_cache = cache;
			_requestCache = requestCache;

			AutoInvalidationEnabled = true;
		}

		public bool AutoInvalidationEnabled
		{
			get;
			set;
		}

		public string GetCacheKey(IQueryable query, string[] tags)
		{
			var sb = new StringBuilder();

			var objectQuery = query.GetObjectQuery();

			sb.AppendLine(string.Join(";", tags));

			var commandTextAndParameters = objectQuery.GetCommandTextAndParameters();
			sb.AppendLine(commandTextAndParameters.Item1);

			foreach (DbParameter parameter in commandTextAndParameters.Item2)
			{
				sb.Append(parameter.ParameterName);
				sb.Append(";");
				sb.Append(parameter.Value);
				sb.AppendLine(";");
			}

			return HashKey(sb.ToString());
		}

		public IEnumerable<string> GetAffectedEntitySets(IQueryable query)
		{
			var objectQuery = query.GetObjectQuery();
			return objectQuery.GetAffectedEntitySets();
		}

		private static string HashKey(string key)
		{
			// Looking up large Keys can be expensive (comparing Large Strings), so if keys are large, hash them, otherwise if keys are short just use as-is
			if (key.Length <= 128)
				return CachePrefix + key;

			using (var sha = new SHA1CryptoServiceProvider())
			{
				key = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(key)));
				return CachePrefix + key;
			}
		}

		public object Get(string key)
		{
			return _cache.Get<object>(key);
		}

		public void Put(string key, object value, TimeSpan duration, IEnumerable<string> tags)
		{
			_cache.Set(key, value, duration == TimeSpan.MaxValue ? (TimeSpan?)null : duration);

			AddExpirationTags(key, tags);
		}

		public void AddExpirationTags(string cacheKey, IEnumerable<string> tags)
		{
			if (tags == null)
				return;

			foreach (var tag in tags)
			{
				var keysForTag = _cache.Get(CachePrefix + "tag:" + tag, () => 
				{
					return new HashSet<string>();
				});
				keysForTag.Add(cacheKey);
			}
		}

		public void InvalidateAll()
		{
			// remove tag lookup
			_cache.RemoveByPattern(CachePrefix + "tag:");
			
			// remove all cached items
			_cache.RemoveByPattern(CachePrefix);
		}

		public void InvalidateByTag(params string[] tags)
		{
			foreach (var tag in tags)
			{
				var tagKey = CachePrefix + "tag:" + tag;
				var keysForTag = _cache.Get<HashSet<string>>(tagKey);
				if (keysForTag != null)
				{
					// remove cached items
					keysForTag.Each(x => _cache.Remove(x));

					// remove tag lookup
					_cache.Remove(tagKey);
				}
			}
		}

		public void InvalidateSets(IEnumerable<string> entitySets)
		{
			if (entitySets == null)
				return;

			foreach (var set in entitySets)
			{
				InvalidateByTag("_set_" + set);
			}
		}

		#region Request Cache

		public object GetFromRequest(string key)
		{
			return _requestCache.Value.Get<object>(key);
		}

		public void PutForRequest(string key, object value, IEnumerable<string> tags)
		{
			_requestCache.Value.Set(key, value);

			AddExpirationTagsForRequest(key, tags);
		}

		public void AddExpirationTagsForRequest(string cacheKey, IEnumerable<string> tags)
		{
			if (tags == null)
				return;

			foreach (var tag in tags)
			{
				var keysForTag = _requestCache.Value.Get(CachePrefix + "tag:" + tag, () =>
				{
					return new HashSet<string>();
				});
				keysForTag.Add(cacheKey);
			}
		}

		public void InvalidateAllInRequest()
		{
			// remove tag lookup
			_requestCache.Value.RemoveByPattern(CachePrefix + "tag:");

			// remove all cached items
			_requestCache.Value.RemoveByPattern(CachePrefix);
		}

		public void InvalidateByTagInRequest(params string[] tags)
		{
			var requestCache = _requestCache.Value;

			foreach (var tag in tags)
			{
				var tagKey = CachePrefix + "tag:" + tag;
				var keysForTag = requestCache.Get<HashSet<string>>(tagKey);
				if (keysForTag != null)
				{
					// remove cached items
					keysForTag.Each(x => requestCache.Remove(x));

					// remove tag lookup
					requestCache.Remove(tagKey);
				}
			}
		}

		public void InvalidateSetsInRequest(IEnumerable<string> entitySets)
		{
			if (entitySets == null)
				return;

			foreach (var set in entitySets)
			{
				InvalidateByTagInRequest("_set_" + set);
			}
		}

		#endregion
	}
}
