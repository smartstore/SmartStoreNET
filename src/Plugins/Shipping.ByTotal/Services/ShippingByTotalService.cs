using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Plugin.Shipping.ByTotal.Domain;
using SmartStore.Utilities;

namespace SmartStore.Plugin.Shipping.ByTotal.Services
{
    /// <summary>
    /// Shipping By Total Service
    /// </summary>
    public partial class ShippingByTotalService : IShippingByTotalService
    {
        #region Constants

        private const string SHIPPINGBYTOTAL_ALL_KEY = "SmartStore.shippingbytotal.all";
        private const string SHIPPINGBYTOTAL_PATTERN_KEY = "SmartStore.shippingbytotal.";

        #endregion

        #region Fields

        private readonly IRepository<ShippingByTotalRecord> _sbtRepository;
        private readonly ICacheManager _cacheManager;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="sbtRepository">ShippingByTotal Repository</param>
        public ShippingByTotalService(ICacheManager cacheManager,
            IRepository<ShippingByTotalRecord> sbtRepository)
        {
            this._cacheManager = cacheManager;
            this._sbtRepository = sbtRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets all the ShippingByTotalRecords
        /// </summary>
        /// <returns>ShippingByTotalRecord collection</returns>
        public virtual IList<ShippingByTotalRecord> GetAllShippingByTotalRecords()
        {
            string key = SHIPPINGBYTOTAL_ALL_KEY;
            return _cacheManager.Get(key, () =>
            {
                var query = from sbt in _sbtRepository.Table
                            orderby sbt.StoreId, sbt.CountryId, sbt.StateProvinceId, sbt.Zip, sbt.ShippingMethodId, sbt.From
                            select sbt;

                var records = query.ToList();

                return records;
            });
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
            var existingRates = GetAllShippingByTotalRecords()
                .Where(sbt => sbt.ShippingMethodId == shippingMethodId && subtotal >= sbt.From && (sbt.To == null || subtotal <= sbt.To.Value))
                .ToList();

			//filter by store
			var matchedByStore = new List<ShippingByTotalRecord>();
			foreach (var sbw in existingRates)
			{
				if (storeId == sbw.StoreId)
				{
					matchedByStore.Add(sbw);
				}
			}
			if (matchedByStore.Count == 0)
			{
				foreach (var sbw in existingRates)
				{
					if (sbw.StoreId == 0)
					{
						matchedByStore.Add(sbw);
					}
				}
			}

            //filter by country
            var matchedByCountry = new List<ShippingByTotalRecord>();
			foreach (var sbt in matchedByStore)
            {
                if (countryId == sbt.CountryId.GetValueOrDefault())
                {
                    matchedByCountry.Add(sbt);
                }
            }
            if (matchedByCountry.Count == 0)
            {
				foreach (var sbt in matchedByStore)
                {
                    if (sbt.CountryId.GetValueOrDefault() == 0)
                    {
                        matchedByCountry.Add(sbt);
                    }
                }
            }

            //filter by state/province
            var matchedByStateProvince = new List<ShippingByTotalRecord>();
            foreach (var sbt in matchedByCountry)
            {
                if (stateProvinceId == sbt.StateProvinceId.GetValueOrDefault())
                {
                    matchedByStateProvince.Add(sbt);
                }
            }
            if (matchedByStateProvince.Count == 0)
            {
                foreach (var sbw in matchedByCountry)
                {
                    if (sbw.StateProvinceId.GetValueOrDefault() == 0)
                    {
                        matchedByStateProvince.Add(sbw);
                    }
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

            //// Obsolete
            //if (matchedByZip.Count == 0)
            //{
            //    foreach (var sbt in matchedByStateProvince)
            //    {
            //        if (sbt.Zip.IsEmpty())
            //        {
            //            matchedByZip.Add(sbt);
            //        }
            //    }
            //}

            return matchedByZip.FirstOrDefault();
        }

        private bool ZipMatches(string zip, string pattern)
        {
            if (pattern.IsEmpty() || pattern == "*")
            {
                return true; // catch all
            }
            
            try
            {
                var wildcard = new Wildcard(pattern);
                return wildcard.IsMatch(zip);
            }
            catch
            {
                return zip.IsCaseInsensitiveEqual(pattern);
            }
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

            _cacheManager.RemoveByPattern(SHIPPINGBYTOTAL_PATTERN_KEY);
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

            _cacheManager.RemoveByPattern(SHIPPINGBYTOTAL_PATTERN_KEY);
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

            _cacheManager.RemoveByPattern(SHIPPINGBYTOTAL_PATTERN_KEY);
        }

        #endregion
    }
}
