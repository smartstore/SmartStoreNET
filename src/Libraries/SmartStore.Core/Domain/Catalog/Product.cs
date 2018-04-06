using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Domain.Stores;

namespace SmartStore.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product
    /// </summary>
    [DataContract]
	public partial class Product : BaseEntity, IAuditable, ISoftDeletable, ILocalizedEntity, ISlugSupported, IAclSupported, IStoreMappingSupported, IMergedData
	{
		#region static

		private static readonly HashSet<string> _visibilityAffectingProductProps = new HashSet<string>();

		static Product()
		{
			AddPropsToSet(_visibilityAffectingProductProps,
				x => x.AvailableEndDateTimeUtc,
				x => x.AvailableStartDateTimeUtc,
				x => x.Deleted,
				x => x.LowStockActivityId,
				x => x.LimitedToStores,
				x => x.ManageInventoryMethodId,
				x => x.MinStockQuantity,
				x => x.Published,
				x => x.SubjectToAcl,
				x => x.VisibleIndividually);
		}

		static void AddPropsToSet(HashSet<string> props, params Expression<Func<Product, object>>[] lambdas)
		{
			foreach (var lambda in lambdas)
			{
				props.Add(lambda.ExtractPropertyInfo().Name);
			}
		}

		public static HashSet<string> GetVisibilityAffectingPropertyNames()
		{
			return _visibilityAffectingProductProps;
		}

		#endregion

		private ICollection<ProductCategory> _productCategories;
        private ICollection<ProductManufacturer> _productManufacturers;
        private ICollection<ProductPicture> _productPictures;
        private ICollection<ProductReview> _productReviews;
        private ICollection<ProductSpecificationAttribute> _productSpecificationAttributes;
        private ICollection<ProductTag> _productTags;
		private ICollection<ProductVariantAttribute> _productVariantAttributes;
		private ICollection<ProductVariantAttributeCombination> _productVariantAttributeCombinations;
		private ICollection<TierPrice> _tierPrices;
		private ICollection<Discount> _appliedDiscounts;
		private ICollection<ProductBundleItem> _productBundleItems;

		private int _stockQuantity;
        private int _backorderModeId;
		private string _sku;
		private string _gtin;
		private string _manufacturerPartNumber;
		private decimal _price;
		private int? _deliveryTimeId;
        private int? _quantityUnitId;
		private decimal _length;
		private decimal _width;
		private decimal _height;
		private decimal? _basePriceAmount;
		private int? _basePriceBaseAmount;

		public bool MergedDataIgnore { get; set; }
		public Dictionary<string, object> MergedDataValues { get; set; }

		/// <summary>
		/// Gets or sets the product type identifier
		/// </summary>
		[DataMember]
		public int ProductTypeId { get; set; }

		/// <summary>
		/// Gets or sets the parent product identifier. It's used to identify associated products (only with "grouped" products)
		/// </summary>
		[DataMember]
		public int ParentGroupedProductId { get; set; }

		/// <summary>
		/// Gets or sets the values indicating whether this product is visible in catalog or search results.
		/// It's used when this product is associated to some "grouped" one
		/// This way associated products could be accessed/added/etc only from a grouped product details page
		/// </summary>
		[DataMember]
		public bool VisibleIndividually { get; set; }

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the short description
        /// </summary>
        [DataMember]
        public string ShortDescription { get; set; }

        /// <summary>
        /// Gets or sets the full description
        /// </summary>
        [DataMember]
        public string FullDescription { get; set; }

        /// <summary>
        /// Gets or sets the admin comment
        /// </summary>
		[DataMember]
		public string AdminComment { get; set; }

        /// <summary>
        /// Gets or sets a value of used product template identifier
        /// </summary>
		[DataMember]
		public int ProductTemplateId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show the product on home page
        /// </summary>
		[DataMember]
		public bool ShowOnHomePage { get; set; }

		/// <summary>
		/// Gets or sets the display order for homepage products
		/// </summary>
		[DataMember]
		public int HomePageDisplayOrder { get; set; }

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
        /// Gets or sets a value indicating whether the product allows customer reviews
        /// </summary>
		[DataMember]
		public bool AllowCustomerReviews { get; set; }

        /// <summary>
        /// Gets or sets the rating sum (approved reviews)
        /// </summary>
		[DataMember]
		public int ApprovedRatingSum { get; set; }

        /// <summary>
        /// Gets or sets the rating sum (not approved reviews)
        /// </summary>
		[DataMember]
		public int NotApprovedRatingSum { get; set; }

        /// <summary>
        /// Gets or sets the total rating votes (approved reviews)
        /// </summary>
		[DataMember]
		public int ApprovedTotalReviews { get; set; }

        /// <summary>
        /// Gets or sets the total rating votes (not approved reviews)
        /// </summary>
		[DataMember]
		public int NotApprovedTotalReviews { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is subject to ACL
        /// </summary>
		[DataMember]
		public bool SubjectToAcl { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the entity is limited/restricted to certain stores
		/// </summary>
		[DataMember]
		public bool LimitedToStores { get; set; }

		/// <summary>
		/// Gets or sets the SKU
		/// </summary>
		[DataMember]
		public string Sku
		{
			[DebuggerStepThrough]
			get
			{
				return this.GetMergedDataValue<string>(nameof(Sku), _sku);
			}
			set
			{
				_sku = value;
			}
		}

		/// <summary>
		/// Gets or sets the manufacturer part number
		/// </summary>
		[DataMember]
		[Index]
		public string ManufacturerPartNumber
		{
			[DebuggerStepThrough]
			get
			{
				return this.GetMergedDataValue<string>(nameof(ManufacturerPartNumber), _manufacturerPartNumber);
			}
			set
			{
				_manufacturerPartNumber = value;
			}
		}

		/// <summary>
		/// Gets or sets the Global Trade Item Number (GTIN). These identifiers include UPC (in North America), EAN (in Europe), JAN (in Japan), and ISBN (for books).
		/// </summary>
		[DataMember]
		[Index]
		public string Gtin
		{
			[DebuggerStepThrough]
			get
			{
				return this.GetMergedDataValue<string>(nameof(Gtin), _gtin);
			}
			set
			{
				_gtin = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the product is gift card
		/// </summary>
		[DataMember]
		public bool IsGiftCard { get; set; }

		/// <summary>
		/// Gets or sets the gift card type identifier
		/// </summary>
		[DataMember]
		public int GiftCardTypeId { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the product requires that other products are added to the cart (Product X requires Product Y)
		/// </summary>
		[DataMember]
		public bool RequireOtherProducts { get; set; }

		/// <summary>
		/// Gets or sets a required product identifiers (comma separated)
		/// </summary>
		[DataMember]
		public string RequiredProductIds { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether required products are automatically added to the cart
		/// </summary>
		[DataMember]
		public bool AutomaticallyAddRequiredProducts { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the product is a download
		/// </summary>
		[DataMember]
		public bool IsDownload { get; set; }

		/// <summary>
		/// Gets or sets the download identifier
		/// </summary>
		[DataMember]
		public int DownloadId { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this downloadable product can be downloaded unlimited number of times
		/// </summary>
		[DataMember]
		public bool UnlimitedDownloads { get; set; }

		/// <summary>
		/// Gets or sets the maximum number of downloads
		/// </summary>
		[DataMember]
		public int MaxNumberOfDownloads { get; set; }

		/// <summary>
		/// Gets or sets the number of days during customers keeps access to the file.
		/// </summary>
		[DataMember]
		public int? DownloadExpirationDays { get; set; }

		/// <summary>
		/// Gets or sets the download activation type
		/// </summary>
		[DataMember]
		public int DownloadActivationTypeId { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the product has a sample download file
		/// </summary>
		[DataMember]
		public bool HasSampleDownload { get; set; }

		/// <summary>
		/// Gets or sets the sample download identifier
		/// </summary>
		[DataMember]
		public int? SampleDownloadId { get; set; }

		/// <summary>
		/// Gets or sets the sample download
		/// </summary>
		[DataMember]
		public Download SampleDownload { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the product has user agreement
		/// </summary>
		[DataMember]
		public bool HasUserAgreement { get; set; }

		/// <summary>
		/// Gets or sets the text of license agreement
		/// </summary>
		[DataMember]
		public string UserAgreementText { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the product is recurring
		/// </summary>
		[DataMember]
		public bool IsRecurring { get; set; }

		/// <summary>
		/// Gets or sets the cycle length
		/// </summary>
		[DataMember]
		public int RecurringCycleLength { get; set; }

		/// <summary>
		/// Gets or sets the cycle period
		/// </summary>
		[DataMember]
		public int RecurringCyclePeriodId { get; set; }

		/// <summary>
		/// Gets or sets the total cycles
		/// </summary>
		[DataMember]
		public int RecurringTotalCycles { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the entity is ship enabled
		/// </summary>
		[DataMember]
		public bool IsShipEnabled { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the entity is free shipping
		/// </summary>
		[DataMember]
		public bool IsFreeShipping { get; set; }

		/// <summary>
		/// Gets or sets the additional shipping charge
		/// </summary>
		[DataMember]
		public decimal AdditionalShippingCharge { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the product is marked as tax exempt
		/// </summary>
		[DataMember]
		public bool IsTaxExempt { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the product is an electronic service
		/// bound to EU VAT regulations for digital goods.
		/// </summary>
		[DataMember]
		public bool IsEsd { get; set; }

		/// <summary>
		/// Gets or sets the tax category identifier
		/// </summary>
		[DataMember]
		public int TaxCategoryId { get; set; }

		/// <summary>
		/// Gets or sets a value indicating how to manage inventory
		/// </summary>
		[DataMember]
		public int ManageInventoryMethodId { get; set; }

		/// <summary>
		/// Gets or sets the stock quantity
		/// </summary>
		[DataMember]
		public int StockQuantity
		{
			[DebuggerStepThrough]
			get
			{
				return this.GetMergedDataValue(nameof(StockQuantity), _stockQuantity);
			}
			set
			{
				_stockQuantity = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether to display stock availability
		/// </summary>
		[DataMember]
		public bool DisplayStockAvailability { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to display stock quantity
		/// </summary>
		[DataMember]
		public bool DisplayStockQuantity { get; set; }

		/// <summary>
		/// Gets or sets the minimum stock quantity
		/// </summary>
		[DataMember]
		public int MinStockQuantity { get; set; }

		/// <summary>
		/// Gets or sets the low stock activity identifier
		/// </summary>
		[DataMember]
		public int LowStockActivityId { get; set; }

		/// <summary>
		/// Gets or sets the quantity when admin should be notified
		/// </summary>
		[DataMember]
		public int NotifyAdminForQuantityBelow { get; set; }

		/// <summary>
		/// Gets or sets a value backorder mode identifier
		/// </summary>
		[DataMember]
		public int BackorderModeId
        {
			[DebuggerStepThrough]
			get
            {
                return this.GetMergedDataValue<int>(nameof(BackorderModeId), _backorderModeId);
            }
            set
            {
                _backorderModeId = value;
            }
        }

		/// <summary>
		/// Gets or sets a value indicating whether to back in stock subscriptions are allowed
		/// </summary>
		[DataMember]
		public bool AllowBackInStockSubscriptions { get; set; }

		/// <summary>
		/// Gets or sets the order minimum quantity
		/// </summary>
		[DataMember]
		public int OrderMinimumQuantity { get; set; }

		/// <summary>
		/// Gets or sets the order maximum quantity
		/// </summary>
		[DataMember]
		public int OrderMaximumQuantity { get; set; }

        /// <summary>
        /// Gets or sets the quantity step
        /// </summary>
        [DataMember]
        public int QuantityStep { get; set; }

        /// <summary>
        /// Gets or sets the quantity control type
        /// </summary>
        [DataMember]
        public QuantityControlType QuantiyControlType { get; set; }

        /// <summary>
        /// Gets or sets a value to specify whether or not to hide the quantity input control
        /// </summary>
        [DataMember]
        public bool HideQuantityControl { get; set; }

        /// <summary>
        /// Gets or sets the comma seperated list of allowed quantities. null or empty if any quantity is allowed
        /// </summary>
        [DataMember]
		public string AllowedQuantities { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to disable buy (Add to cart) button
		/// </summary>
		[DataMember]
		public bool DisableBuyButton { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to disable "Add to wishlist" button
		/// </summary>
		[DataMember]
		public bool DisableWishlistButton { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this item is available for Pre-Order
		/// </summary>
		[DataMember]
		public bool AvailableForPreOrder { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to show "Call for Pricing" or "Call for quote" instead of price
		/// </summary>
		[DataMember]
		public bool CallForPrice { get; set; }

		/// <summary>
		/// Gets or sets the price
		/// </summary>
		[DataMember]
		public decimal Price
		{
			[DebuggerStepThrough]
			get
			{
				return this.GetMergedDataValue<decimal>(nameof(Price), _price);
			}
			set
			{
				_price = value;
			}
		}

		/// <summary>
		/// Gets or sets the old price
		/// </summary>
		[DataMember]
		public decimal OldPrice { get; set; }

		/// <summary>
		/// Gets or sets the product cost
		/// </summary>
		[DataMember]
		public decimal ProductCost { get; set; }

		/// <summary>
		/// Gets or sets the product special price
		/// </summary>
		[DataMember]
		public decimal? SpecialPrice { get; set; }

		/// <summary>
		/// Gets or sets the start date and time of the special price
		/// </summary>
		[DataMember]
		public DateTime? SpecialPriceStartDateTimeUtc { get; set; }

		/// <summary>
		/// Gets or sets the end date and time of the special price
		/// </summary>
		[DataMember]
		public DateTime? SpecialPriceEndDateTimeUtc { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether a customer enters price
		/// </summary>
		[DataMember]
		public bool CustomerEntersPrice { get; set; }

		/// <summary>
		/// Gets or sets the minimum price entered by a customer
		/// </summary>
		[DataMember]
		public decimal MinimumCustomerEnteredPrice { get; set; }

		/// <summary>
		/// Gets or sets the maximum price entered by a customer
		/// </summary>
		[DataMember]
		public decimal MaximumCustomerEnteredPrice { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this product has tier prices configured
		/// <remarks>The same as if we run this.TierPrices.Count > 0
		/// We use this property for performance optimization:
		/// if this property is set to false, then we do not need to load tier prices navigation property
		/// </remarks>
		/// </summary>
		[DataMember]
		public bool HasTierPrices { get; set; }

		/// <summary>
		/// Gets or sets a value for the lowest attribute combination price override
		/// </summary>
		[DataMember]
		public decimal? LowestAttributeCombinationPrice { get; set; }
		
		/// <summary>
		/// Gets or sets a value indicating whether this product has discounts applied
		/// <remarks>The same as if we run this.AppliedDiscounts.Count > 0
		/// We use this property for performance optimization:
		/// if this property is set to false, then we do not need to load Applied Discounts navifation property
		/// </remarks>
		/// </summary>
		[DataMember]
		public bool HasDiscountsApplied { get; set; }

		/// <summary>
		/// Gets or sets the weight
		/// </summary>
		[DataMember]
		public decimal Weight { get; set; }

		/// <summary>
		/// Gets or sets the length
		/// </summary>
		[DataMember]
		public decimal Length
		{
			[DebuggerStepThrough]
			get
			{
				return this.GetMergedDataValue<decimal>(nameof(Length), _length);
			}
			set
			{
				_length = value;
			}
		}

		/// <summary>
		/// Gets or sets the width
		/// </summary>
		[DataMember]
		public decimal Width
		{
			[DebuggerStepThrough]
			get
			{
				return this.GetMergedDataValue<decimal>(nameof(Width), _width);
			}
			set
			{
				_width = value;
			}
		}

		/// <summary>
		/// Gets or sets the height
		/// </summary>
		[DataMember]
		public decimal Height
		{
			[DebuggerStepThrough]
			get
			{
				return this.GetMergedDataValue<decimal>(nameof(Height), _height);
			}
			set
			{
				_height = value;
			}
		}

		/// <summary>
		/// Gets or sets the available start date and time
		/// </summary>
		[DataMember]
		public DateTime? AvailableStartDateTimeUtc { get; set; }

		/// <summary>
		/// Gets or sets the available end date and time
		/// </summary>
		[DataMember]
		public DateTime? AvailableEndDateTimeUtc { get; set; }

		/// <summary>
		/// Gets or sets a display order. This value is used when sorting associated products (used with "grouped" products)
		/// </summary>
		[DataMember]
		public int DisplayOrder { get; set; }

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
		/// Gets or sets a value indicating whether the entity is a system product.
		/// </summary>
		[DataMember]
		[Index("Product_SystemName_IsSystemProduct", 2)]
		public bool IsSystemProduct { get; set; }

		/// <summary>
		/// Gets or sets the product system name.
		/// </summary>
		[DataMember]
		[Index("Product_SystemName_IsSystemProduct", 1)]
		public string SystemName { get; set; }

		/// <summary>
		/// Gets or sets the date and time of product creation
		/// </summary>
		[DataMember]
		public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time of product update
        /// </summary>
		[DataMember]
		public DateTime UpdatedOnUtc { get; set; }

		/// <summary>
		/// Gets or sets the delivery time identifier
		/// </summary>
		[DataMember]
		public int? DeliveryTimeId
		{
			[DebuggerStepThrough]
			get
			{
				return this.GetMergedDataValue<int?>(nameof(DeliveryTimeId), _deliveryTimeId);
			}
			set
			{
				_deliveryTimeId = value;
			}
		}

		[DataMember]
		public virtual DeliveryTime DeliveryTime { get; set; }

        /// <summary>
        /// Gets or sets the quantity unit identifier
        /// </summary>
        [DataMember]
        public int? QuantityUnitId
        {
            [DebuggerStepThrough]
            get
            {
                return this.GetMergedDataValue<int?>(nameof(QuantityUnitId), _quantityUnitId);
            }
            set
            {
                _quantityUnitId = value;
            }
        }

        [DataMember]
        public virtual QuantityUnit QuantityUnit { get; set; }

		/// <summary>
		/// Gets or sets the customs tariff number
		/// </summary>
		[DataMember]
		public string CustomsTariffNumber { get; set; }

		/// <summary>
		/// Gets or sets the country of origin identifier
		/// </summary>
		[DataMember]
		public int? CountryOfOriginId { get; set; }

		/// <summary>
		/// Gets or sets the country of origin
		/// </summary>
		[DataMember]
		public virtual Country CountryOfOrigin { get; set; }

		/// <summary>
		/// Gets or sets if base price quotation (PAnGV) is enabled
		/// </summary>
		[DataMember]
		public bool BasePriceEnabled { get; set; }

		/// <summary>
		/// Measure unit for the base price (e.g. "kg", "g", "qm²" etc.)
		/// </summary>
		[DataMember]
		public string BasePriceMeasureUnit { get; set; }

		/// <summary>
		/// Amount of product per packing unit in the given measure unit 
        /// (e.g. 250 ml shower gel: "0.25" if MeasureUnit = "liter" and BaseAmount = 1)
		/// </summary>
		[DataMember]
		public decimal? BasePriceAmount
		{
			[DebuggerStepThrough]
			get
			{
				return this.GetMergedDataValue<decimal?>(nameof(BasePriceAmount), _basePriceAmount);
			}
			set
			{
				_basePriceAmount = value;
			}
		}

		/// <summary>
		/// Reference value for the given measure unit 
		/// (e.g. "1" liter. Formula: [BaseAmount] [MeasureUnit] = [SellingPrice] / [Amount])
		/// </summary>
		[DataMember]
		public int? BasePriceBaseAmount
		{
			[DebuggerStepThrough]
			get
			{
				return this.GetMergedDataValue<int?>(nameof(BasePriceBaseAmount), _basePriceBaseAmount);
			}
			set
			{
				_basePriceBaseAmount = value;
			}
		}

		[DataMember]
		public bool BasePriceHasValue
		{
			get
			{
				return BasePriceEnabled && BasePriceAmount.GetValueOrDefault() > 0 && BasePriceBaseAmount.GetValueOrDefault() > 0 && BasePriceMeasureUnit.HasValue();
			}
		}

		/// <summary>
		/// Optional title text of a product bundle
		/// </summary>
		[DataMember]
		public string BundleTitleText { get; set; }

		/// <summary>
		/// Per item shipping of bundle items
		/// </summary>
		[DataMember]
		public bool BundlePerItemShipping { get; set; }

		/// <summary>
		/// Per item pricing of bundle items
		/// </summary>
		[DataMember]
		public bool BundlePerItemPricing { get; set; }

		/// <summary>
		/// Per item shopping cart handling of bundle items
		/// </summary>
		[DataMember]
		public bool BundlePerItemShoppingCart { get; set; }

		/// <summary>
		/// Gets or sets the main picture id
		/// </summary>
		[DataMember]
		public int? MainPictureId { get; set; }

		/// <summary>
		/// Gets or sets the product type
		/// </summary>
		[DataMember]
		public ProductType ProductType
		{
			get
			{
				return (ProductType)this.ProductTypeId;
			}
			set
			{
				this.ProductTypeId = (int)value;
			}
		}

		public string ProductTypeLabelHint
		{
			get
			{
				switch (ProductType)
				{
					case ProductType.SimpleProduct:
						return "secondary d-none";
					case ProductType.GroupedProduct:
						return "success";
					case ProductType.BundledProduct:
						return "info";
					default:
						return "";
				}
			}
		}

		/// <summary>
		/// Gets or sets the backorder mode
		/// </summary>
		[DataMember]
		public BackorderMode BackorderMode
		{
			get
			{
				return (BackorderMode)this.BackorderModeId;
			}
			set
			{
				this.BackorderModeId = (int)value;
			}
		}

		/// <summary>
		/// Gets or sets the download activation type
		/// </summary>
		[DataMember]
		public DownloadActivationType DownloadActivationType
		{
			get
			{
				return (DownloadActivationType)this.DownloadActivationTypeId;
			}
			set
			{
				this.DownloadActivationTypeId = (int)value;
			}
		}

		/// <summary>
		/// Gets or sets the gift card type
		/// </summary>
		[DataMember]
		public GiftCardType GiftCardType
		{
			get
			{
				return (GiftCardType)this.GiftCardTypeId;
			}
			set
			{
				this.GiftCardTypeId = (int)value;
			}
		}

		/// <summary>
		/// Gets or sets the low stock activity
		/// </summary>
		[DataMember]
		public LowStockActivity LowStockActivity
		{
			get
			{
				return (LowStockActivity)this.LowStockActivityId;
			}
			set
			{
				this.LowStockActivityId = (int)value;
			}
		}

		/// <summary>
		/// Gets or sets the value indicating how to manage inventory
		/// </summary>
		[DataMember]
		public ManageInventoryMethod ManageInventoryMethod
		{
			get
			{
				return (ManageInventoryMethod)this.ManageInventoryMethodId;
			}
			set
			{
				this.ManageInventoryMethodId = (int)value;
			}
		}

		/// <summary>
		/// Gets or sets the cycle period for recurring products
		/// </summary>
		[DataMember]
		public RecurringProductCyclePeriod RecurringCyclePeriod
		{
			get
			{
				return (RecurringProductCyclePeriod)this.RecurringCyclePeriodId;
			}
			set
			{
				this.RecurringCyclePeriodId = (int)value;
			}
		}

		/// <summary>
        /// Gets or sets the collection of ProductCategory
        /// </summary>
        [DataMember]
        public virtual ICollection<ProductCategory> ProductCategories
        {
			get { return _productCategories ?? (_productCategories = new HashSet<ProductCategory>()); }
            protected set { _productCategories = value; }
        }

        /// <summary>
        /// Gets or sets the collection of ProductManufacturer
        /// </summary>
		[DataMember]
		public virtual ICollection<ProductManufacturer> ProductManufacturers
        {
			get { return _productManufacturers ?? (_productManufacturers = new HashSet<ProductManufacturer>()); }
            protected set { _productManufacturers = value; }
        }

        /// <summary>
        /// Gets or sets the collection of ProductPicture
        /// </summary>
		[DataMember]
		public virtual ICollection<ProductPicture> ProductPictures
        {
            get { return _productPictures ?? (_productPictures = new List<ProductPicture>()); }
            protected set { _productPictures = value; }
        }

        /// <summary>
        /// Gets or sets the collection of product reviews
        /// </summary>
        public virtual ICollection<ProductReview> ProductReviews
        {
			get { return _productReviews ?? (_productReviews = new HashSet<ProductReview>()); }
            protected set { _productReviews = value; }
        }

        /// <summary>
        /// Gets or sets the product specification attribute
        /// </summary>
		[DataMember]
		public virtual ICollection<ProductSpecificationAttribute> ProductSpecificationAttributes
        {
			get { return _productSpecificationAttributes ?? (_productSpecificationAttributes = new HashSet<ProductSpecificationAttribute>()); }
            protected set { _productSpecificationAttributes = value; }
        }

        /// <summary>
		/// Gets or sets the product tags
        /// </summary>
		[DataMember]
		public virtual ICollection<ProductTag> ProductTags
        {
			get { return _productTags ?? (_productTags = new HashSet<ProductTag>()); }
            protected set { _productTags = value; }
        }

		/// <summary>
		/// Gets or sets the product attributes
		/// </summary>
		[DataMember]
		public virtual ICollection<ProductVariantAttribute> ProductVariantAttributes
		{
			get { return _productVariantAttributes ?? (_productVariantAttributes = new HashSet<ProductVariantAttribute>()); }
			protected set { _productVariantAttributes = value; }
		}

		/// <summary>
		/// Gets or sets the product attribute combinations
		/// </summary>
		[DataMember]
		public virtual ICollection<ProductVariantAttributeCombination> ProductVariantAttributeCombinations
		{
			get { return _productVariantAttributeCombinations ?? (_productVariantAttributeCombinations = new List<ProductVariantAttributeCombination>()); }
			protected set { _productVariantAttributeCombinations = value; }
		}

		/// <summary>
		/// Gets or sets the tier prices
		/// </summary>
		[DataMember]
		public virtual ICollection<TierPrice> TierPrices
		{
			get { return _tierPrices ?? (_tierPrices = new HashSet<TierPrice>()); }
			protected set { _tierPrices = value; }
		}

		/// <summary>
		/// Gets or sets the collection of applied discounts
		/// </summary>
		[DataMember]
		public virtual ICollection<Discount> AppliedDiscounts
		{
			get { return _appliedDiscounts ?? (_appliedDiscounts = new HashSet<Discount>()); }
			protected set { _appliedDiscounts = value; }
		}

		/// <summary>
		/// Gets or sets the collection of product bundle items
		/// </summary>
		[DataMember]
		public virtual ICollection<ProductBundleItem> ProductBundleItems
		{
			get { return _productBundleItems ?? (_productBundleItems = new HashSet<ProductBundleItem>()); }
			protected set { _productBundleItems = value; }
		}
	}
}
