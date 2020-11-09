using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Data.Caching;
using SmartStore.Tax.Domain;

namespace SmartStore.Tax.Services
{
    public partial class TaxRateService : ITaxRateService
    {
        private readonly IRepository<TaxRate> _taxRateRepository;

        public TaxRateService(IRepository<TaxRate> taxRateRepository)
        {
            this._taxRateRepository = taxRateRepository;
        }

        public virtual void DeleteTaxRate(TaxRate taxRate)
        {
            if (taxRate == null)
                throw new ArgumentNullException("taxRate");

            _taxRateRepository.Delete(taxRate);
        }

        public virtual IList<TaxRate> GetAllTaxRates()
        {
            var query = from tr in _taxRateRepository.Table
                        orderby tr.CountryId, tr.StateProvinceId, tr.Zip, tr.TaxCategoryId
                        select tr;

            var taxRates = query.ToListCached("db.taxrate.all");
            return taxRates;
        }

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

        public virtual TaxRate GetTaxRateById(int taxRateId)
        {
            if (taxRateId == 0)
                return null;

            var taxRate = _taxRateRepository.GetByIdCached(taxRateId, "db.taxrate.id-" + taxRateId);
            return taxRate;
        }

        public virtual void InsertTaxRate(TaxRate taxRate)
        {
            if (taxRate == null)
                throw new ArgumentNullException("taxRate");

            _taxRateRepository.Insert(taxRate);
        }

        public virtual void UpdateTaxRate(TaxRate taxRate)
        {
            if (taxRate == null)
                throw new ArgumentNullException("taxRate");

            _taxRateRepository.Update(taxRate);
        }
    }
}
