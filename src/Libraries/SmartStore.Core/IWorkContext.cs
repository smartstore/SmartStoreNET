using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Domain.Tax;

namespace SmartStore.Core
{
    /// <summary>
    /// Work context
    /// </summary>
    public interface IWorkContext
    {
        /// <summary>
        /// Gets or sets the current customer
        /// </summary>
        Customer CurrentCustomer { get; set; }

        /// <summary>
        /// Gets or sets the original customer (in case the current one is impersonated)
        /// </summary>
        Customer OriginalCustomerIfImpersonated { get; }

		/// <summary>
		/// Gets or sets the current store
		/// </summary>
		Store CurrentStore { get; }

        /// <summary>
        /// Get or set current user working language
        /// </summary>
        Language WorkingLanguage { get; set; }

        /// <summary>
        /// Get or set current user working currency
        /// </summary>
        Currency WorkingCurrency { get; set; }

        /// <summary>
        /// Get or set current tax display type
        /// </summary>
        TaxDisplayType TaxDisplayType { get; set; }

        /// <summary>
        /// Gets the tax display type for a given customer
        /// </summary>
        TaxDisplayType GetTaxDisplayTypeFor(Customer customer);

        /// <summary>
        /// Get or set value indicating whether we're in admin area
        /// </summary>
        bool IsAdmin { get; set; }

        ///// <summary>
        ///// Get or set a value indicating whether we're in the public shop
        ///// </summary>
        //bool IsPublic { get; set; }
    }
}
