using System.Collections.Generic;
using SmartStore.Plugin.Shipping.ByWeight.Domain;

namespace SmartStore.Plugin.Shipping.ByWeight.Services
{
    public partial interface IShippingByWeightService
    {
        void DeleteShippingByWeightRecord(ShippingByWeightRecord shippingByWeightRecord);

        IList<ShippingByWeightRecord> GetAll();

		ShippingByWeightRecord FindRecord(int shippingMethodId, int storeId, int countryId, decimal weight);

        ShippingByWeightRecord GetById(int shippingByWeightRecordId);

        void InsertShippingByWeightRecord(ShippingByWeightRecord shippingByWeightRecord);

        void UpdateShippingByWeightRecord(ShippingByWeightRecord shippingByWeightRecord);
    }
}
