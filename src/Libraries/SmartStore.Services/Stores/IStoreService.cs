using System.Collections.Generic;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Domain.Security;

namespace SmartStore.Services.Stores
{
    /// <summary>
    /// Store service interface
    /// </summary>
    public partial interface IStoreService
    {
        /// <summary>
        /// Deletes a store
        /// </summary>
        /// <param name="store">Store</param>
        void DeleteStore(Store store);

        /// <summary>
        /// Gets all stores
        /// </summary>
        /// <returns>Store collection</returns>
        IList<Store> GetAllStores();

        /// <summary>
        /// Gets a store 
        /// </summary>
        /// <param name="storeId">Store identifier</param>
        /// <returns>Store</returns>
        Store GetStoreById(int storeId);

        /// <summary>
        /// Inserts a store
        /// </summary>
        /// <param name="store">Store</param>
        void InsertStore(Store store);

        /// <summary>
        /// Updates the store
        /// </summary>
        /// <param name="store">Store</param>
        void UpdateStore(Store store);

        /// <summary>
        /// True if there's only one store. Otherwise False.
        /// </summary>
        bool IsSingleStoreMode();

        /// <summary>
        /// True if the store data is valid. Otherwise False.
        /// </summary>
        /// <param name="store">Store entity</param>
        bool IsStoreDataValid(Store store);

        /// <summary>
        /// Gets the store host name
        /// </summary>
        /// <param name="store">The store to get the host name for</param>
        /// <param name="secure">
        /// If <c>null</c>, checks whether all pages should be secured per <see cref="SecuritySettings.ForceSslForAllPages"/>.
        /// If <c>true</c>, returns the secure url, but only if SSL is enabled for the store.
        /// </param>
        /// <returns>The host name</returns>
        string GetHost(Store store, bool? secure = null);
    }
}