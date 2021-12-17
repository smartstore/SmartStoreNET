using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Seo;

namespace SmartStore.Services.Seo
{
    public partial class UrlRecordService : ScopedServiceBase, IUrlRecordService
    {
        /// <summary>
        /// 0 = segment (EntityName.IdRange), 1 = language id
        /// </summary>
        const string URLRECORD_SEGMENT_KEY = "urlrecord:segment:{0}-lang-{1}";
        const string URLRECORD_SEGMENT_PATTERN = "urlrecord:segment:{0}*";
        const string URLRECORD_ALL_PATTERN = "urlrecord:*";
        const string URLRECORD_ALL_ACTIVESLUGS_KEY = "urlrecord:all-active-slugs";

        private readonly IRepository<UrlRecord> _urlRecordRepository;
        private readonly ICacheManager _cacheManager;
        private readonly SeoSettings _seoSettings;
        private readonly PerformanceSettings _performanceSettings;

        private readonly IDictionary<string, UrlRecordCollection> _prefetchedCollections;
        private static int _lastCacheSegmentSize = -1;

        public UrlRecordService(
            ICacheManager cacheManager,
            IRepository<UrlRecord> urlRecordRepository,
            SeoSettings seoSettings,
            PerformanceSettings performanceSettings)
        {
            _cacheManager = cacheManager;
            _urlRecordRepository = urlRecordRepository;
            _seoSettings = seoSettings;
            _performanceSettings = performanceSettings;

            _prefetchedCollections = new Dictionary<string, UrlRecordCollection>(StringComparer.OrdinalIgnoreCase);

            ValidateCacheState();
        }

        private void ValidateCacheState()
        {
            // Ensure that after a segment size change the cache segments are invalidated.
            var size = _performanceSettings.CacheSegmentSize;
            var changed = _lastCacheSegmentSize == -1;

            if (size <= 0)
            {
                _performanceSettings.CacheSegmentSize = size = 1;
            }

            if (_lastCacheSegmentSize > 0 && _lastCacheSegmentSize != size)
            {
                _cacheManager.RemoveByPattern(URLRECORD_SEGMENT_PATTERN);
                changed = true;
            }

            if (changed)
            {
                Interlocked.Exchange(ref _lastCacheSegmentSize, size);
            }
        }

        protected override void OnClearCache()
        {
            _cacheManager.RemoveByPattern(URLRECORD_ALL_PATTERN);
        }

        public virtual void DeleteUrlRecord(UrlRecord urlRecord)
        {
            Guard.NotNull(urlRecord, nameof(urlRecord));

            try
            {
                // cache
                ClearCacheSegment(urlRecord.EntityName, urlRecord.EntityId, urlRecord.LanguageId);
                HasChanges = true;

                // db
                _urlRecordRepository.Delete(urlRecord);
            }
            catch { }
        }

        public virtual UrlRecord GetUrlRecordById(int urlRecordId)
        {
            if (urlRecordId == 0)
                return null;

            var urlRecord = _urlRecordRepository.GetById(urlRecordId);
            return urlRecord;
        }

        public virtual IList<UrlRecord> GetUrlRecordsByIds(int[] urlRecordIds)
        {
            if (urlRecordIds == null || urlRecordIds.Length == 0)
                return new List<UrlRecord>();

            var urlRecords = _urlRecordRepository.Table
                .Where(x => urlRecordIds.Contains(x.Id))
                .ToList();

            return urlRecords;
        }

        public virtual void InsertUrlRecord(UrlRecord urlRecord)
        {
            Guard.NotNull(urlRecord, nameof(urlRecord));

            try
            {
                // db
                _urlRecordRepository.Insert(urlRecord);
                HasChanges = true;

                // cache
                ClearCacheSegment(urlRecord.EntityName, urlRecord.EntityId, urlRecord.LanguageId);
            }
            catch { }
        }

        public virtual void UpdateUrlRecord(UrlRecord urlRecord)
        {
            Guard.NotNull(urlRecord, nameof(urlRecord));

            try
            {
                // db
                _urlRecordRepository.Update(urlRecord);
                HasChanges = true;

                // cache
                ClearCacheSegment(urlRecord.EntityName, urlRecord.EntityId, urlRecord.LanguageId);
            }
            catch { }
        }

        public virtual IPagedList<UrlRecord> GetAllUrlRecords(int pageIndex, int pageSize, string slug, string entityName, int? entityId, int? languageId, bool? isActive)
        {
            var query = _urlRecordRepository.Table;

            if (slug.HasValue())
                query = query.Where(x => x.Slug.Contains(slug));

            if (entityName.HasValue())
                query = query.Where(x => x.EntityName == entityName);

            if (entityId.HasValue)
                query = query.Where(x => x.EntityId == entityId.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (languageId.HasValue)
                query = query.Where(x => x.LanguageId == languageId);

            query = query.OrderBy(x => x.Slug);

            var urlRecords = new PagedList<UrlRecord>(query, pageIndex, pageSize);
            return urlRecords;
        }

        public virtual IList<UrlRecord> GetUrlRecordsFor(string entityName, int entityId, bool activeOnly = false)
        {
            Guard.NotEmpty(entityName, nameof(entityName));

            var query = from ur in _urlRecordRepository.Table
                        where ur.EntityId == entityId &&
                        ur.EntityName == entityName
                        select ur;

            if (activeOnly)
            {
                query = query.Where(ur => ur.IsActive);
            }

            return query.ToList();
        }

        public virtual void PrefetchUrlRecords(string entityName, int[] languageIds, int[] entityIds, bool isRange = false, bool isSorted = false)
        {
            var collection = GetUrlRecordCollectionInternal(entityName, languageIds, entityIds, isRange, isSorted);

            if (_prefetchedCollections.TryGetValue(entityName, out var existing))
            {
                collection.MergeWith(existing);
            }
            else
            {
                _prefetchedCollections[entityName] = collection;
            }
        }

        public virtual UrlRecordCollection GetUrlRecordCollection(string entityName, int[] languageIds, int[] entityIds, bool isRange = false, bool isSorted = false)
        {
            return GetUrlRecordCollectionInternal(entityName, languageIds, entityIds, isRange, isSorted);
        }

        public virtual UrlRecordCollection GetUrlRecordCollectionInternal(string entityName, int[] languageIds, int[] entityIds, bool isRange = false, bool isSorted = false)
        {
            Guard.NotEmpty(entityName, nameof(entityName));

            using (new DbContextScope(proxyCreation: false, lazyLoading: false))
            {
                var query = from x in _urlRecordRepository.TableUntracked
                            where x.EntityName == entityName && x.IsActive
                            select x;

                var requestedSet = entityIds;

                if (entityIds != null && entityIds.Length > 0)
                {
                    if (isRange)
                    {
                        if (!isSorted)
                        {
                            Array.Sort(entityIds);
                        }

                        var min = entityIds[0];
                        var max = entityIds[entityIds.Length - 1];

                        if (entityIds.Length == 2 && max > min + 1)
                        {
                            // Only min & max were passed, create the range sequence.
                            requestedSet = Enumerable.Range(min, max - min + 1).ToArray();
                        }

                        query = query.Where(x => x.EntityId >= min && x.EntityId <= max);
                    }
                    else
                    {
                        requestedSet = entityIds;
                        query = query.Where(x => entityIds.Contains(x.EntityId));
                    }
                }

                if (languageIds != null && languageIds.Length > 0)
                {
                    if (languageIds.Length == 1)
                    {
                        // Avoid "The LINQ expression node type 'ArrayIndex' is not supported in LINQ to Entities".
                        var languageId = languageIds[0];
                        query = query.Where(x => x.LanguageId == languageId);
                    }
                    else
                    {
                        query = query.Where(x => languageIds.Contains(x.LanguageId));
                    }
                }

                // Don't sort DESC, because latter items overwrite exisiting ones (it's the same as sorting DESC and taking the first)
                return new UrlRecordCollection(entityName, requestedSet, query.OrderBy(x => x.Id).ToList());
            }
        }

        public virtual UrlRecord GetBySlug(string slug)
        {
            // INFO: (mc) Caching unnecessary here. This is not a 'bottleneck' function.
            if (String.IsNullOrEmpty(slug))
                return null;

            var query = from ur in _urlRecordRepository.Table
                        where ur.Slug == slug
                        select ur;

            var urlRecord = query.FirstOrDefault();
            return urlRecord;
        }

        public virtual string GetActiveSlug(int entityId, string entityName, int languageId)
        {
            if (_prefetchedCollections.TryGetValue(entityName, out var collection))
            {
                var cachedItem = collection.Find(languageId, entityId);
                if (cachedItem != null)
                {
                    return cachedItem.Slug.EmptyNull();
                }
            }

            if (IsInScope)
            {
                return GetActiveSlugUncached(entityId, entityName, languageId);
            }

            string slug = null;

            if (_seoSettings.LoadAllUrlAliasesOnStartup)
            {
                var allActiveSlugs = _cacheManager.Get(URLRECORD_ALL_ACTIVESLUGS_KEY, () =>
                {
                    using (new DbContextScope(proxyCreation: false, lazyLoading: false))
                    {
                        var query = from x in _urlRecordRepository.TableUntracked
                                    where x.IsActive
                                    orderby x.Id descending
                                    select x;

                        var result = query.ToDictionarySafe(
                            x => GenerateKey(x.EntityId, x.EntityName, x.LanguageId),
                            x => x.Slug,
                            StringComparer.OrdinalIgnoreCase);

                        return result;
                    }
                }, independent: true);

                var key = GenerateKey(entityId, entityName, languageId);
                if (!allActiveSlugs.TryGetValue(key, out slug))
                {
                    return string.Empty;
                }
            }
            else
            {
                var slugs = GetCacheSegment(entityName, entityId, languageId);

                if (!slugs.TryGetValue(entityId, out slug))
                {
                    return string.Empty;
                }
            }

            return slug;
        }

        protected string GetActiveSlugUncached(int entityId, string entityName, int languageId)
        {
            var query = from x in _urlRecordRepository.TableUntracked
                        where
                            x.EntityId == entityId &&
                            x.EntityName == entityName &&
                            x.LanguageId == languageId &&
                            x.IsActive
                        orderby x.Id descending
                        select x.Slug;

            return query.FirstOrDefault().EmptyNull();
        }

        public virtual UrlRecord SaveSlug<T>(T entity, string slug, int languageId) where T : BaseEntity, ISlugSupported
        {
            Guard.NotNull(entity, nameof(entity));

            int entityId = entity.Id;
            string entityName = entity.GetEntityName();
            UrlRecord result = null;

            var query = from ur in _urlRecordRepository.Table
                        where ur.EntityId == entityId &&
                        ur.EntityName == entityName &&
                        ur.LanguageId == languageId
                        orderby ur.Id descending
                        select ur;
            var allUrlRecords = query.ToList();

            var activeUrlRecord = allUrlRecords.FirstOrDefault(x => x.IsActive);
            if (activeUrlRecord == null && !string.IsNullOrWhiteSpace(slug))
            {
                // find in non-active records with the specified slug
                var nonActiveRecordWithSpecifiedSlug = allUrlRecords.FirstOrDefault(x => x.Slug.Equals(slug, StringComparison.InvariantCultureIgnoreCase) && !x.IsActive);
                if (nonActiveRecordWithSpecifiedSlug != null)
                {
                    // mark non-active record as active
                    nonActiveRecordWithSpecifiedSlug.IsActive = true;
                    UpdateUrlRecord(nonActiveRecordWithSpecifiedSlug);
                }
                else
                {
                    // new record
                    var urlRecord = new UrlRecord
                    {
                        EntityId = entity.Id,
                        EntityName = entityName,
                        Slug = slug,
                        LanguageId = languageId,
                        IsActive = true,
                    };
                    InsertUrlRecord(urlRecord);
                    result = urlRecord;
                }
            }

            if (activeUrlRecord != null && string.IsNullOrWhiteSpace(slug))
            {
                // disable the previous active URL record
                activeUrlRecord.IsActive = false;
                UpdateUrlRecord(activeUrlRecord);
            }

            if (activeUrlRecord != null && !string.IsNullOrWhiteSpace(slug))
            {
                // is it the same slug as in active URL record?
                if (activeUrlRecord.Slug.Equals(slug, StringComparison.InvariantCultureIgnoreCase))
                {
                    // yes. do nothing
                    // P.S. wrote this way for more source code readability
                }
                else
                {
                    // find in non-active records with the specified slug
                    var nonActiveRecordWithSpecifiedSlug = allUrlRecords
                        .FirstOrDefault(x => x.Slug.Equals(slug, StringComparison.InvariantCultureIgnoreCase) && !x.IsActive);
                    if (nonActiveRecordWithSpecifiedSlug != null)
                    {
                        // mark non-active record as active
                        nonActiveRecordWithSpecifiedSlug.IsActive = true;
                        UpdateUrlRecord(nonActiveRecordWithSpecifiedSlug);

                        //disable the previous active URL record
                        activeUrlRecord.IsActive = false;
                        UpdateUrlRecord(activeUrlRecord);
                    }
                    else
                    {
                        // MC: Absolutely ensure, that we have no duplicate active record for this entity.
                        // In such case a record other than "activeUrlRecord" could have same seName
                        // and the DB would report an Index error.
                        var alreadyActiveDuplicate = allUrlRecords.FirstOrDefault(x => x.Slug.IsCaseInsensitiveEqual(slug) && x.IsActive);
                        if (alreadyActiveDuplicate != null)
                        {
                            // deactivate all
                            allUrlRecords.Each(x => x.IsActive = false);
                            // set the existing one to active again
                            alreadyActiveDuplicate.IsActive = true;
                            // update all records
                            allUrlRecords.Each(x => UpdateUrlRecord(x));
                        }
                        else
                        {
                            // insert new record
                            // we do not update the existing record because we should track all previously entered slugs
                            // to ensure that URLs will work fine
                            var urlRecord = new UrlRecord
                            {
                                EntityId = entity.Id,
                                EntityName = entityName,
                                Slug = slug,
                                LanguageId = languageId,
                                IsActive = true,
                            };
                            InsertUrlRecord(urlRecord);
                            result = urlRecord;

                            // disable the previous active URL record
                            activeUrlRecord.IsActive = false;
                            UpdateUrlRecord(activeUrlRecord);
                        }
                    }

                }
            }

            return result;
        }

        public virtual UrlRecord SaveSlug<T>(T entity, Func<T, string> nameProperty) where T : BaseEntity, ISlugSupported
        {
            string name = nameProperty.Invoke(entity);

            string existingSeName = entity.GetSeName<T>(0, true, false);
            existingSeName = entity.ValidateSeName(existingSeName, name, true);

            return SaveSlug(entity, existingSeName, 0);
        }

        private string GenerateKey(int entityId, string entityName, int languageId)
        {
            return "{0}.{1}.{2}".FormatInvariant(entityId, entityName, languageId);
        }

        public virtual Dictionary<int, int> CountSlugsPerEntity(int[] urlRecordIds)
        {
            if (urlRecordIds == null || urlRecordIds.Length == 0)
                return new Dictionary<int, int>();

            var query =
                from x in _urlRecordRepository.TableUntracked
                where urlRecordIds.Contains(x.Id)
                select new
                {
                    Id = x.Id,
                    Count = _urlRecordRepository.TableUntracked.Where(y => y.EntityName == x.EntityName && y.EntityId == x.EntityId).Count()
                };

            var result = query
                .ToList()
                .ToDictionary(x => x.Id, x => x.Count);

            return result;
        }

        public virtual int CountSlugsPerEntity(string entityName, int entityId)
        {
            var count = _urlRecordRepository.Table
                .Where(x => x.EntityName == entityName && x.EntityId == entityId)
                .Count();

            return count;
        }

        protected virtual IDictionary<int, string> GetCacheSegment(string entityName, int entityId, int languageId)
        {
            Guard.NotEmpty(entityName, nameof(entityName));

            var segmentKey = GetSegmentKeyPart(entityName, entityId, out var minEntityId, out var maxEntityId);
            var cacheKey = BuildCacheSegmentKey(segmentKey, languageId);

            return _cacheManager.Get(cacheKey, () =>
            {
                using (new DbContextScope(proxyCreation: false, lazyLoading: false))
                {
                    var query = from ur in _urlRecordRepository.TableUntracked
                                where
                                    ur.EntityId >= minEntityId &&
                                    ur.EntityId <= maxEntityId &&
                                    ur.EntityName == entityName &&
                                    ur.LanguageId == languageId &&
                                    ur.IsActive
                                orderby ur.Id descending
                                select ur;

                    var urlRecords = query.ToList();

                    var dict = new Dictionary<int, string>(urlRecords.Count);

                    foreach (var ur in urlRecords)
                    {
                        dict[ur.EntityId] = ur.Slug.EmptyNull();
                    }

                    return dict;
                }
            }, independent: true);
        }

        /// <summary>
        /// Clears the cached segment from the cache
        /// </summary>
        protected virtual void ClearCacheSegment(string entityName, int entityId, int? languageId = null)
        {
            if (IsInScope)
                return;

            var segmentKey = GetSegmentKeyPart(entityName, entityId);

            if (languageId.HasValue && languageId.Value > 0)
            {
                _cacheManager.Remove(BuildCacheSegmentKey(segmentKey, languageId.Value));
            }
            else
            {
                _cacheManager.RemoveByPattern(URLRECORD_SEGMENT_PATTERN.FormatInvariant(segmentKey));
            }

            // always delete this (in case when LoadAllOnStartup is true)
            _cacheManager.Remove(URLRECORD_ALL_ACTIVESLUGS_KEY);
        }

        private string BuildCacheSegmentKey(string segment, int languageId)
        {
            return string.Format(URLRECORD_SEGMENT_KEY, segment, languageId);
        }

        private string GetSegmentKeyPart(string entityName, int entityId)
        {
            return GetSegmentKeyPart(entityName, entityId, out _, out _);
        }

        private string GetSegmentKeyPart(string entityName, int entityId, out int minId, out int maxId)
        {
            (minId, maxId) = entityId.GetRange(_performanceSettings.CacheSegmentSize);
            return (entityName + "." + minId.ToString()).ToLowerInvariant();
        }
    }
}