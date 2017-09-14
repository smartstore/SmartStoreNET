using System.Runtime.Serialization;
using SmartStore.Core.Domain.Customers;

namespace SmartStore.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a tier price
    /// </summary>
    [DataContract]
	public partial class TierPrice : BaseEntity
    {
        /// <summary>
        /// Gets or sets the product identifier
        /// </summary>
		[DataMember]
		public int ProductId { get; set; }

		/// <summary>
		/// Gets or sets the store identifier (0 - all stores)
		/// </summary>
		[DataMember]
		public int StoreId { get; set; }

        /// <summary>
        /// Gets or sets the customer role identifier
        /// </summary>
		[DataMember]
		public int? CustomerRoleId { get; set; }

        /// <summary>
        /// Gets or sets the quantity
        /// </summary>
		[DataMember]
		public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the price
        /// </summary>
		[DataMember]
		public decimal Price { get; set; }

        /// <summary>
        /// Gets or sets the product
        /// </summary>
        [DataMember]
        public TierPriceCalculationMethod CalculationMethod { get; set; }

        /// <summary>
        /// Gets or sets the product
        /// </summary>
        public virtual Product Product { get; set; }

        /// <summary>
        /// Gets or sets the customer role
        /// </summary>
        public virtual CustomerRole CustomerRole { get; set; }
    }

    public enum TierPriceCalculationMethod
    {
        Fixed = 0,
        Percental = 5,
        Adjustment = 10
    }
}
