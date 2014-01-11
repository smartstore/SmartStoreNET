namespace SmartStore.Core.Domain.Discounts
{
    /// <summary>
    /// Represents a discount requirement
    /// </summary>
	/// <remarks>TODO: more abstract data structuring cause properties are plugin dependent.</remarks>
    public partial class DiscountRequirement : BaseEntity
    {
        /// <summary>
        /// Gets or sets the discount identifier
        /// </summary>
        public int DiscountId { get; set; }
        
        /// <summary>
        /// Gets or sets the discount requirement rule system name
        /// </summary>
        public string DiscountRequirementRuleSystemName { get; set; }

        /// <summary>
        /// Gets or sets the the discount requirement spent amount - customer had spent/purchased x.xx amount (used when requirement is set to "Customer had spent/purchased x.xx amount")
        /// </summary>
        public decimal SpentAmount { get; set; }

        /// <summary>
        /// Gets or sets the discount requirement - customer's billing country is... (used when requirement is set to "Billing country is")
        /// </summary>
        public int BillingCountryId { get; set; }

        /// <summary>
        /// Gets or sets the discount requirement - customer's shipping country is... (used when requirement is set to "Shipping country is")
        /// </summary>
        public int ShippingCountryId { get; set; }

        /// <summary>
        /// Gets or sets the restricted customer role identifier
        /// </summary>
        public int? RestrictedToCustomerRoleId { get; set; }

        /// <summary>
        /// Gets or sets the restricted product identifiers (comma separated)
        /// </summary>
        public string RestrictedProductIds { get; set; }

		/// <summary>
		/// Gets or sets the restricted payment methods (comma separated)
		/// </summary>
		public string RestrictedPaymentMethods { get; set; }

		/// <summary>
		/// Gets or sets the restricted shipping options (comma separated)
		/// </summary>
		public string RestrictedShippingOptions { get; set; }

		/// <summary>
		/// Gets or sets the restricted store identifier
		/// </summary>
		public int? RestrictedToStoreId { get; set; }

        /// <summary>
        /// Gets or sets extra data for use with the discount requirement (could be JSON, XML or any arbitrary deserializable string expression)
        /// </summary>
        public string ExtraData { get; set; }
        
        /// <summary>
        /// Gets or sets the discount
        /// </summary>
        public virtual Discount Discount { get; set; }

    }
}
