using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Core.Domain.Discounts
{
	/// <summary>
	/// Represents a discount
	/// </summary>
	[DataContract]
	public partial class Discount : BaseEntity
    {
        private ICollection<DiscountRequirement> _discountRequirements;
        private ICollection<Category> _appliedToCategories;
		private ICollection<Manufacturer> _appliedToManufacturers;
		private ICollection<Product> _appliedToProducts;

        /// <summary>
        /// Gets or sets the name
        /// </summary>
		[DataMember]
		public string Name { get; set; }

        /// <summary>
        /// Gets or sets the discount type identifier
        /// </summary>
		[DataMember]
		public int DiscountTypeId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use percentage
        /// </summary>
		[DataMember]
		public bool UsePercentage { get; set; }

        /// <summary>
        /// Gets or sets the discount percentage
        /// </summary>
		[DataMember]
		public decimal DiscountPercentage { get; set; }

        /// <summary>
        /// Gets or sets the discount amount
        /// </summary>
		[DataMember]
		public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Gets or sets the discount start date and time
        /// </summary>
		[DataMember]
		public DateTime? StartDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the discount end date and time
        /// </summary>
		[DataMember]
		public DateTime? EndDateUtc { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether discount requires coupon code
        /// </summary>
		[DataMember]
		public bool RequiresCouponCode { get; set; }

        /// <summary>
        /// Gets or sets the coupon code
        /// </summary>
		[DataMember]
		public string CouponCode { get; set; }

        /// <summary>
        /// Gets or sets the discount limitation identifier
        /// </summary>
		[DataMember]
		public int DiscountLimitationId { get; set; }

        /// <summary>
        /// Gets or sets the discount limitation times (used when Limitation is set to "N Times Only" or "N Times Per Customer")
        /// </summary>
		[DataMember]
		public int LimitationTimes { get; set; }

        /// <summary>
        /// Gets or sets the discount type
        /// </summary>
		[DataMember]
		public DiscountType DiscountType
        {
            get
            {
                return (DiscountType)this.DiscountTypeId;
            }
            set
            {
                this.DiscountTypeId = (int)value;
            }
        }

        /// <summary>
        /// Gets or sets the discount limitation
        /// </summary>
		[DataMember]
		public DiscountLimitationType DiscountLimitation
        {
            get
            {
                return (DiscountLimitationType)this.DiscountLimitationId;
            }
            set
            {
                this.DiscountLimitationId = (int)value;
            }
        }

        /// <summary>
        /// Gets or sets the discount requirement
        /// </summary>
        public virtual ICollection<DiscountRequirement> DiscountRequirements
        {
			get { return _discountRequirements ?? (_discountRequirements = new HashSet<DiscountRequirement>()); }
            protected set { _discountRequirements = value; }
        }

        /// <summary>
        /// Gets or sets the categories
        /// </summary>
		[DataMember]
		public virtual ICollection<Category> AppliedToCategories
        {
			get { return _appliedToCategories ?? (_appliedToCategories = new HashSet<Category>()); }
            protected set { _appliedToCategories = value; }
        }

		/// <summary>
		/// Gets or sets the manufacturers
		/// </summary>
		[DataMember]
		public virtual ICollection<Manufacturer> AppliedToManufacturers
		{
			get { return _appliedToManufacturers ?? (_appliedToManufacturers = new HashSet<Manufacturer>()); }
			protected set { _appliedToManufacturers = value; }
		}

		/// <summary>
		/// Gets or sets the products 
		/// </summary>
		public virtual ICollection<Product> AppliedToProducts
		{
			get { return _appliedToProducts ?? (_appliedToProducts = new HashSet<Product>()); }
			protected set { _appliedToProducts = value; }
		}
    }
}
