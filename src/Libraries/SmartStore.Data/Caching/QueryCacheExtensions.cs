using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Entity;
using SmartStore.Core;
using System.Diagnostics;
using SmartStore.Core.Infrastructure;

namespace SmartStore.Data.Caching
{
	public static partial class QueryCacheExtensions
	{
		internal static QueryCache GetQueryCacheInstance()
		{
			return EngineContext.Current.Resolve<QueryCache>();
		}
		
		/// <summary>
		/// Returns the result of the <paramref name="query" /> from the cache. If the query result is not cached
		/// yet, the query is materialized and cached before being returned.
		/// </summary>
		/// <param name="tags">The list of tags to use for cache expiration.</param>
		/// <returns>The result of the query.</returns>
		public static IList<T> FromCache<T>(this IQueryable<T> query, params string[] tags) where T : BaseEntity
		{
			return FromCache(query, TimeSpan.MaxValue, tags);
		}

		/// <summary>
		/// Returns the result of the <paramref name="query" /> from the cache. If the query result is not cached
		/// yet, the query is materialized and cached before being returned.
		/// </summary>
		/// <param name="duration">Absolute expiration time</param>
		/// <param name="tags">The list of tags to use for cache expiration.</param>
		/// <returns>The result of the query.</returns>
		public static IList<T> FromCache<T>(this IQueryable<T> query, TimeSpan duration, params string[] tags) where T : BaseEntity
		{
			var queryCache = GetQueryCacheInstance();

			var key = queryCache.GetCacheKey(query, tags);

			var result = queryCache.Get(key);

			if (result == null)
			{			
				result = query.AsNoTracking().ToList();

				var affectedEntitySets = query.GetObjectQuery().GetAffectedEntitySets();
				tags = tags.Concat(affectedEntitySets.Select(x => "_set_" + x)).ToArray();

				queryCache.Put(key, result, duration, tags);
			}

			return (IList<T>)result;
		}

		/// <summary>
		/// Returns the result of the <paramref name="query" /> from the cache. If the query result is not cached
		/// yet, the query is materialized and cached before being returned.
		/// </summary>
		/// <param name="entityId"></param>
		/// <param name="duration">Absolute expiration time</param>
		/// <param name="tags">The list of tags to use for cache expiration.</param>
		/// <returns>The result of the query.</returns>
		public static T FirstOrDefaultFromCache<T>(this IQueryable<T> query, int entityId, TimeSpan duration, params string[] tags) where T : BaseEntity
		{
			throw new NotImplementedException();
		}


		#region RequestCache

		public static IList<T> FromRequestCache<T>(this IQueryable<T> query, params string[] tags) where T : BaseEntity
		{
			throw new NotImplementedException();
		}

		public static T FirstOrDefaultRequestFromCache<T>(this IQueryable<T> query, int entityId, params string[] tags) where T : BaseEntity
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
