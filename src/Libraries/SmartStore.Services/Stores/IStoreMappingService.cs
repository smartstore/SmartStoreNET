using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Domain.Stores;

namespace SmartStore.Services.Stores
{
    /// <summary>
    /// Store mapping service interface
    /// </summary>
    public partial interface IStoreMappingService
    {
        /// <summary>
        /// Deletes a store mapping record
        /// </summary>
        /// <param name="storeMapping">Store mapping record</param>
        void DeleteStoreMapping(StoreMapping storeMapping);

        /// <summary>
        /// Gets a store mapping record
        /// </summary>
        /// <param name="storeMappingId">Store mapping record identifier</param>
        /// <returns>Store mapping record</returns>
        StoreMapping GetStoreMappingById(int storeMappingId);

        /// <summary>
        /// Gets store mapping records
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Entity</param>
        /// <returns>Store mapping records</returns>
        IList<StoreMapping> GetStoreMappings<T>(T entity) where T : BaseEntity, IStoreMappingSupported;

        /// <summary>
        /// Gets store mapping records
        /// </summary>
        /// <param name="entityName">Could be null</param>
        /// <param name="entityId">Could be 0</param>
        /// <returns>Store mapping record query</returns>
        IQueryable<StoreMapping> GetStoreMappingsFor(string entityName, int entityId);

        /// <summary>
        /// Save the store mapping for an entity
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="entity">The entity</param>
        /// <param name="selectedStoreIds">Array of selected store ids</param>
        void SaveStoreMappings<T>(T entity, int[] selectedStoreIds) where T : BaseEntity, IStoreMappingSupported;

        /// <summary>
        /// Inserts a store mapping record
        /// </summary>
        /// <param name="storeMapping">Store mapping</param>
        void InsertStoreMapping(StoreMapping storeMapping);

        /// <summary>
        /// Inserts a store mapping record
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="storeId">Store id</param>
        /// <param name="entity">Entity</param>
        void InsertStoreMapping<T>(T entity, int storeId) where T : BaseEntity, IStoreMappingSupported;

        /// <summary>
        /// Updates the store mapping record
        /// </summary>
        /// <param name="storeMapping">Store mapping</param>
        void UpdateStoreMapping(StoreMapping storeMapping);

        /// <summary>
        /// Find store identifiers with granted access (mapped to the entity)
        /// </summary>
        /// <param name="entityName">Entity name to check</param>
        /// <param name="entityId">Entity id to check</param>
        /// <returns>Store identifiers</returns>
        int[] GetStoresIdsWithAccess(string entityName, int entityId);

        /// <summary>
        /// Checks whether an entity can be accessed in a store (mapped to this store)
        /// </summary>
        /// <param name="entityName">Entity name to check</param>
        /// <param name="entityId">Entity id to check</param>
        /// <returns>true - authorized; otherwise, false</returns>
        bool Authorize(string entityName, int entityId);

        /// <summary>
        /// Checks whether an entity can be accessed in a store (mapped to this store)
        /// </summary>
        /// <param name="entityName">Entity name to check</param>
        /// <param name="entityId">Entity id to check</param>
        /// <param name="storeId">Store identifier to check against</param>
        /// <returns>true - authorized; otherwise, false</returns>
        bool Authorize(string entityName, int entityId, int storeId);
    }

    public static class IStoreMappingServiceExtensions
    {
        /// <summary>
        /// Find store identifiers with granted access (mapped to the entity)
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Entity</param>
        /// <returns>Store identifiers</returns>
        public static int[] GetStoresIdsWithAccess<T>(this IStoreMappingService svc, T entity) where T : BaseEntity, IStoreMappingSupported
        {
            if (entity == null)
                return new int[0];

            return svc.GetStoresIdsWithAccess(entity.GetEntityName(), entity.Id);
        }

        /// <summary>
        /// Authorize whether entity could be accessed in the current store (mapped to this store)
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Entity</param>
        /// <returns>true - authorized; otherwise, false</returns>
        public static bool Authorize<T>(this IStoreMappingService svc, T entity) where T : BaseEntity, IStoreMappingSupported
        {
            if (entity == null)
                return false;

            if (!entity.LimitedToStores)
                return true;

            return svc.Authorize(entity.GetEntityName(), entity.Id);
        }

        /// <summary>
        /// Authorize whether entity could be accessed in a store (mapped to this store)
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Entity</param>
        /// <param name="storeId">Store identifier to check against</param>
        /// <returns>true - authorized; otherwise, false</returns>
        public static bool Authorize<T>(this IStoreMappingService svc, T entity, int storeId) where T : BaseEntity, IStoreMappingSupported
        {
            if (entity == null)
                return false;

            if (!entity.LimitedToStores)
                return true;

            return svc.Authorize(entity.GetEntityName(), entity.Id, storeId);
        }
    }
}