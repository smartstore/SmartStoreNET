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
		private const string STOREMAPPING_BY_ENTITYID_NAME_KEY = "storemapping:entityid-name-{0}-{1}";
		private const string STOREMAPPING_PATTERN_KEY = "storemapping:*";

		private readonly IRepository<StoreMapping> _storeMappingRepository;
		private readonly IStoreContext _storeContext;
		private readonly IStoreService _storeService;
		private readonly ICacheManager _cacheManager;

		public StoreMappingService(ICacheManager cacheManager,
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

			_cacheManager.RemoveByPattern(STOREMAPPING_PATTERN_KEY);
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
			string entityName = typeof(T).Name;

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

			foreach (var store in allStores)
			{
				if (selectedStoreIds != null && selectedStoreIds.Contains(store.Id))
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
		}

		public virtual void InsertStoreMapping(StoreMapping storeMapping)
		{
			Guard.NotNull(storeMapping, nameof(storeMapping));

			_storeMappingRepository.Insert(storeMapping);

			_cacheManager.RemoveByPattern(STOREMAPPING_PATTERN_KEY);
		}

		public virtual void InsertStoreMapping<T>(T entity, int storeId) where T : BaseEntity, IStoreMappingSupported
		{
			Guard.NotNull(entity, nameof(entity));

			if (storeId == 0)
				throw new ArgumentOutOfRangeException(nameof(storeId));

			int entityId = entity.Id;
			string entityName = typeof(T).Name;

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

			_cacheManager.RemoveByPattern(STOREMAPPING_PATTERN_KEY);
		}

		public virtual int[] GetStoresIdsWithAccess(string entityName, int entityId)
		{
			Guard.NotEmpty(entityName, nameof(entityName));

			if (entityId <= 0)
				return new int[0];

			string key = string.Format(STOREMAPPING_BY_ENTITYID_NAME_KEY, entityId, entityName);
			return _cacheManager.Get(key, () =>
			{
				var query = from sm in _storeMappingRepository.Table
							where sm.EntityId == entityId &&
							sm.EntityName == entityName
							select sm.StoreId;

				var result = query.ToArray();
				return result;
			});
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

			foreach (var storeIdWithAccess in GetStoresIdsWithAccess(entityName, entityId))
			{
				if (storeId == storeIdWithAccess)
				{
					// yes, we have such permission
					return true;
				}
			}

			// no permission granted
			return false;
		}
	}
}