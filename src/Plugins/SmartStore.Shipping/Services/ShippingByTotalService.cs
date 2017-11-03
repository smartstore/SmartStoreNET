using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Shipping.Domain;
using SmartStore.Shipping.Models;
using SmartStore.Services.Directory;
using SmartStore.Services.Shipping;
using SmartStore.Services.Stores;
using SmartStore.Utilities;

namespace SmartStore.Shipping.Services
{
    /// <summary>
    /// Shipping By Total Service
    /// </summary>
    public partial class ShippingByTotalService : IShippingByTotalService
    {
        #region Fields

        private readonly IRepository<ShippingByTotalRecord> _sbtRepository;
		private readonly IStoreService _storeService;
		private readonly IShippingService _shippingService;
		private readonly ICountryService _countryService;
		private readonly IStateProvinceService _stateProvinceService;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        public ShippingByTotalService(
            IRepository<ShippingByTotalRecord> sbtRepository,
			IStoreService storeService,
			IShippingService shippingService,
			ICountryService countryService,
			IStateProvinceService stateProvinceService)
        {
            _sbtRepository = sbtRepository;
			_storeService = storeService;
			_shippingService = shippingService;
			_countryService = countryService;
			_stateProvinceService = stateProvinceService;
        }

        #endregion

        #region Methods

		/// <summary>
		/// Get queryable shipping by total records
		/// </summary>
		public virtual IQueryable<ShippingByTotalRecord> GetShippingByTotalRecords()
		{
			var query =
				from x in _sbtRepository.Table
				orderby x.StoreId, x.CountryId, x.StateProvinceId, x.Zip, x.ShippingMethodId, x.From
				select x;

			return query;
		}

		/// <summary>
		/// Get paged shipping by total records
		/// </summary>
		public virtual IPagedList<ShippingByTotalRecord> GetShippingByTotalRecords(int pageIndex, int pageSize)
		{
			var result = new PagedList<ShippingByTotalRecord>(GetShippingByTotalRecords(), pageIndex, pageSize);
			return result;
		}

		/// <summary>
		/// Get models for shipping by total records
		/// </summary>
		public virtual IList<ByTotalModel> GetShippingByTotalModels(int pageIndex, int pageSize, out int totalCount)
		{
			// data join would be much better but not possible here cause ShippingByTotalObjectContext cannot be shared across repositories
			var records = GetShippingByTotalRecords(pageIndex, pageSize);
			totalCount = records.TotalCount;

			if (records.Count <= 0)
				return new List<ByTotalModel>();

			var allStores = _storeService.GetAllStores();

			var result = records.Select(x =>
				{
					var store = allStores.FirstOrDefault(y => y.Id == x.StoreId);
					var shippingMethod = _shippingService.GetShippingMethodById(x.ShippingMethodId);
					var country = _countryService.GetCountryById(x.CountryId ?? 0);
					var stateProvince = _stateProvinceService.GetStateProvinceById(x.StateProvinceId ?? 0);

					var model = new ByTotalModel()
					{
						Id = x.Id,
						StoreId = x.StoreId,
						ShippingMethodId = x.ShippingMethodId,
						CountryId = x.CountryId,
						StateProvinceId = x.StateProvinceId,
						Zip = (x.Zip.HasValue() ? x.Zip : "*"),
						From = x.From,
						To = x.To,
						UsePercentage = x.UsePercentage,
						ShippingChargePercentage = x.ShippingChargePercentage,
						ShippingChargeAmount = x.ShippingChargeAmount,
						BaseCharge = x.BaseCharge,
						MaxCharge = x.MaxCharge,
						StoreName = (store == null ? "*" : store.Name),
						ShippingMethodName = (shippingMethod == null ? "".NaIfEmpty() : shippingMethod.Name),
						CountryName = (country == null ? "*" : country.Name),
						StateProvinceName = (stateProvince ==null ? "*" : stateProvince.Name)
					};

					return model;
				})
				.ToList();

			return result;
		}

        /// <summary>
        /// Finds the ShippingByTotalRecord by its identifier
        /// </summary>
        /// <param name="shippingByTotalRecordId">ShippingByTotalRecord identifier</param>
        /// <returns>ShippingByTotalRecord</returns>
        public virtual ShippingByTotalRecord GetShippingByTotalRecordById(int shippingByTotalRecordId)
        {
            if (shippingByTotalRecordId == 0)
            {
                return null;
            }

            var record = _sbtRepository.GetById(shippingByTotalRecordId);

            return record;
        }

        /// <summary>
        /// Finds the ShippingByTotalRecord
        /// </summary>
        /// <param name="shippingMethodId">shipping method identifier</param>
        /// <param name="countryId">country identifier</param>
        /// <param name="subtotal">subtotal</param>
        /// <param name="stateProvinceId">state province identifier</param>
        /// <param name="zip">Zip code</param>
        /// <returns>ShippingByTotalRecord</returns> 
		public virtual ShippingByTotalRecord FindShippingByTotalRecord(int shippingMethodId, int storeId, int countryId, decimal subtotal, int stateProvinceId, string zip)
        {
            if (zip == null)
            {
                zip = string.Empty;
            }
            else
            {
                zip = zip.Trim();
            }

            //filter by shipping method and subtotal
            var existingRates = GetShippingByTotalRecords()
                .Where(sbt => sbt.ShippingMethodId == shippingMethodId && subtotal >= sbt.From && (sbt.To == null || subtotal <= sbt.To.Value))
                .ToList();

			//filter by store
			var matchedByStore = new List<ShippingByTotalRecord>();
			foreach (var sbw in existingRates)
			{
                if (sbw.StoreId == 0 || storeId == sbw.StoreId)
				{
					matchedByStore.Add(sbw);
				}
			}

            //filter by country
            var matchedByCountry = new List<ShippingByTotalRecord>();
			foreach (var sbt in matchedByStore)
            {
                if (sbt.CountryId.GetValueOrDefault() == 0 || countryId == sbt.CountryId.GetValueOrDefault())
                {
                    matchedByCountry.Add(sbt);
                }
            }

            //filter by state/province
            var matchedByStateProvince = new List<ShippingByTotalRecord>();
            foreach (var sbt in matchedByCountry)
            {
                if (sbt.StateProvinceId.GetValueOrDefault() == 0 || stateProvinceId == sbt.StateProvinceId.GetValueOrDefault())
                {
                    matchedByStateProvince.Add(sbt);
                }
            }

            //filter by zip
            var matchedByZip = new List<ShippingByTotalRecord>();
            foreach (var sbt in matchedByStateProvince)
            {
                if ((zip.IsEmpty() && sbt.Zip.IsEmpty()) || (ZipMatches(zip, sbt.Zip)))
                {
                    matchedByZip.Add(sbt);
                }
            }

            return matchedByZip.LastOrDefault();
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

        /// <summary>
        /// Deletes the ShippingByTotalRecord
        /// </summary>
        /// <param name="shippingByTotalRecord">ShippingByTotalRecord</param>
        public virtual void DeleteShippingByTotalRecord(ShippingByTotalRecord shippingByTotalRecord)
        {
            if (shippingByTotalRecord == null)
            {
                throw new ArgumentNullException("shippingByTotalRecord");
            }

            _sbtRepository.Delete(shippingByTotalRecord);
        }

        /// <summary>
        /// Inserts the ShippingByTotalRecord
        /// </summary>
        /// <param name="shippingByTotalRecord">ShippingByTotalRecord</param>
        public virtual void InsertShippingByTotalRecord(ShippingByTotalRecord shippingByTotalRecord)
        {
            if (shippingByTotalRecord == null)
            {
                throw new ArgumentNullException("shippingByTotalRecord");
            }

            _sbtRepository.Insert(shippingByTotalRecord);
        }

        /// <summary>
        /// Updates the ShippingByTotalRecord
        /// </summary>
        /// <param name="shippingByTotalRecord">ShippingByTotalRecord</param>
        public virtual void UpdateShippingByTotalRecord(ShippingByTotalRecord shippingByTotalRecord)
        {
            if (shippingByTotalRecord == null)
            {
                throw new ArgumentNullException("shippingByTotalRecord");
            }

            _sbtRepository.Update(shippingByTotalRecord);
        }

        #endregion
    }
}
