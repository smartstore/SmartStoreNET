using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Stores;

namespace SmartStore.Services.Discounts
{
    /// <summary>
    /// Represents a discount requirement request
    /// </summary>
    public partial class CheckDiscountRequirementRequest
    {
        /// <summary>
        /// Gets or sets the discount
        /// </summary>
        public DiscountRequirement DiscountRequirement { get; set; }

        /// <summary>
        /// Gets or sets the customer
        /// </summary>
        public Customer Customer { get; set; }

		/// <summary>
		/// Gets or sets the store
		/// </summary>
		public Store Store { get; set; }
    }
}
