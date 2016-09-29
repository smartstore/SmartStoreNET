using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;
using SmartStore.ComponentModel;
using SmartStore.Core;
using SmartStore.Core.Infrastructure;

namespace SmartStore.Data.Caching
{
	public static class DbCacheExtensions
	{
		internal static IDbCache GetDbCacheInstance()
		{
			return EngineContext.Current.Resolve<IDbCache>();
		}

		/// <summary>
		/// Forces query results to be cached.
		/// Allows caching results for queries using non-deterministic functions. 
		/// </summary>
		/// <typeparam name="T">Query element type.</typeparam>
		/// <param name="source">Query whose results will be cached. Must not be null.</param>
		public static IQueryable<T> Cached<T>(this IQueryable<T> source)
			where T : class
		{
			Guard.NotNull(source, nameof(source));

			// TODO: implement Uncached()

			var objectQuery = DbCacheUtil.GetObjectQuery(source) ?? source as ObjectQuery;

			if (objectQuery != null)
			{
				SingletonQueries.Current.AddCachedQuery(
					objectQuery.Context.MetadataWorkspace, 
					objectQuery.ToTraceString());
			}

			return source;
		}

		public static List<T> FromRequestCache<T>(this IQueryable<T> source)
			where T : BaseEntity
		{
			Guard.NotNull(source, nameof(source));

			var cacheKey = GetCacheKeyFromQuery(source);

			if (cacheKey.IsEmpty())
			{
				return source.ToList();
			}

			var cache = GetDbCacheInstance();
			DbCacheEntry entry;
			if (!cache.RequestTryGet(cacheKey, out entry))
			{
				// TODO: resolve and pass EntitySets
				entry = new DbCacheEntry
				{
					Key = cacheKey,
					Value = source.ToList(),
					EntitySets = Enumerable.Empty<string>().ToArray() // TODO
				};

				cache.RequestPut(cacheKey, entry, entry.EntitySets);
			}

			return (List<T>)entry.Value;
		}

		public static T FromRequestCacheFirstOrDefault<T>(this IQueryable<T> source)
			where T : BaseEntity
		{
			Guard.NotNull(source, nameof(source));

			var cacheKey = GetCacheKeyFromQuery(source);

			if (cacheKey.IsEmpty())
			{
				return source.FirstOrDefault();
			}

			var cache = GetDbCacheInstance();
			DbCacheEntry entry;
			if (!cache.RequestTryGet(cacheKey, out entry))
			{
				// TODO: resolve and pass EntitySets
				entry = new DbCacheEntry
				{
					Key = cacheKey,
					Value = source.FirstOrDefault(),
					EntitySets = Enumerable.Empty<string>().ToArray() // TODO
				};

				cache.RequestPut(cacheKey, entry, entry.EntitySets);
			}

			return (T)entry.Value;
		}

		private static string GetCacheKeyFromQuery<T>(IQueryable<T> source)
		{
			var objectQuery = DbCacheUtil.GetObjectQuery(source) ?? source as ObjectQuery;

			if (objectQuery != null)
			{
				return objectQuery.ToTraceString();
			}

			return null;
		}
	}
}
