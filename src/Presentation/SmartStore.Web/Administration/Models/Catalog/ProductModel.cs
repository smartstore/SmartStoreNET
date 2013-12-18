using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation.Attributes;
using SmartStore.Admin.Models.Customers;
using SmartStore.Admin.Models.Stores;
using SmartStore.Admin.Validators.Catalog;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Mvc;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Models.Catalog
{
    [Validator(typeof(ProductValidator))]
    public class ProductModel : EntityModelBase, ILocalizedModel<ProductLocalizedModel>
    {
        public ProductModel()
        {
            Locales = new List<ProductLocalizedModel>();
            ProductPictureModels = new List<ProductPictureModel>();
            CopyProductModel = new CopyProductModel();
            AvailableProductTemplates = new List<SelectListItem>();
            AvailableProductTags = new List<SelectListItem>();
			AvailableTaxCategories = new List<SelectListItem>();
			AvailableDeliveryTimes = new List<SelectListItem>();
			AvailableMeasureUnits = new List<SelectListItem>();
			AddPictureModel = new ProductPictureModel();
			AddSpecificationAttributeModel = new AddProductSpecificationAttributeModel();
        }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.ID")]
		public override int Id { get; set; }

        //picture thumbnail
        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.PictureThumbnailUrl")]
        public string PictureThumbnailUrl { get; set; }
        public bool NoThumb { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.ShortDescription")]
        [AllowHtml]
        public string ShortDescription { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.FullDescription")]
        [AllowHtml]
        public string FullDescription { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.AdminComment")]
        [AllowHtml]
        public string AdminComment { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.ProductTemplate")]
        [AllowHtml]
        public int ProductTemplateId { get; set; }
        public IList<SelectListItem> AvailableProductTemplates { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.ShowOnHomePage")]
        public bool ShowOnHomePage { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.MetaKeywords")]
        [AllowHtml]
        public string MetaKeywords { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.MetaDescription")]
        [AllowHtml]
        public string MetaDescription { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.MetaTitle")]
        [AllowHtml]
        public string MetaTitle { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.SeName")]
        [AllowHtml]
        public string SeName { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.AllowCustomerReviews")]
        public bool AllowCustomerReviews { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.ProductTags")]
        public string ProductTags { get; set; }
        public IList<SelectListItem> AvailableProductTags { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.Sku")]
		[AllowHtml]
		public string Sku { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.ManufacturerPartNumber")]
		[AllowHtml]
		public string ManufacturerPartNumber { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.GTIN")]
		[AllowHtml]
		public virtual string Gtin { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.IsGiftCard")]
		public bool IsGiftCard { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.GiftCardType")]
		public int GiftCardTypeId { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.RequireOtherProducts")]
		public bool RequireOtherProducts { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.RequiredProductVariantIds")]
		public string RequiredProductIds { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.AutomaticallyAddRequiredProductVariants")]
		public bool AutomaticallyAddRequiredProducts { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.IsDownload")]
		public bool IsDownload { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.Download")]
		[UIHint("Download")]
		public int DownloadId { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.UnlimitedDownloads")]
		public bool UnlimitedDownloads { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.MaxNumberOfDownloads")]
		public int MaxNumberOfDownloads { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.DownloadExpirationDays")]
		public int? DownloadExpirationDays { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.DownloadActivationType")]
		public int DownloadActivationTypeId { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.HasSampleDownload")]
		public bool HasSampleDownload { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.SampleDownload")]
		[UIHint("Download")]
		public int SampleDownloadId { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.HasUserAgreement")]
		public bool HasUserAgreement { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.UserAgreementText")]
		[AllowHtml]
		public string UserAgreementText { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.IsRecurring")]
		public bool IsRecurring { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.RecurringCycleLength")]
		public int RecurringCycleLength { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.RecurringCyclePeriod")]
		public int RecurringCyclePeriodId { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.RecurringTotalCycles")]
		public int RecurringTotalCycles { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.IsShipEnabled")]
		public bool IsShipEnabled { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.IsFreeShipping")]
		public bool IsFreeShipping { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.AdditionalShippingCharge")]
		public decimal AdditionalShippingCharge { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.IsTaxExempt")]
		public bool IsTaxExempt { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.TaxCategory")]
		public int TaxCategoryId { get; set; }
		public IList<SelectListItem> AvailableTaxCategories { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.ManageInventoryMethod")]
		public int ManageInventoryMethodId { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.StockQuantity")]
		public int StockQuantity { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.DisplayStockAvailability")]
		public bool DisplayStockAvailability { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.DisplayStockQuantity")]
		public bool DisplayStockQuantity { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.MinStockQuantity")]
		public int MinStockQuantity { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.LowStockActivity")]
		public int LowStockActivityId { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.NotifyAdminForQuantityBelow")]
		public int NotifyAdminForQuantityBelow { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.BackorderMode")]
		public int BackorderModeId { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.AllowBackInStockSubscriptions")]
		public bool AllowBackInStockSubscriptions { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.OrderMinimumQuantity")]
		public int OrderMinimumQuantity { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.OrderMaximumQuantity")]
		public int OrderMaximumQuantity { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.AllowedQuantities")]
		public string AllowedQuantities { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.DisableBuyButton")]
		public bool DisableBuyButton { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.DisableWishlistButton")]
		public bool DisableWishlistButton { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.AvailableForPreOrder")]
		public bool AvailableForPreOrder { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.CallForPrice")]
		public bool CallForPrice { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.Price")]
		public decimal Price { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.OldPrice")]
		public decimal OldPrice { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.ProductCost")]
		public decimal ProductCost { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.SpecialPrice")]
		[UIHint("DecimalNullable")]
		public decimal? SpecialPrice { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.SpecialPriceStartDateTimeUtc")]
		[UIHint("DateTimeNullable")]
		public DateTime? SpecialPriceStartDateTimeUtc { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.SpecialPriceEndDateTimeUtc")]
		[UIHint("DateTimeNullable")]
		public DateTime? SpecialPriceEndDateTimeUtc { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.CustomerEntersPrice")]
		public bool CustomerEntersPrice { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.MinimumCustomerEnteredPrice")]
		public decimal MinimumCustomerEnteredPrice { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.MaximumCustomerEnteredPrice")]
		public decimal MaximumCustomerEnteredPrice { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.Weight")]
		public decimal Weight { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.Length")]
		public decimal Length { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.Width")]
		public decimal Width { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.Height")]
		public decimal Height { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.AvailableStartDateTime")]
		[UIHint("DateTimeNullable")]
		public DateTime? AvailableStartDateTimeUtc { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.AvailableEndDateTime")]
		[UIHint("DateTimeNullable")]
		public DateTime? AvailableEndDateTimeUtc { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.Published")]
        public bool Published { get; set; }

		public string PrimaryStoreCurrencyCode { get; set; }
		public string BaseDimensionIn { get; set; }
		public string BaseWeightIn { get; set; }

        public IList<ProductLocalizedModel> Locales { get; set; }

        //ACL (customer roles)
        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.SubjectToAcl")]
        public bool SubjectToAcl { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.AclCustomerRoles")]
        public List<CustomerRoleModel> AvailableCustomerRoles { get; set; }
        public int[] SelectedCustomerRoleIds { get; set; }

		//Store mapping
		[SmartResourceDisplayName("Admin.Common.Store.LimitedTo")]
		public bool LimitedToStores { get; set; }

		[SmartResourceDisplayName("Admin.Common.Store.AvailableFor")]
		public List<StoreModel> AvailableStores { get; set; }
		public int[] SelectedStoreIds { get; set; }

        //categories
        public int NumberOfAvailableCategories { get; set; }

        //manufacturers
        public int NumberOfAvailableManufacturers { get; set; }

		//product attributes
		public int NumberOfAvailableProductAttributes { get; set; }

        //pictures
        public ProductPictureModel AddPictureModel { get; set; }
        public IList<ProductPictureModel> ProductPictureModels { get; set; }

		//discounts
		public List<Discount> AvailableDiscounts { get; set; }
		public int[] SelectedDiscountIds { get; set; }

		//add specification attribute model
        public AddProductSpecificationAttributeModel AddSpecificationAttributeModel { get; set; }

        //copy product
        public CopyProductModel CopyProductModel { get; set; }

		//BasePrice
		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.BasePriceEnabled")]
		public bool BasePriceEnabled { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.BasePriceMeasureUnit")]
		public string BasePriceMeasureUnit { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.BasePriceAmount")]
		public decimal? BasePriceAmount { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.BasePriceBaseAmount")]
		public int? BasePriceBaseAmount { get; set; }

		public IList<SelectListItem> AvailableMeasureUnits { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.DeliveryTime")]
		public int? DeliveryTimeId { get; set; }
		public IList<SelectListItem> AvailableDeliveryTimes { get; set; }
        
        #region Nested classes
        
        public class AddProductSpecificationAttributeModel : EntityModelBase
        {
            public AddProductSpecificationAttributeModel()
            {
                AvailableAttributes = new List<SelectListItem>();
                AvailableOptions = new List<SelectListItem>();

				AllowFiltering = true;		// codehint: sm-add
            }
            
            [SmartResourceDisplayName("Admin.Catalog.Products.SpecificationAttributes.Fields.SpecificationAttribute")]
            public int SpecificationAttributeId { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.SpecificationAttributes.Fields.SpecificationAttributeOption")]
            public int SpecificationAttributeOptionId { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.SpecificationAttributes.Fields.AllowFiltering")]
            public bool AllowFiltering { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.SpecificationAttributes.Fields.ShowOnProductPage")]
            public bool ShowOnProductPage { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.SpecificationAttributes.Fields.DisplayOrder")]
            public int DisplayOrder { get; set; }

            public IList<SelectListItem> AvailableAttributes { get; set; }
            public IList<SelectListItem> AvailableOptions { get; set; }
        }
        
        public class ProductPictureModel : EntityModelBase
        {
            public int ProductId { get; set; }

            [UIHint("Picture")]
            [SmartResourceDisplayName("Admin.Catalog.Products.Pictures.Fields.Picture")]
            public int PictureId { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.Pictures.Fields.Picture")]
            public string PictureUrl { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.Pictures.Fields.DisplayOrder")]
            public int DisplayOrder { get; set; }
        }
        
        public class ProductCategoryModel : EntityModelBase
        {
            [SmartResourceDisplayName("Admin.Catalog.Products.Categories.Fields.Category")]
            [UIHint("ProductCategory")]
            public string Category { get; set; }

            public int ProductId { get; set; }

            public int CategoryId { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.Categories.Fields.IsFeaturedProduct")]
            public bool IsFeaturedProduct { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.Categories.Fields.DisplayOrder")]
            public int DisplayOrder { get; set; }
        }

        public class ProductManufacturerModel : EntityModelBase
        {
            [SmartResourceDisplayName("Admin.Catalog.Products.Manufacturers.Fields.Manufacturer")]
            [UIHint("ProductManufacturer")]
            public string Manufacturer { get; set; }

            public int ProductId { get; set; }

            public int ManufacturerId { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.Manufacturers.Fields.IsFeaturedProduct")]
            public bool IsFeaturedProduct { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.Manufacturers.Fields.DisplayOrder")]
            public int DisplayOrder { get; set; }
        }

        public class RelatedProductModel : EntityModelBase
        {
            public int ProductId1 { get; set; }

            public int ProductId2 { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.RelatedProducts.Fields.Product")]
            public string Product2Name { get; set; }
            
            [SmartResourceDisplayName("Admin.Catalog.Products.RelatedProducts.Fields.DisplayOrder")]
            public int DisplayOrder { get; set; }
        }

        public class AddRelatedProductModel : ModelBase
        {
            public AddRelatedProductModel()
            {
                AvailableCategories = new List<SelectListItem>();
                AvailableManufacturers = new List<SelectListItem>();
            }
            public GridModel<ProductModel> Products { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.List.SearchProductName")]
            [AllowHtml]
            public string SearchProductName { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.List.SearchCategory")]
            public int SearchCategoryId { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.List.SearchManufacturer")]
            public int SearchManufacturerId { get; set; }

            public IList<SelectListItem> AvailableCategories { get; set; }
            public IList<SelectListItem> AvailableManufacturers { get; set; }

            public int ProductId { get; set; }

            public int[] SelectedProductIds { get; set; }
        }

        public class CrossSellProductModel : EntityModelBase
        {
            public int ProductId1 { get; set; }

            public int ProductId2 { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.CrossSells.Fields.Product")]
            public string Product2Name { get; set; }
        }

        public class AddCrossSellProductModel : ModelBase
        {
            public AddCrossSellProductModel()
            {
                AvailableCategories = new List<SelectListItem>();
                AvailableManufacturers = new List<SelectListItem>();
            }
            public GridModel<ProductModel> Products { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.List.SearchProductName")]
            [AllowHtml]
            public string SearchProductName { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.List.SearchCategory")]
            public int SearchCategoryId { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.List.SearchManufacturer")]
            public int SearchManufacturerId { get; set; }

            public IList<SelectListItem> AvailableCategories { get; set; }
            public IList<SelectListItem> AvailableManufacturers { get; set; }

            public int ProductId { get; set; }

            public int[] SelectedProductIds { get; set; }
        }

		public class TierPriceModel : EntityModelBase
		{
			public int ProductId { get; set; }

			public int CustomerRoleId { get; set; }
			[SmartResourceDisplayName("Admin.Catalog.Products.Variants.TierPrices.Fields.CustomerRole")]
			[UIHint("TierPriceCustomer")]
			public string CustomerRole { get; set; }


			public int StoreId { get; set; }
			[SmartResourceDisplayName("Admin.Catalog.Products.Variants.TierPrices.Fields.Store")]
			[UIHint("TierPriceStore")]
			public string Store { get; set; }

			[SmartResourceDisplayName("Admin.Catalog.Products.Variants.TierPrices.Fields.Quantity")]
			public int Quantity { get; set; }

			[SmartResourceDisplayName("Admin.Catalog.Products.Variants.TierPrices.Fields.Price")]
			//we don't name it Price because Telerik has a small bug 
			//"if we have one more editor with the same name on a page, it doesn't allow editing"
			//in our case it's productVariant.Price1
			public decimal Price1 { get; set; }
		}

		public class ProductVariantAttributeModel : EntityModelBase
		{
			public int ProductId { get; set; }

			public int ProductAttributeId { get; set; }
			[SmartResourceDisplayName("Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Fields.Attribute")]
			[UIHint("ProductAttribute")]
			public string ProductAttribute { get; set; }

			[SmartResourceDisplayName("Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Fields.TextPrompt")]
			[AllowHtml]
			public string TextPrompt { get; set; }

			[SmartResourceDisplayName("Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Fields.IsRequired")]
			public bool IsRequired { get; set; }

			public int AttributeControlTypeId { get; set; }
			[SmartResourceDisplayName("Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Fields.AttributeControlType")]
			[UIHint("AttributeControlType")]
			public string AttributeControlType { get; set; }

			[SmartResourceDisplayName("Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Fields.DisplayOrder")]
			//we don't name it DisplayOrder because Telerik has a small bug 
			//"if we have one more editor with the same name on a page, it doesn't allow editing"
			//in our case it's category.DisplayOrder
			public int DisplayOrder1 { get; set; }

			public string ViewEditUrl { get; set; }
			public string ViewEditText { get; set; }
		}

		public class ProductVariantAttributeValueListModel : ModelBase
		{
			public int ProductId { get; set; }

			public string ProductName { get; set; }

			public int ProductVariantAttributeId { get; set; }

			public string ProductVariantAttributeName { get; set; }
		}

		[Validator(typeof(ProductVariantAttributeValueModelValidator))]
		public class ProductVariantAttributeValueModel : EntityModelBase, ILocalizedModel<ProductVariantAttributeValueLocalizedModel>
		{
			public ProductVariantAttributeValueModel()
			{
				Locales = new List<ProductVariantAttributeValueLocalizedModel>();
			}

			public int ProductVariantAttributeId { get; set; }

			[SmartResourceDisplayName("Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Values.Fields.Alias")]
			public string Alias { get; set; }

			[SmartResourceDisplayName("Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Values.Fields.Name")]
			[AllowHtml]
			public string Name { get; set; }

			[SmartResourceDisplayName("Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Values.Fields.ColorSquaresRgb")]
			[AllowHtml, UIHint("Color")]
			public string ColorSquaresRgb { get; set; }
			public bool DisplayColorSquaresRgb { get; set; }

			[SmartResourceDisplayName("Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Values.Fields.PriceAdjustment")]
			public decimal PriceAdjustment { get; set; }

			[SmartResourceDisplayName("Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Values.Fields.WeightAdjustment")]
			public decimal WeightAdjustment { get; set; }

			[SmartResourceDisplayName("Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Values.Fields.IsPreSelected")]
			public bool IsPreSelected { get; set; }

			[SmartResourceDisplayName("Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Values.Fields.DisplayOrder")]
			public int DisplayOrder { get; set; }

			public IList<ProductVariantAttributeValueLocalizedModel> Locales { get; set; }
		}

		public class ProductVariantAttributeValueLocalizedModel : ILocalizedModelLocal
		{
			public int LanguageId { get; set; }

			[SmartResourceDisplayName("Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Values.Fields.Name")]
			[AllowHtml]
			public string Name { get; set; }
		}

		//public class ProductVariantAttributeCombinationModel : EntityModelBase
		//{
		//	public int ProductId { get; set; }

		//	[SmartResourceDisplayName("Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.Fields.Attributes")]
		//	[AllowHtml]
		//	public string AttributesXml { get; set; }

		//	[AllowHtml]
		//	public string Warnings { get; set; }

		//	[SmartResourceDisplayName("Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.Fields.StockQuantity")]
		//	//we don't name it StockQuantity because Telerik has a small bug 
		//	//"if we have one more editor with the same name on a page, it doesn't allow editing"
		//	//in our case it's productVariant.StockQuantity1
		//	public int StockQuantity1 { get; set; }

		//	[SmartResourceDisplayName("Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.Fields.AllowOutOfStockOrders")]
		//	//we don't name it AllowOutOfStockOrders because Telerik has a small bug 
		//	//"if we have one more editor with the same name on a page, it doesn't allow editing"
		//	//in our case it's productVariant.AllowOutOfStockOrders1
		//	public bool AllowOutOfStockOrders1 { get; set; }

		//	[SmartResourceDisplayName("Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.Fields.Sku")]
		//	//we don't name it StockQuantity because Telerik has a small bug 
		//	//"if we have one more editor with the same name on a page, it doesn't allow editing"
		//	//in our case it's productVariant.Sku1
		//	public string Sku1 { get; set; }

		//	[SmartResourceDisplayName("Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.Fields.ManufacturerPartNumber")]
		//	//we don't name it StockQuantity because Telerik has a small bug 
		//	//"if we have one more editor with the same name on a page, it doesn't allow editing"
		//	//in our case it's productVariant.ManufacturerPartNumber1
		//	public string ManufacturerPartNumber1 { get; set; }

		//	[SmartResourceDisplayName("Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.Fields.Gtin")]
		//	//we don't name it StockQuantity because Telerik has a small bug 
		//	//"if we have one more editor with the same name on a page, it doesn't allow editing"
		//	//in our case it's productVariant.Gtin1
		//	public string Gtin1 { get; set; }
		//}

        #endregion
    }

    public class ProductLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.ShortDescription")]
        [AllowHtml]
        public string ShortDescription { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.FullDescription")]
        [AllowHtml]
        public string FullDescription { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.MetaKeywords")]
        [AllowHtml]
        public string MetaKeywords { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.MetaDescription")]
        [AllowHtml]
        public string MetaDescription { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.MetaTitle")]
        [AllowHtml]
        public string MetaTitle { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.SeName")]
        [AllowHtml]
        public string SeName { get; set; }
    }
}