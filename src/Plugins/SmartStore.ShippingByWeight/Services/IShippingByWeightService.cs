using System.Collections.Generic;
using System.Linq;
using SmartStore.Core;
using SmartStore.ShippingByWeight.Domain;
using SmartStore.ShippingByWeight.Models;

namespace SmartStore.ShippingByWeight.Services
{
    public partial interface IShippingByWeightService
    {
        /// <summary>
        /// Get queryable shipping by weight records
        /// </summary>
        IQueryable<ShippingByWeightRecord> GetShippingByWeightRecords();

        /// <summary>
        /// Get paged shipping by weight records
        /// </summary>
        IPagedList<ShippingByWeightRecord> GetShippingByWeightRecords(int pageIndex, int pageSize);

        /// <summary>
        /// Get models for shipping by weight records
        /// </summary>
        IList<ShippingByWeightModel> GetShippingByWeightModels(int pageIndex, int pageSize, out int totalCount);

        ShippingByWeightRecord FindRecord(int shippingMethodId, int storeId, int countryId, decimal weight, string zip);

        ShippingByWeightRecord GetById(int shippingByWeightRecordId);

        void DeleteShippingByWeightRecord(ShippingByWeightRecord shippingByWeightRecord);

        void InsertShippingByWeightRecord(ShippingByWeightRecord shippingByWeightRecord);

        void UpdateShippingByWeightRecord(ShippingByWeightRecord shippingByWeightRecord);
    }
}
