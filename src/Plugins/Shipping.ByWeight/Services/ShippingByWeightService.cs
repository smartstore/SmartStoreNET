using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Plugin.Shipping.ByWeight.Domain;

namespace SmartStore.Plugin.Shipping.ByWeight.Services
{
    public partial class ShippingByWeightService : IShippingByWeightService
    {
        #region Fields

        private readonly IRepository<ShippingByWeightRecord> _sbwRepository;
        private readonly ICacheManager _cacheManager;

        #endregion

        #region Ctor

        public ShippingByWeightService(ICacheManager cacheManager,
            IRepository<ShippingByWeightRecord> sbwRepository)
        {
            this._cacheManager = cacheManager;
            this._sbwRepository = sbwRepository;
        }

        #endregion

        #region Methods

        
        public virtual void DeleteShippingByWeightRecord(ShippingByWeightRecord shippingByWeightRecord)
        {
            if (shippingByWeightRecord == null)
                throw new ArgumentNullException("shippingByWeightRecord");

            _sbwRepository.Delete(shippingByWeightRecord);
        }

        public virtual IList<ShippingByWeightRecord> GetAll()
        {
            var query = from sbw in _sbwRepository.Table
						orderby sbw.StoreId, sbw.CountryId, sbw.ShippingMethodId, sbw.From
                        select sbw;
            var records = query.ToList();
            return records;
        }

		public virtual ShippingByWeightRecord FindRecord(int shippingMethodId, int storeId, int countryId, decimal weight)
        {
            var query = from sbw in _sbwRepository.Table
                        where sbw.ShippingMethodId == shippingMethodId && weight >= sbw.From && weight <= sbw.To
						orderby sbw.StoreId, sbw.CountryId, sbw.ShippingMethodId, sbw.From
                        select sbw;

            var existingRecords = query.ToList();

			//filter by store
			var matchedByStore = new List<ShippingByWeightRecord>();
			foreach (var sbw in existingRecords)
				if (storeId == sbw.StoreId)
					matchedByStore.Add(sbw);
			if (matchedByStore.Count == 0)
				foreach (var sbw in existingRecords)
					if (sbw.StoreId == 0)
						matchedByStore.Add(sbw);

			//filter by country
			var matchedByCountry = new List<ShippingByWeightRecord>();
			foreach (var sbw in matchedByStore)
				if (countryId == sbw.CountryId)
					matchedByCountry.Add(sbw);
			if (matchedByCountry.Count == 0)
				foreach (var sbw in matchedByStore)
					if (sbw.CountryId == 0)
						matchedByCountry.Add(sbw);

			return matchedByCountry.FirstOrDefault();
        }

        public virtual ShippingByWeightRecord GetById(int shippingByWeightRecordId)
        {
            if (shippingByWeightRecordId == 0)
                return null;

            var record = _sbwRepository.GetById(shippingByWeightRecordId);
            return record;
        }

        public virtual void InsertShippingByWeightRecord(ShippingByWeightRecord shippingByWeightRecord)
        {
            if (shippingByWeightRecord == null)
                throw new ArgumentNullException("shippingByWeightRecord");

            _sbwRepository.Insert(shippingByWeightRecord);
        }

        public virtual void UpdateShippingByWeightRecord(ShippingByWeightRecord shippingByWeightRecord)
        {
            if (shippingByWeightRecord == null)
                throw new ArgumentNullException("shippingByWeightRecord");

            _sbwRepository.Update(shippingByWeightRecord);
        }

        #endregion
    }
}
