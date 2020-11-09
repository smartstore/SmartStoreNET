using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Infrastructure;
using SmartStore.Utilities.ObjectPools;

namespace SmartStore.Data.Caching
{
    public static class DbCacheExtensions
    {
        internal static IDbCache GetDbCacheInstance()
        {
            try
            {
                return EngineContext.Current.Resolve<IDbCache>();
            }
            catch
            {
                return new NullDbCache();
            }
        }

        #region Request Cache

        /// <summary>
        /// Returns the result of <paramref name="source"/> from the (HTTP)-REQUEST cache if available,
        /// otherwise the query is materialized and cached before being returned. Cache result invalidation
        /// is performed automatically by the underlying interceptor.
        /// </summary>
        /// <typeparam name="T">The type of the data in the data source.</typeparam>
        /// <param name="source">The query to be materialized.</param>
        /// <returns>The result of the query.</returns>
        public static List<T> ToListCached<T>(this IQueryable<T> source)
        {
            return ToListCached(source, null);
        }

        /// <summary>
        /// Returns the result of <paramref name="source"/> from the (HTTP)-REQUEST cache if available,
        /// otherwise the query is materialized and cached before being returned. Cache result invalidation
        /// is performed automatically by the underlying interceptor.
        /// </summary>
        /// <typeparam name="T">The type of the data in the data source.</typeparam>
        /// <param name="source">The query to be materialized.</param>
        /// <param name="customKey">A custom cache key to use instead of the computed one.</param>
        /// <returns>The result of the query.</returns>
        /// <remarks>
        /// This overload is slightly faster, because the key does not need to be generated from the passed <paramref name="source"/>
        /// </remarks>
        public static List<T> ToListCached<T>(this IQueryable<T> source, string customKey)
        {
            var entry = GetCacheEntry(source, customKey, () => source.ToList());
            if (entry == null)
            {
                return source.ToList();
            }

            return (List<T>)entry.Value;
        }

        /// <summary>
        /// Returns the first element of <paramref name="source"/> from the (HTTP)-REQUEST cache if available,
        /// otherwise the query is materialized and cached before being returned. Cache result invalidation
        /// is performed automatically by the underlying interceptor.
        /// </summary>
        /// <typeparam name="T">The type of the data in the data source.</typeparam>
        /// <param name="source">The query to be materialized.</param>
        /// <returns>The result of the query.</returns>
        public static T FirstOrDefaultCached<T>(this IQueryable<T> source)
        {
            return FirstOrDefaultCached(source, null);
        }

        /// <summary>
        /// Returns the first element of <paramref name="source"/> from the (HTTP)-REQUEST cache if available,
        /// otherwise the query is materialized and cached before being returned. Cache result invalidation
        /// is performed automatically by the underlying interceptor.
        /// </summary>
        /// <typeparam name="T">The type of the data in the data source.</typeparam>
        /// <param name="source">The query to be materialized.</param>
        /// <param name="customKey">A custom cache key to use instead of the computed one.</param>
        /// <returns>The result of the query.</returns>
        /// <remarks>
        /// This overload is slightly faster, because the key does not need to be generated from the passed <paramref name="source"/>
        /// </remarks>
        public static T FirstOrDefaultCached<T>(this IQueryable<T> source, string customKey)
        {
            var entry = GetCacheEntry(source, customKey, () => source.FirstOrDefault());
            if (entry == null)
            {
                return source.FirstOrDefault();
            }

            return (T)entry.Value;
        }

        internal static DbCacheEntry GetCacheEntry<T>(this IQueryable<T> source, string customKey, Func<object> valueFactory)
        {
            // (Perf) Checking here means higher perf, as keys and entitysets does not need to be evaluated
            var cache = GetDbCacheInstance();
            if (!cache.Enabled)
            {
                return null;
            }

            Guard.NotNull(source, nameof(source));
            Guard.NotNull(valueFactory, nameof(valueFactory));

            var cacheKey = new CacheKey<T>(source, customKey);
            if (cacheKey.Key.IsEmpty())
            {
                return null;
            }

            if (!cache.RequestTryGet(cacheKey.Key, out var entry))
            {
                entry = cache.RequestPut(cacheKey.Key, valueFactory(), cacheKey.AffectedEntitySets);
            }
            //else
            //{
            //	Debug.WriteLine("FromRequestCache: " + cacheKey.Key);
            //}

            return entry;
        }

        #endregion

        #region Repository extensions

        /// <summary>
        /// Gets an entity by id from the database, the local change tracker, or the (HTTP)-REQUEST cache.
        /// Cache result invalidation is performed automatically by the underlying interceptor.
        /// </summary>
        /// <param name="id">The id of the entity. This can also be a composite key.</param>
        /// <returns>The resolved entity</returns>
        public static T GetByIdCached<T>(this IRepository<T> rs, object id)
            where T : BaseEntity
        {
            return GetByIdCached(rs, id, null);
        }

        /// <summary>
        /// Gets an entity by id from the database, the local change tracker, or the (HTTP)-REQUEST cache.
        /// Cache result invalidation is performed automatically by the underlying interceptor.
        /// </summary>
        /// <param name="id">The id of the entity. This can also be a composite key.</param>
        /// <param name="customKey">A custom cache key to use instead of the computed one.</param>
        /// <returns>The resolved entity</returns>
        /// <remarks>
        /// This overload is slightly faster, because the key does not need to be generated.
        /// </remarks>
        public static T GetByIdCached<T>(this IRepository<T> rs, object id, string customKey)
            where T : BaseEntity
        {
            var entry = rs.Table.GetCacheEntry(customKey, () => rs.GetById(id));
            if (entry == null)
            {
                return rs.GetById(id);
            }

            return (T)entry.Value;
        }

        #endregion

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

        private class CacheKey<T>
        {
            public CacheKey(IQueryable<T> source, string customKey = null)
            {
                Guard.NotNull(source, nameof(source));

                GenerateKeyAndAffectedEntitySets(source, customKey);
            }

            private void GenerateKeyAndAffectedEntitySets(IQueryable<T> source, string customKey = null)
            {
                var objectQuery = DbCacheUtil.GetObjectQuery(source) ?? source as ObjectQuery;

                var key = customKey.NullEmpty();
                if (objectQuery != null && string.IsNullOrEmpty(key))
                {
                    var commandInfo = objectQuery.GetCommandInfo();

                    var psb = PooledStringBuilder.Rent();
                    var sb = (StringBuilder)psb;
                    sb.AppendLine(commandInfo.Sql);

                    foreach (DbParameter parameter in commandInfo.Parameters)
                    {
                        sb.Append(parameter.ParameterName);
                        sb.Append(";");
                        sb.Append(parameter.Value);
                        sb.AppendLine(";");
                    }

                    key = psb.ToStringAndReturn();
                }

                Key = key;
                AffectedEntitySets = objectQuery != null
                    ? objectQuery.GetAffectedEntitySets()
                    : new string[0];
            }

            public string Key
            {
                get;
                private set;
            }

            public string[] AffectedEntitySets
            {
                get;
                private set;
            }
        }
    }
}
