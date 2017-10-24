using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.ShippingByWeight.Domain;
using SmartStore.ShippingByWeight.Models;
using SmartStore.Services.Directory;
using SmartStore.Services.Shipping;
using SmartStore.Services.Stores;
using SmartStore.Utilities;

namespace SmartStore.ShippingByWeight.Services
{
    public partial class ShippingByWeightService : IShippingByWeightService
    {
        #region Fields

        private readonly IRepository<ShippingByWeightRecord> _sbwRepository;
		private readonly IStoreService _storeService;
		private readonly IShippingService _shippingService;
		private readonly ICountryService _countryService;

        #endregion

        #region Ctor

        public ShippingByWeightService(
            IRepository<ShippingByWeightRecord> sbwRepository,
			IStoreService storeService,
			IShippingService shippingService,
			ICountryService countryService)
        {
            _sbwRepository = sbwRepository;
			_storeService = storeService;
			_shippingService = shippingService;
			_countryService = countryService;
        }

        #endregion

        #region Methods

		/// <summary>
		/// Get queryable shipping by weight records
		/// </summary>
		public virtual IQueryable<ShippingByWeightRecord> GetShippingByWeightRecords()
		{
			var query = 
				from x in _sbwRepository.Table
				orderby x.StoreId, x.CountryId, x.ShippingMethodId, x.From
				select x;

			return query;
		}

		/// <summary>
		/// Get paged shipping by weight records
		/// </summary>
		public virtual IPagedList<ShippingByWeightRecord> GetShippingByWeightRecords(int pageIndex, int pageSize)
		{
			var result = new PagedList<ShippingByWeightRecord>(GetShippingByWeightRecords(), pageIndex, pageSize);
			return result;
		}

		/// <summary>
		/// Get models for shipping by weight records
		/// </summary>
		public virtual IList<ShippingByWeightModel> GetShippingByWeightModels(int pageIndex, int pageSize, out int totalCount)
		{
			// data join would be much better but not possible here cause ShippingByWeightObjectContext cannot be shared across repositories
			var records = GetShippingByWeightRecords(pageIndex, pageSize);
			totalCount = records.TotalCount;

			if (records.Count <= 0)
				return new List<ShippingByWeightModel>();

			var allStores = _storeService.GetAllStores();

			var result = records.Select(x =>
			{
				var store = allStores.FirstOrDefault(y => y.Id == x.StoreId);
				var shippingMethod = _shippingService.GetShippingMethodById(x.ShippingMethodId);
				var country = _countryService.GetCountryById(x.CountryId);

				var model = new ShippingByWeightModel
				{
					Id = x.Id,
					StoreId = x.StoreId,
					ShippingMethodId = x.ShippingMethodId,
					CountryId = x.CountryId,
					From = x.From,
					To = x.To,
                    Zip = (x.Zip.HasValue() ? x.Zip : "*"),
					UsePercentage = x.UsePercentage,
					ShippingChargePercentage = x.ShippingChargePercentage,
					ShippingChargeAmount = x.ShippingChargeAmount,
					SmallQuantitySurcharge = x.SmallQuantitySurcharge,
					SmallQuantityThreshold = x.SmallQuantityThreshold,
					StoreName = (store == null ? "*" : store.Name),
					ShippingMethodName = (shippingMethod == null ? "".NaIfEmpty() : shippingMethod.Name),
					CountryName = (country == null ? "*" : country.Name)
				};

				return model;
			})
			.ToList();

			return result;
		}

        public virtual ShippingByWeightRecord FindRecord(int shippingMethodId, int storeId, int countryId, decimal weight, string zip)
        {
            var existingRecords = GetShippingByWeightRecords()
				.Where(x => x.ShippingMethodId == shippingMethodId && weight >= x.From && weight <= x.To)
				.ToList();

			//filter by store
			var matchedByStore = new List<ShippingByWeightRecord>();
			foreach (var sbw in existingRecords)
			{
				if (storeId == sbw.StoreId)
					matchedByStore.Add(sbw);
			}

			if (matchedByStore.Count == 0)
			{
				foreach (var sbw in existingRecords)
				{
					if (sbw.StoreId == 0)
						matchedByStore.Add(sbw);
				}
			}

			//filter by country
			var matchedByCountry = new List<ShippingByWeightRecord>();
			foreach (var sbw in matchedByStore)
			{
				if (countryId == sbw.CountryId)
					matchedByCountry.Add(sbw);
			}

			if (matchedByCountry.Count == 0)
			{
				foreach (var sbw in matchedByStore)
				{
					if (sbw.CountryId == 0)
						matchedByCountry.Add(sbw);
				}
			}

            //filter by zip
            var matchedByZip = new List<ShippingByWeightRecord>();
            foreach (var sbt in matchedByCountry)
            {
                if ((zip.IsEmpty() && sbt.Zip.IsEmpty()) || (ZipMatches(zip, sbt.Zip)))
                {
                    matchedByZip.Add(sbt);
                }
            }

            return matchedByZip.LastOrDefault();
        }

        public virtual ShippingByWeightRecord GetById(int shippingByWeightRecordId)
        {
            if (shippingByWeightRecordId == 0)
                return null;

            var record = _sbwRepository.GetById(shippingByWeightRecordId);
            return record;
        }

		public virtual void DeleteShippingByWeightRecord(ShippingByWeightRecord shippingByWeightRecord)
		{
			if (shippingByWeightRecord == null)
				throw new ArgumentNullException("shippingByWeightRecord");

			_sbwRepository.Delete(shippingByWeightRecord);
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

        private bool ZipMatches(string zip, string pattern)
        {
            if (pattern.IsEmpty() || pattern == "*")
            {
                return true; // catch all
            }
            
            var patterns = pattern.Contains(",")
                ? pattern.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim())
                : new string[] { pattern };

            try
            {
                foreach (var entry in patterns)
                {
                    var wildcard = new Wildcard(entry, true);
                    if (wildcard.IsMatch(zip))
                        return true;
                }
            }
            catch
            {
                return zip.IsCaseInsensitiveEqual(pattern);
            }

            return false;
        }

        #endregion
    }
}
