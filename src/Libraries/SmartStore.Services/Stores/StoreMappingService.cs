using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Stores;

namespace SmartStore.Services.Stores
{
	/// <summary>
	/// Store mapping service
	/// </summary>
	public partial class StoreMappingService : IStoreMappingService
	{
		#region Constants

		private const string STOREMAPPING_BY_ENTITYID_NAME_KEY = "SmartStore.storemapping.entityid-name-{0}-{1}";
		private const string STOREMAPPING_PATTERN_KEY = "SmartStore.storemapping.";

		#endregion

		#region Fields

		private readonly IRepository<StoreMapping> _storeMappingRepository;
		private readonly IStoreContext _storeContext;
		private readonly IStoreService _storeService;
		private readonly ICacheManager _cacheManager;

		#endregion

		#region Ctor

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="cacheManager">Cache manager</param>
		/// <param name="storeContext">Store context</param>
		/// <param name="storeMappingRepository">Store mapping repository</param>
		public StoreMappingService(ICacheManager cacheManager,
			IStoreContext storeContext,
			IStoreService storeService,
			IRepository<StoreMapping> storeMappingRepository)
		{
			this._cacheManager = cacheManager;
			this._storeContext = storeContext;
			this._storeService = storeService;
			this._storeMappingRepository = storeMappingRepository;

			this.QuerySettings = DbQuerySettings.Default;
		}

		public DbQuerySettings QuerySettings { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Deletes a store mapping record
		/// </summary>
		/// <param name="storeMapping">Store mapping record</param>
		public virtual void DeleteStoreMapping(StoreMapping storeMapping)
		{
			if (storeMapping == null)
				throw new ArgumentNullException("storeMapping");

			_storeMappingRepository.Delete(storeMapping);

			//cache
			_cacheManager.RemoveByPattern(STOREMAPPING_PATTERN_KEY);
		}

		/// <summary>
		/// Gets a store mapping record
		/// </summary>
		/// <param name="storeMappingId">Store mapping record identifier</param>
		/// <returns>Store mapping record</returns>
		public virtual StoreMapping GetStoreMappingById(int storeMappingId)
		{
			if (storeMappingId == 0)
				return null;

			var storeMapping = _storeMappingRepository.GetById(storeMappingId);
			return storeMapping;
		}

		/// <summary>
		/// Gets store mapping records
		/// </summary>
		/// <typeparam name="T">Type</typeparam>
		/// <param name="entity">Entity</param>
		/// <returns>Store mapping records</returns>
		public virtual IList<StoreMapping> GetStoreMappings<T>(T entity) where T : BaseEntity, IStoreMappingSupported
		{
			if (entity == null)
				throw new ArgumentNullException("entity");

			int entityId = entity.Id;
			string entityName = typeof(T).Name;

			var query = from sm in _storeMappingRepository.Table
						where sm.EntityId == entityId &&
						sm.EntityName == entityName
						select sm;
			var storeMappings = query.ToList();
			return storeMappings;
		}

		/// <summary>
		/// Gets store mapping records
		/// </summary>
		/// <param name="entityName">Could be null</param>
		/// <param name="entityId">Could be 0</param>
		/// <returns>Store mapping record query</returns>
		public virtual IQueryable<StoreMapping> GetStoreMappingsFor(string entityName, int entityId)
		{
			var query = _storeMappingRepository.Table;

			if (entityName.HasValue())
				query = query.Where(x => x.EntityName == entityName);

			if (entityId != 0)
				query = query.Where(x => x.EntityId == entityId);

			return query;
		}

		/// <summary>
		/// Save the store napping for an entity
		/// </summary>
		/// <typeparam name="T">Entity type</typeparam>
		/// <param name="entity">The entity</param>
		/// <param name="selectedStoreIds">Array of selected store ids</param>
		public virtual void SaveStoreMappings<T>(T entity, int[] selectedStoreIds) where T : BaseEntity, IStoreMappingSupported
		{
			var existingStoreMappings = GetStoreMappings(entity);
			var allStores = _storeService.GetAllStores();

			foreach (var store in allStores)
			{
				if (selectedStoreIds != null && selectedStoreIds.Contains(store.Id))
				{
					if (existingStoreMappings.Where(sm => sm.StoreId == store.Id).Count() == 0)
						InsertStoreMapping(entity, store.Id);
				}
				else
				{
					var storeMappingToDelete = existingStoreMappings.Where(sm => sm.StoreId == store.Id).FirstOrDefault();
					if (storeMappingToDelete != null)
						DeleteStoreMapping(storeMappingToDelete);
				}
			}
		}

		/// <summary>
		/// Inserts a store mapping record
		/// </summary>
		/// <param name="storeMapping">Store mapping</param>
		public virtual void InsertStoreMapping(StoreMapping storeMapping)
		{
			if (storeMapping == null)
				throw new ArgumentNullException("storeMapping");

			_storeMappingRepository.Insert(storeMapping);

			//cache
			_cacheManager.RemoveByPattern(STOREMAPPING_PATTERN_KEY);
		}

		/// <summary>
		/// Inserts a store mapping record
		/// </summary>
		/// <typeparam name="T">Type</typeparam>
		/// <param name="storeId">Store id</param>
		/// <param name="entity">Entity</param>
		public virtual void InsertStoreMapping<T>(T entity, int storeId) where T : BaseEntity, IStoreMappingSupported
		{
			if (entity == null)
				throw new ArgumentNullException("entity");

			if (storeId == 0)
				throw new ArgumentOutOfRangeException("storeId");

			int entityId = entity.Id;
			string entityName = typeof(T).Name;

			var storeMapping = new StoreMapping()
			{
				EntityId = entityId,
				EntityName = entityName,
				StoreId = storeId
			};

			InsertStoreMapping(storeMapping);
		}

		/// <summary>
		/// Updates the store mapping record
		/// </summary>
		/// <param name="storeMapping">Store mapping</param>
		public virtual void UpdateStoreMapping(StoreMapping storeMapping)
		{
			if (storeMapping == null)
				throw new ArgumentNullException("storeMapping");

			_storeMappingRepository.Update(storeMapping);

			//cache
			_cacheManager.RemoveByPattern(STOREMAPPING_PATTERN_KEY);
		}

		/// <summary>
		/// Find store identifiers with granted access (mapped to the entity)
		/// </summary>
		/// <typeparam name="T">Type</typeparam>
		/// <param name="entity">Wntity</param>
		/// <returns>Store identifiers</returns>
		public virtual int[] GetStoresIdsWithAccess<T>(T entity) where T : BaseEntity, IStoreMappingSupported
		{
			if (entity == null)
				throw new ArgumentNullException("entity");

			int entityId = entity.Id;
			string entityName = typeof(T).Name;

			string key = string.Format(STOREMAPPING_BY_ENTITYID_NAME_KEY, entityId, entityName);
			return _cacheManager.Get(key, () =>
			{
				var query = from sm in _storeMappingRepository.Table
							where sm.EntityId == entityId &&
							sm.EntityName == entityName
							select sm.StoreId;
				var result = query.ToArray();
				//little hack here. nulls aren't cacheable so set it to ""
				if (result == null)
					result = new int[0];
				return result;
			});
		}

		/// <summary>
		/// Authorize whether entity could be accessed in the current store (mapped to this store)
		/// </summary>
		/// <typeparam name="T">Type</typeparam>
		/// <param name="entity">Wntity</param>
		/// <returns>true - authorized; otherwise, false</returns>
		public virtual bool Authorize<T>(T entity) where T : BaseEntity, IStoreMappingSupported
		{
			return Authorize(entity, _storeContext.CurrentStore.Id);
		}

		/// <summary>
		/// Authorize whether entity could be accessed in a store (mapped to this store)
		/// </summary>
		/// <typeparam name="T">Type</typeparam>
		/// <param name="entity">Entity</param>
		/// <param name="store">Store</param>
		/// <returns>true - authorized; otherwise, false</returns>
		public virtual bool Authorize<T>(T entity, int storeId) where T : BaseEntity, IStoreMappingSupported
		{
			if (entity == null)
				return false;

			if (storeId == 0)
				//return true if no store specified/found
				return true;

			if (QuerySettings.IgnoreMultiStore)
				return true;

			if (!entity.LimitedToStores)
				return true;

			foreach (var storeIdWithAccess in GetStoresIdsWithAccess(entity))
				if (storeId == storeIdWithAccess)
					//yes, we have such permission
					return true;

			//no permission found
			return false;
		}

		#endregion
	}
}