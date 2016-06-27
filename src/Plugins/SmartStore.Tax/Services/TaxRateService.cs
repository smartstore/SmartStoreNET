using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Tax.Domain;

namespace SmartStore.Tax.Services
{
    /// <summary>
    /// Tax rate service
    /// </summary>
    public partial class TaxRateService : ITaxRateService
    {
        #region Constants
        private const string TAXRATE_ALL_KEY = "SmartStore.taxrate.all";
        private const string TAXRATE_BY_ID_KEY = "SmartStore.taxrate.id-{0}";
        private const string TAXRATE_PATTERN_KEY = "SmartStore.taxrate.";
        #endregion

        #region Fields

        private readonly IRepository<TaxRate> _taxRateRepository;
        private readonly IRequestCache _requestCache;

        #endregion

        #region Ctor

        public TaxRateService(IRequestCache requestCache, IRepository<TaxRate> taxRateRepository)
        {
            this._requestCache = requestCache;
            this._taxRateRepository = taxRateRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a tax rate
        /// </summary>
        /// <param name="taxRate">Tax rate</param>
        public virtual void DeleteTaxRate(TaxRate taxRate)
        {
            if (taxRate == null)
                throw new ArgumentNullException("taxRate");

            _taxRateRepository.Delete(taxRate);

            _requestCache.RemoveByPattern(TAXRATE_PATTERN_KEY);
        }

        /// <summary>
        /// Gets all tax rates
        /// </summary>
        /// <returns>Tax rates</returns>
        public virtual IList<TaxRate> GetAllTaxRates()
        {
            string key = TAXRATE_ALL_KEY;
            return _requestCache.Get(key, () =>
            {
                var query = from tr in _taxRateRepository.Table
                            orderby tr.CountryId, tr.StateProvinceId, tr.Zip, tr.TaxCategoryId
                            select tr;
                var taxRates = query.ToList();
                return taxRates;
            });
        }

        /// <summary>
        /// Gets all tax rates
        /// </summary>
        /// <param name="taxCategoryId">The tax category identifier</param>
        /// <param name="countryId">The country identifier</param>
        /// <param name="stateProvinceId">The state/province identifier</param>
        /// <param name="zip">The zip</param>
        /// <returns>Tax rates</returns>
        public virtual IList<TaxRate> GetAllTaxRates(int taxCategoryId, int countryId,
            int stateProvinceId, string zip)
        {
            if (zip == null)
                zip = string.Empty;
            zip = zip.Trim();

            var existingRates = GetAllTaxRates().FindTaxRates(countryId, taxCategoryId);

            //filter by state/province
            var matchedByStateProvince = new List<TaxRate>();
            foreach (var taxRate in existingRates)
                if (stateProvinceId == taxRate.StateProvinceId)
                    matchedByStateProvince.Add(taxRate);


            if (matchedByStateProvince.Count == 0)
                foreach (var taxRate in existingRates)
                    if (taxRate.StateProvinceId == 0)
                        matchedByStateProvince.Add(taxRate);


            //filter by zip
            var matchedByZip = new List<TaxRate>();
            foreach (var taxRate in matchedByStateProvince)
                if ((String.IsNullOrEmpty(zip) && String.IsNullOrEmpty(taxRate.Zip)) ||
                    (zip.Equals(taxRate.Zip, StringComparison.InvariantCultureIgnoreCase)))
                    matchedByZip.Add(taxRate);

            if (matchedByZip.Count == 0)
                foreach (var taxRate in matchedByStateProvince)
                    if (String.IsNullOrWhiteSpace(taxRate.Zip))
                        matchedByZip.Add(taxRate);
                
            return matchedByZip;
        }

        /// <summary>
        /// Gets a tax rate
        /// </summary>
        /// <param name="taxRateId">Tax rate identifier</param>
        /// <returns>Tax rate</returns>
        public virtual TaxRate GetTaxRateById(int taxRateId)
        {
            if (taxRateId == 0)
                return null;

            string key = string.Format(TAXRATE_BY_ID_KEY, taxRateId);
            return _requestCache.Get(key, () =>
            {
                var taxRate = _taxRateRepository.GetById(taxRateId);
                return taxRate;
            });
        }

        /// <summary>
        /// Inserts a tax rate
        /// </summary>
        /// <param name="taxRate">Tax rate</param>
        public virtual void InsertTaxRate(TaxRate taxRate)
        {
            if (taxRate == null)
                throw new ArgumentNullException("taxRate");

            _taxRateRepository.Insert(taxRate);

            _requestCache.RemoveByPattern(TAXRATE_PATTERN_KEY);
        }

        /// <summary>
        /// Updates the tax rate
        /// </summary>
        /// <param name="taxRate">Tax rate</param>
        public virtual void UpdateTaxRate(TaxRate taxRate)
        {
            if (taxRate == null)
                throw new ArgumentNullException("taxRate");

            _taxRateRepository.Update(taxRate);

            _requestCache.RemoveByPattern(TAXRATE_PATTERN_KEY);
        }
        #endregion
    }
}
