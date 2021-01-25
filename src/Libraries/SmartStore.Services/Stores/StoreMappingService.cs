using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Stores;

namespace SmartStore.Services.Stores
{
    public partial class StoreMappingService : IStoreMappingService
    {
        /// <summary>
        /// 0 = segment (EntityName.IdRange)
        /// </summary>
        const string STOREMAPPING_SEGMENT_KEY = "storemapping:range-{0}";
        internal const string STOREMAPPING_SEGMENT_PATTERN = "storemapping:range-*";

        private readonly IRepository<StoreMapping> _storeMappingRepository;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly ICacheManager _cacheManager;

        public StoreMappingService(
            ICacheManager cacheManager,
            IStoreContext storeContext,
            IStoreService storeService,
            IRepository<StoreMapping> storeMappingRepository)
        {
            _cacheManager = cacheManager;
            _storeContext = storeContext;
            _storeService = storeService;
            _storeMappingRepository = storeMappingRepository;

            QuerySettings = DbQuerySettings.Default;
        }

        public DbQuerySettings QuerySettings { get; set; }

        public virtual void DeleteStoreMapping(StoreMapping storeMapping)
        {
            Guard.NotNull(storeMapping, nameof(storeMapping));

            _storeMappingRepository.Delete(storeMapping);

            ClearCacheSegment(storeMapping.EntityName, storeMapping.EntityId);
        }

        public virtual StoreMapping GetStoreMappingById(int storeMappingId)
        {
            if (storeMappingId == 0)
                return null;

            var storeMapping = _storeMappingRepository.GetById(storeMappingId);
            return storeMapping;
        }

        public virtual IList<StoreMapping> GetStoreMappings<T>(T entity) where T : BaseEntity, IStoreMappingSupported
        {
            Guard.NotNull(entity, nameof(entity));

            int entityId = entity.Id;
            string entityName = entity.GetEntityName();

            var query = from sm in _storeMappingRepository.Table
                        where sm.EntityId == entityId &&
                        sm.EntityName == entityName
                        select sm;
            var storeMappings = query.ToList();
            return storeMappings;
        }

        public virtual IQueryable<StoreMapping> GetStoreMappingsFor(string entityName, int entityId)
        {
            var query = _storeMappingRepository.Table;

            if (entityName.HasValue())
                query = query.Where(x => x.EntityName == entityName);

            if (entityId != 0)
                query = query.Where(x => x.EntityId == entityId);

            return query;
        }

        public virtual void SaveStoreMappings<T>(T entity, int[] selectedStoreIds) where T : BaseEntity, IStoreMappingSupported
        {
            var existingStoreMappings = GetStoreMappings(entity);
            var allStores = _storeService.GetAllStores();
            var selectedIds = selectedStoreIds ?? Array.Empty<int>();

            entity.LimitedToStores = selectedIds.Length == 1 && selectedIds[0] == 0
                ? false
                : selectedIds.Any();

            foreach (var store in allStores)
            {
                if (selectedIds.Contains(store.Id))
                {
                    if (!existingStoreMappings.Any(x => x.StoreId == store.Id))
                        InsertStoreMapping(entity, store.Id);
                }
                else
                {
                    var storeMappingToDelete = existingStoreMappings.FirstOrDefault(x => x.StoreId == store.Id);
                    if (storeMappingToDelete != null)
                        DeleteStoreMapping(storeMappingToDelete);
                }
            }

            _storeMappingRepository.Context.SaveChanges();
        }

        public virtual void InsertStoreMapping(StoreMapping storeMapping)
        {
            Guard.NotNull(storeMapping, nameof(storeMapping));

            _storeMappingRepository.Insert(storeMapping);

            ClearCacheSegment(storeMapping.EntityName, storeMapping.EntityId);
        }

        public virtual void InsertStoreMapping<T>(T entity, int storeId) where T : BaseEntity, IStoreMappingSupported
        {
            Guard.NotNull(entity, nameof(entity));

            if (storeId == 0)
                throw new ArgumentOutOfRangeException(nameof(storeId));

            int entityId = entity.Id;
            string entityName = entity.GetEntityName();

            var storeMapping = new StoreMapping
            {
                EntityId = entityId,
                EntityName = entityName,
                StoreId = storeId
            };

            InsertStoreMapping(storeMapping);
        }

        public virtual void UpdateStoreMapping(StoreMapping storeMapping)
        {
            Guard.NotNull(storeMapping, nameof(storeMapping));

            _storeMappingRepository.Update(storeMapping);

            ClearCacheSegment(storeMapping.EntityName, storeMapping.EntityId);
        }

        public virtual int[] GetStoresIdsWithAccess(string entityName, int entityId)
        {
            Guard.NotEmpty(entityName, nameof(entityName));

            if (entityId <= 0)
                return new int[0];

            var cacheSegment = GetCacheSegment(entityName, entityId);

            if (!cacheSegment.TryGetValue(entityId, out var storeIds))
            {
                return Array.Empty<int>();
            }

            return storeIds;
        }

        public bool Authorize(string entityName, int entityId)
        {
            return Authorize(entityName, entityId, _storeContext.CurrentStore.Id);
        }

        public virtual bool Authorize(string entityName, int entityId, int storeId)
        {
            Guard.NotEmpty(entityName, nameof(entityName));

            if (entityId <= 0)
                return false;

            if (storeId <= 0)
                // return true if no store specified/found
                return true;

            if (QuerySettings.IgnoreMultiStore)
                return true;

            // Permission granted only when the id list contains the passed storeId
            return GetStoresIdsWithAccess(entityName, entityId).Any(x => x == storeId);
        }

        #region Cache segmenting

        protected virtual IDictionary<int, int[]> GetCacheSegment(string entityName, int entityId)
        {
            Guard.NotEmpty(entityName, nameof(entityName));

            var segmentKey = GetSegmentKeyPart(entityName, entityId, out var minEntityId, out var maxEntityId);
            var cacheKey = BuildCacheSegmentKey(segmentKey);

            return _cacheManager.Get(cacheKey, () =>
            {
                var query = from sm in _storeMappingRepository.TableUntracked
                            where
                                sm.EntityId >= minEntityId &&
                                sm.EntityId <= maxEntityId &&
                                sm.EntityName == entityName
                            select sm;

                var mappings = query.ToLookup(x => x.EntityId, x => x.StoreId);

                var dict = new Dictionary<int, int[]>(mappings.Count);

                foreach (var sm in mappings)
                {
                    dict[sm.Key] = sm.ToArray();
                }

                return dict;
            });
        }

        /// <summary>
        /// Clears the cached segment from the cache
        /// </summary>
        protected virtual void ClearCacheSegment(string entityName, int entityId)
        {
            try
            {
                var segmentKey = GetSegmentKeyPart(entityName, entityId);
                _cacheManager.Remove(BuildCacheSegmentKey(segmentKey));
            }
            catch { }
        }

        private string BuildCacheSegmentKey(string segment)
        {
            return String.Format(STOREMAPPING_SEGMENT_KEY, segment);
        }

        private string GetSegmentKeyPart(string entityName, int entityId)
        {
            return GetSegmentKeyPart(entityName, entityId, out _, out _);
        }

        private string GetSegmentKeyPart(string entityName, int entityId, out int minId, out int maxId)
        {
            (minId, maxId) = entityId.GetRange(1000);
            return (entityName + "." + minId.ToString()).ToLowerInvariant();
        }

        #endregion
    }
}