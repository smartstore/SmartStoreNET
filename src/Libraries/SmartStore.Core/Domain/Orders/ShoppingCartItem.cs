using System;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;

namespace SmartStore.Core.Domain.Orders
{
    /// <summary>
    /// Represents a shopping cart item
    /// </summary>
    public partial class ShoppingCartItem : BaseEntity, IAuditable
    {
        /// <summary>
        /// Gets or sets the store identifier
        /// </summary>
        public int StoreId { get; set; }

        /// <summary>
        /// The parent shopping cart item id
        /// </summary>
        public int? ParentItemId { get; set; }

        /// <summary>
        /// Gets or sets ths bundle item identifier
        /// </summary>
        public int? BundleItemId { get; set; }

        /// <summary>
        /// Gets or sets the shopping cart type identifier
        /// </summary>
        public int ShoppingCartTypeId { get; set; }

        /// <summary>
        /// Gets or sets the customer identifier
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the product identifier
        /// </summary>
		public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the product variant attributes
        /// </summary>
        public string AttributesXml { get; set; }

        /// <summary>
        /// Gets or sets the price enter by a customer
        /// </summary>
        public decimal CustomerEnteredPrice { get; set; }

        /// <summary>
        /// Gets or sets the quantity
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance update
        /// </summary>
        public DateTime UpdatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the shopping cart type
        /// </summary>
        public ShoppingCartType ShoppingCartType
        {
            get => (ShoppingCartType)this.ShoppingCartTypeId;
            set => this.ShoppingCartTypeId = (int)value;
        }

        /// <summary>
        /// Gets or sets the product
        /// </summary>
		public virtual Product Product { get; set; }

        /// <summary>
        /// Gets or sets the customer
        /// </summary>
        public virtual Customer Customer { get; set; }

        /// <summary>
        /// Gets or sets the product bundle item
        /// </summary>
        public virtual ProductBundleItem BundleItem { get; set; }

        /// <summary>
        /// Gets a value indicating whether the shopping cart item is free shipping
        /// </summary>
        public bool IsFreeShipping
        {
            get
            {
                var product = this.Product;
                if (product != null)
                    return product.IsFreeShipping;
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the shopping cart item is ship enabled
        /// </summary>
        public bool IsShipEnabled
        {
            get
            {
                var product = this.Product;
                if (product != null)
                    return product.IsShipEnabled;
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the shopping cart item is tax exempt
        /// </summary>
        public bool IsTaxExempt
        {
            get
            {
                var product = this.Product;
                if (product != null)
                    return product.IsTaxExempt;
                return false;
            }
        }
    }
}
