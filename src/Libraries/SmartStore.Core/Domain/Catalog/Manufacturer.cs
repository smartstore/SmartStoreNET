using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Domain.Stores;

namespace SmartStore.Core.Domain.Catalog
{
	/// <summary>
	/// Represents a manufacturer
	/// </summary>
	[DataContract]
	public partial class Manufacturer : BaseEntity, IAuditable, ISoftDeletable, ILocalizedEntity, ISlugSupported, IStoreMappingSupported, IPagingOptions
	{
		private ICollection<Discount> _appliedDiscounts;

		/// <summary>
		/// Gets or sets the name
		/// </summary>
		[DataMember]
		public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description
        /// </summary>
		[DataMember]
		public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value of used manufacturer template identifier
        /// </summary>
		[DataMember]
		public int ManufacturerTemplateId { get; set; }

        /// <summary>
        /// Gets or sets the meta keywords
        /// </summary>
		[DataMember]
		public string MetaKeywords { get; set; }

        /// <summary>
        /// Gets or sets the meta description
        /// </summary>
		[DataMember]
		public string MetaDescription { get; set; }

        /// <summary>
        /// Gets or sets the meta title
        /// </summary>
		[DataMember]
		public string MetaTitle { get; set; }

        /// <summary>
        /// Gets or sets the parent picture identifier
        /// </summary>
		[DataMember]
		public int? PictureId { get; set; }

		/// <summary>
		/// Gets or sets the picture
		/// </summary>
		[DataMember]
		public virtual Picture Picture { get; set; }

        /// <summary>
        /// Gets or sets the page size
        /// </summary>
		[DataMember]
		public int? PageSize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether customers can select the page size
        /// </summary>
		[DataMember]
		public bool? AllowCustomersToSelectPageSize { get; set; }

        /// <summary>
        /// Gets or sets the available customer selectable page size options
        /// </summary>
		[DataMember]
		public string PageSizeOptions { get; set; }

        /// <summary>
        /// Gets or sets the available price ranges
        /// </summary>
		[DataMember]
		[Obsolete("Price ranges are calculated automatically since version 3")]
		[StringLength(400)]
		public string PriceRanges { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the entity is limited/restricted to certain stores
		/// </summary>
		[DataMember]
		public bool LimitedToStores { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is published
        /// </summary>
		[DataMember]
		public bool Published { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity has been deleted
        /// </summary>
		[Index]
        public bool Deleted { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
		[DataMember]
		public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
		[DataMember]
		public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance update
        /// </summary>
		[DataMember]
		public DateTime UpdatedOnUtc { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this manufacturer has discounts applied
		/// <remarks>The same as if we run Manufacturer.AppliedDiscounts.Count > 0
		/// We use this property for performance optimization:
		/// if this property is set to false, then we do not need to load Applied Discounts navigation property
		/// </remarks>
		/// </summary>
		[DataMember]
		public bool HasDiscountsApplied { get; set; }

		/// <summary>
		/// Gets or sets the collection of applied discounts
		/// </summary>
		[DataMember]
		public virtual ICollection<Discount> AppliedDiscounts
		{
			get { return _appliedDiscounts ?? (_appliedDiscounts = new HashSet<Discount>()); }
			protected set { _appliedDiscounts = value; }
		}
	}
}
