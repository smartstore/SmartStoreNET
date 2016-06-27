using System.Collections.Generic;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Stores;

namespace SmartStore.Core.Domain.Orders
{
	/// <summary>
	/// Represents a checkout attribute
	/// </summary>
	public partial class CheckoutAttribute : BaseEntity, ILocalizedEntity, IStoreMappingSupported
	{
        private ICollection<CheckoutAttributeValue> _checkoutAttributeValues;

		public CheckoutAttribute()
		{
			this.IsActive = true;
		}

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the text prompt
        /// </summary>
        public string TextPrompt { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is required
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether shippable products are required in order to display this attribute
        /// </summary>
        public bool ShippableProductRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the attribute is marked as tax exempt
        /// </summary>
        public bool IsTaxExempt { get; set; }

        /// <summary>
        /// Gets or sets the tax category identifier
        /// </summary>
        public int TaxCategoryId { get; set; }

        /// <summary>
        /// Gets or sets the attribute control type identifier
        /// </summary>
        public int AttributeControlTypeId { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }

		/// <summary>
		/// Gets or sets whether the checkout attribute is active
		/// </summary>
		public bool IsActive { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the entity is limited/restricted to certain stores
		/// </summary>
		public bool LimitedToStores { get; set; }

		/// <summary>
		/// Gets the attribute control type
		/// </summary>
		public AttributeControlType AttributeControlType
        {
            get
            {
                return (AttributeControlType)this.AttributeControlTypeId;
            }
            set
            {
                this.AttributeControlTypeId = (int)value;
            }
        }

        /// <summary>
        /// Gets the checkout attribute values
        /// </summary>
        public virtual ICollection<CheckoutAttributeValue> CheckoutAttributeValues
        {
			get { return _checkoutAttributeValues ?? (_checkoutAttributeValues = new HashSet<CheckoutAttributeValue>()); }
            protected set { _checkoutAttributeValues = value; }
        }
    }

}
