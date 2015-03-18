using System.Collections.Generic;
using SmartStore.Tax.Domain;

namespace SmartStore.Tax.Services
{
    /// <summary>
    /// Tax rate service interface
    /// </summary>
    public partial interface ITaxRateService
    {
        /// <summary>
        /// Deletes a tax rate
        /// </summary>
        /// <param name="taxRate">Tax rate</param>
        void DeleteTaxRate(TaxRate taxRate);

        /// <summary>
        /// Gets all tax rates
        /// </summary>
        /// <returns>Tax rates</returns>
        IList<TaxRate> GetAllTaxRates();

        /// <summary>
        /// Gets all tax rates
        /// </summary>
        /// <param name="taxCategoryId">The tax category identifier</param>
        /// <param name="countryId">The country identifier</param>
        /// <param name="stateProvinceId">The state/province identifier</param>
        /// <param name="zip">The zip</param>
        /// <returns>Tax rates</returns>
        IList<TaxRate> GetAllTaxRates(int taxCategoryId, int countryId,
            int stateProvinceId, string zip);

        /// <summary>
        /// Gets a tax rate
        /// </summary>
        /// <param name="taxRateId">Tax rate identifier</param>
        /// <returns>Tax rate</returns>
        TaxRate GetTaxRateById(int taxRateId);

        /// <summary>
        /// Inserts a tax rate
        /// </summary>
        /// <param name="taxRate">Tax rate</param>
        void InsertTaxRate(TaxRate taxRate);

        /// <summary>
        /// Updates the tax rate
        /// </summary>
        /// <param name="taxRate">Tax rate</param>
        void UpdateTaxRate(TaxRate taxRate);
    }
}
