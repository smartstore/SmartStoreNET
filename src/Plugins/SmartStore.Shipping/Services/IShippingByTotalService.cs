using System.Collections.Generic;
using System.Linq;
using SmartStore.Core;
using SmartStore.Shipping.Domain;
using SmartStore.Shipping.Models;

namespace SmartStore.Shipping.Services
{
    public partial interface IShippingByTotalService
    {
        /// <summary>
        /// Get queryable shipping by total records
        /// </summary>
        IQueryable<ShippingByTotalRecord> GetShippingByTotalRecords();

        /// <summary>
        /// Get paged shipping by total records
        /// </summary>
        IPagedList<ShippingByTotalRecord> GetShippingByTotalRecords(int pageIndex, int pageSize);

        /// <summary>
        /// Get models for shipping by total records
        /// </summary>
        IList<ByTotalModel> GetShippingByTotalModels(int pageIndex, int pageSize, out int totalCount);

        /// <summary>
        /// Finds the ShippingByTotalRecord by its identifier
        /// </summary>
        /// <param name="shippingByTotalRecordId">ShippingByTotalRecord identifier</param>
        /// <returns>ShippingByTotalRecord</returns>
        ShippingByTotalRecord GetShippingByTotalRecordById(int shippingByTotalRecordId);

        /// <summary>
        /// Finds the ShippingByTotalRecord
        /// </summary>
        /// <param name="shippingMethodId">shipping method identifier</param>
        /// <param name="countryId">country identifier</param>
        /// <param name="subtotal">subtotal</param>
        /// <param name="stateProvinceId">state province identifier</param>
        /// <param name="zip">Zip code</param>
        /// <returns>ShippingByTotalRecord</returns>
		ShippingByTotalRecord FindShippingByTotalRecord(int shippingMethodId, int storeId, int countryId,
            decimal subTotal, int stateProvinceId, string zip);

        /// <summary>
        /// Deletes the ShippingByTotalRecord
        /// </summary>
        /// <param name="shippingByTotalRecord">ShippingByTotalRecord</param>
        void DeleteShippingByTotalRecord(ShippingByTotalRecord shippingByTotalRecord);

        /// <summary>
        /// Inserts the ShippingByTotalRecord
        /// </summary>
        /// <param name="shippingByTotalRecord">ShippingByTotalRecord</param>
        void InsertShippingByTotalRecord(ShippingByTotalRecord shippingByTotalRecord);

        /// <summary>
        /// Updates the ShippingByTotalRecord
        /// </summary>
        /// <param name="shippingByTotalRecord">ShippingByTotalRecord</param>
        void UpdateShippingByTotalRecord(ShippingByTotalRecord shippingByTotalRecord);
    }
}
