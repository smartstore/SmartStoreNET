using System.Collections.Generic;
using System.Linq;
using SmartStore.Core;
using SmartStore.Plugin.Shipping.ByWeight.Domain;
using SmartStore.Plugin.Shipping.ByWeight.Models;

namespace SmartStore.Plugin.Shipping.ByWeight.Services
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

		ShippingByWeightRecord FindRecord(int shippingMethodId, int storeId, int countryId, decimal weight);

        ShippingByWeightRecord GetById(int shippingByWeightRecordId);
		
		void DeleteShippingByWeightRecord(ShippingByWeightRecord shippingByWeightRecord);

        void InsertShippingByWeightRecord(ShippingByWeightRecord shippingByWeightRecord);

        void UpdateShippingByWeightRecord(ShippingByWeightRecord shippingByWeightRecord);
    }
}
