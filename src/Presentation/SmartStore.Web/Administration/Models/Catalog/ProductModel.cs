using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.ComponentModel;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Localization;
using SmartStore.Services.Seo;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Catalog
{
    [Validator(typeof(ProductValidator))]
    public class ProductModel : TabbableModel, ILocalizedModel<ProductLocalizedModel>
    {
        public ProductModel()
        {
            Locales = new List<ProductLocalizedModel>();
            CopyProductModel = new CopyProductModel();
            AvailableProductTemplates = new List<SelectListItem>();
            AvailableTaxCategories = new List<SelectListItem>();
            AvailableMeasureUnits = new List<SelectListItem>();
            AvailableQuantityUnits = new List<SelectListItem>();
            AvailableMeasureWeights = new List<SelectListItem>();
            AddPictureModel = new ProductPictureModel();
            AddSpecificationAttributeModel = new AddProductSpecificationAttributeModel();
            AvailableManageInventoryMethods = new List<SelectListItem>();
            AvailableCountries = new List<SelectListItem>();
            DownloadVersions = new List<DownloadVersion>();
        }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.ID")]
        public override int Id { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.PictureThumbnailUrl")]
        public string PictureThumbnailUrl { get; set; }
        public bool NoThumb { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.ProductType")]
        public int ProductTypeId { get; set; }
        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.ProductType")]
        public string ProductTypeName { get; set; }
        public string ProductTypeLabelHint { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.ProductUrl")]
        public string ProductUrl { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.AssociatedToProductName")]
        public int AssociatedToProductId { get; set; }
        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.AssociatedToProductName")]
        public string AssociatedToProductName { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.Visibility")]
        public ProductVisibility Visibility { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.Condition")]
        public ProductCondition Condition { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.ProductTemplate")]
        [AllowHtml]
        public int ProductTemplateId { get; set; }
        public IList<SelectListItem> AvailableProductTemplates { get; set; }

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

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.ShowOnHomePage")]
        public bool ShowOnHomePage { get; set; }

        [SmartResourceDisplayName("Common.DisplayOrder")]
        public int HomePageDisplayOrder { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Seo.MetaKeywords")]
        [AllowHtml]
        public string MetaKeywords { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Seo.MetaDescription")]
        [AllowHtml]
        public string MetaDescription { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Seo.MetaTitle")]
        [AllowHtml]
        public string MetaTitle { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Seo.SeName")]
        [AllowHtml]
        public string SeName { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.AllowCustomerReviews")]
        public bool AllowCustomerReviews { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.ProductTags")]
        public string[] ProductTags { get; set; }
        public MultiSelectList AvailableProductTags { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.Sku")]
        [AllowHtml]
        public string Sku { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.ManufacturerPartNumber")]
        [AllowHtml]
        public string ManufacturerPartNumber { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.GTIN")]
        [AllowHtml]
        public string Gtin { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.CustomsTariffNumber")]
        public string CustomsTariffNumber { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.CountryOfOriginId")]
        public int? CountryOfOriginId { get; set; }
        public IList<SelectListItem> AvailableCountries { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.IsGiftCard")]
        public bool IsGiftCard { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.GiftCardType")]
        public int GiftCardTypeId { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.RequireOtherProducts")]
        public bool RequireOtherProducts { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.RequiredProductIds")]
        public string RequiredProductIds { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.AutomaticallyAddRequiredProducts")]
        public bool AutomaticallyAddRequiredProducts { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.IsDownload")]
        public bool IsDownload { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.NewVersionDownloadId")]
        [UIHint("Download")]
        public int? NewVersionDownloadId { get; set; }

        [SmartResourceDisplayName("Common.Download.Version")]
        public string NewVersion { get; set; }

        public List<DownloadVersion> DownloadVersions { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.Download")]
        [UIHint("Download")]
        public int? DownloadId { get; set; }
        public string DownloadThumbUrl { get; set; }
        public Download CurrentDownload { get; set; }

        [SmartResourceDisplayName("Common.Download.Version")]
        public string DownloadFileVersion { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.UnlimitedDownloads")]
        public bool UnlimitedDownloads { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.MaxNumberOfDownloads")]
        public int MaxNumberOfDownloads { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.DownloadExpirationDays")]
        public int? DownloadExpirationDays { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.DownloadActivationType")]
        public int DownloadActivationTypeId { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.HasSampleDownload")]
        public bool HasSampleDownload { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.SampleDownload")]
        [UIHint("Download")]
        public int? SampleDownloadId { get; set; }

        public int? OldSampleDownloadId { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.HasUserAgreement")]
        public bool HasUserAgreement { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.UserAgreementText")]
        [AllowHtml]
        public string UserAgreementText { get; set; }

        [AllowHtml]
        public string AddChangelog { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.IsRecurring")]
        public bool IsRecurring { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.RecurringCycleLength")]
        public int RecurringCycleLength { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.RecurringCyclePeriod")]
        public int RecurringCyclePeriodId { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.RecurringTotalCycles")]
        public int RecurringTotalCycles { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.IsShipEnabled")]
        public bool IsShipEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.IsFreeShipping")]
        public bool IsFreeShipping { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.AdditionalShippingCharge")]
        public decimal AdditionalShippingCharge { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.IsEsd")]
        public bool IsEsd { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.IsTaxExempt")]
        public bool IsTaxExempt { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.TaxCategory")]
        public int? TaxCategoryId { get; set; }
        public IList<SelectListItem> AvailableTaxCategories { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.ManageInventoryMethod")]
        public int ManageInventoryMethodId { get; set; }
        public IList<SelectListItem> AvailableManageInventoryMethods { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.StockQuantity")]
        public int StockQuantity { get; set; }
        public int OriginalStockQuantity { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.DisplayStockAvailability")]
        public bool DisplayStockAvailability { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.DisplayStockQuantity")]
        public bool DisplayStockQuantity { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.MinStockQuantity")]
        public int MinStockQuantity { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.LowStockActivity")]
        public int LowStockActivityId { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.NotifyAdminForQuantityBelow")]
        public int NotifyAdminForQuantityBelow { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.BackorderMode")]
        public int BackorderModeId { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.AllowBackInStockSubscriptions")]
        public bool AllowBackInStockSubscriptions { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.OrderMinimumQuantity")]
        public int OrderMinimumQuantity { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.OrderMaximumQuantity")]
        public int OrderMaximumQuantity { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.QuantityStep")]
        public int QuantityStep { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.QuantiyControlType")]
        public QuantityControlType QuantiyControlType { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.HideQuantityControl")]
        public bool HideQuantityControl { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.AllowedQuantities")]
        public string AllowedQuantities { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.DisableBuyButton")]
        public bool DisableBuyButton { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.DisableWishlistButton")]
        public bool DisableWishlistButton { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.AvailableForPreOrder")]
        public bool AvailableForPreOrder { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.CallForPrice")]
        public bool CallForPrice { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.Price")]
        public decimal Price { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.OldPrice")]
        public decimal OldPrice { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.ProductCost")]
        public decimal ProductCost { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.SpecialPrice")]
        public decimal? SpecialPrice { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.SpecialPriceStartDateTimeUtc")]
        public DateTime? SpecialPriceStartDateTimeUtc { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.SpecialPriceEndDateTimeUtc")]
        public DateTime? SpecialPriceEndDateTimeUtc { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.CustomerEntersPrice")]
        public bool CustomerEntersPrice { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.MinimumCustomerEnteredPrice")]
        public decimal MinimumCustomerEnteredPrice { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.MaximumCustomerEnteredPrice")]
        public decimal MaximumCustomerEnteredPrice { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.Weight")]
        public decimal Weight { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.Length")]
        public decimal Length { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.Width")]
        public decimal Width { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.Height")]
        public decimal Height { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.AvailableStartDateTime")]
        public DateTime? AvailableStartDateTimeUtc { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.AvailableEndDateTime")]
        public DateTime? AvailableEndDateTimeUtc { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.Published")]
        public bool Published { get; set; }

        [SmartResourceDisplayName("Common.CreatedOn")]
        public DateTime? CreatedOn { get; set; }

        [SmartResourceDisplayName("Common.UpdatedOn")]
        public DateTime? UpdatedOn { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.BundleTitleText")]
        public string BundleTitleText { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.BundlePerItemPricing")]
        public bool BundlePerItemPricing { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.BundlePerItemShipping")]
        public bool BundlePerItemShipping { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.BundlePerItemShoppingCart")]
        public bool BundlePerItemShoppingCart { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.AttributeChoiceBehaviour")]
        public AttributeChoiceBehaviour AttributeChoiceBehaviour { get; set; }

        public string PrimaryStoreCurrencyCode { get; set; }
        public string BaseDimensionIn { get; set; }
        public string BaseWeightIn { get; set; }

        public IList<ProductLocalizedModel> Locales { get; set; }

        // ACL (customer roles).
        [UIHint("CustomerRoles")]
        [AdditionalMetadata("multiple", true)]
        [SmartResourceDisplayName("Admin.Common.CustomerRole.LimitedTo")]
        public int[] SelectedCustomerRoleIds { get; set; }

        [SmartResourceDisplayName("Admin.Common.CustomerRole.LimitedTo")]
        public bool SubjectToAcl { get; set; }

        // Store mapping.
        [UIHint("Stores")]
        [AdditionalMetadata("multiple", true)]
        [SmartResourceDisplayName("Admin.Common.Store.LimitedTo")]
        public int[] SelectedStoreIds { get; set; }

        [SmartResourceDisplayName("Admin.Common.Store.LimitedTo")]
        public bool LimitedToStores { get; set; }

        public int NumberOfAvailableCategories { get; set; }
        public int NumberOfAvailableManufacturers { get; set; }
        public int NumberOfAvailableProductAttributes { get; set; }

        //Pictures.
        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.HasPreviewPicture")]
        public bool HasPreviewPicture { get; set; }
        public ProductPictureModel AddPictureModel { get; set; }

        public IList<ProductMediaFile> ProductMediaFiles { get; set; }

        [UIHint("Discounts")]
        [AdditionalMetadata("multiple", true)]
        [AdditionalMetadata("discountType", DiscountType.AssignedToSkus)]
        [SmartResourceDisplayName("Admin.Promotions.Discounts.AppliedDiscounts")]
        public int[] SelectedDiscountIds { get; set; }

        public AddProductSpecificationAttributeModel AddSpecificationAttributeModel { get; set; }

        public CopyProductModel CopyProductModel { get; set; }

        //BasePrice
        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.BasePriceEnabled")]
        public bool BasePriceEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.BasePriceMeasureUnit")]
        public string BasePriceMeasureUnit { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.BasePriceAmount")]
        public decimal? BasePriceAmount { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.BasePriceBaseAmount")]
        public int? BasePriceBaseAmount { get; set; }

        public IList<SelectListItem> AvailableMeasureWeights { get; set; }
        public IList<SelectListItem> AvailableMeasureUnits { get; set; }

        [UIHint("DeliveryTimes")]
        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.DeliveryTime")]
        public int? DeliveryTimeId { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.QuantityUnit")]
        public int? QuantityUnitId { get; set; }
        public IList<SelectListItem> AvailableQuantityUnits { get; set; }

        public string ProductSelectCheckboxClass { get; set; }

        public bool IsSystemProduct { get; set; }
        public string SystemName { get; set; }

        #region Nested classes

        public class AddProductSpecificationAttributeModel : ModelBase
        {
            public AddProductSpecificationAttributeModel()
            {
                AvailableAttributes = new List<SelectListItem>();
                AvailableOptions = new List<SelectListItem>();
            }

            [SmartResourceDisplayName("Admin.Catalog.Products.SpecificationAttributes.Fields.SpecificationAttribute")]
            public int SpecificationAttributeId { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.SpecificationAttributes.Fields.SpecificationAttributeOption")]
            public int SpecificationAttributeOptionId { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.SpecificationAttributes.Fields.AllowFiltering")]
            public bool? AllowFiltering { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.SpecificationAttributes.Fields.ShowOnProductPage")]
            public bool? ShowOnProductPage { get; set; }

            [SmartResourceDisplayName("Common.DisplayOrder")]
            public int DisplayOrder { get; set; }

            public IList<SelectListItem> AvailableAttributes { get; set; }
            public IList<SelectListItem> AvailableOptions { get; set; }
        }

        public class ProductPictureModel : EntityModelBase
        {
            public int ProductId { get; set; }

            [UIHint("Media"), AdditionalMetadata("album", "catalog"), AdditionalMetadata("typeFilter", "image,video")]
            [SmartResourceDisplayName("Admin.Catalog.Products.Pictures.Fields.Picture")]
            public int PictureId { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.Pictures.Fields.Picture")]
            public string PictureUrl { get; set; }

            [SmartResourceDisplayName("Common.DisplayOrder")]
            public int DisplayOrder { get; set; }

            public ProductMediaFile ProductMediaFile { get; set; }
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

            [SmartResourceDisplayName("Common.DisplayOrder")]
            public int DisplayOrder { get; set; }

            [SmartResourceDisplayName("Admin.Rules.AddedByRule")]
            public bool IsSystemMapping { get; set; }
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

            [SmartResourceDisplayName("Common.DisplayOrder")]
            public int DisplayOrder { get; set; }
        }

        public class RelatedProductModel : EntityModelBase
        {
            public int ProductId2 { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.RelatedProducts.Fields.Product")]
            public string Product2Name { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.Fields.ProductType")]
            public string ProductTypeName { get; set; }
            public string ProductTypeLabelHint { get; set; }

            [SmartResourceDisplayName("Common.DisplayOrder")]
            public int DisplayOrder { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.Fields.Sku")]
            public string Product2Sku { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.Fields.Published")]
            public bool Product2Published { get; set; }
        }

        public partial class AssociatedProductModel : EntityModelBase
        {
            [SmartResourceDisplayName("Admin.Catalog.Products.AssociatedProducts.Fields.Product")]
            public string ProductName { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.Fields.ProductType")]
            public string ProductTypeName { get; set; }
            public string ProductTypeLabelHint { get; set; }

            [SmartResourceDisplayName("Common.DisplayOrder")]
            public int DisplayOrder { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.Fields.Sku")]
            public string Sku { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.Fields.Published")]
            public bool Published { get; set; }
        }

        public partial class BundleItemModel : EntityModelBase
        {
            public int ProductId { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.BundleItems.Fields.Product")]
            public string ProductName { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.Fields.ProductType")]
            public string ProductTypeName { get; set; }
            public string ProductTypeLabelHint { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.Fields.Sku")]
            public string Sku { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.BundleItems.Fields.Quantity")]
            public int Quantity { get; set; }

            [SmartResourceDisplayName("Common.DisplayOrder")]
            public int DisplayOrder { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.BundleItems.Fields.Discount")]
            public decimal? Discount { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.BundleItems.Fields.Visible")]
            public bool Visible { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.BundleItems.Fields.Published")]
            public bool Published { get; set; }
        }

        public class CrossSellProductModel : EntityModelBase
        {
            public int ProductId2 { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.CrossSells.Fields.Product")]
            public string Product2Name { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.Fields.ProductType")]
            public string ProductTypeName { get; set; }
            public string ProductTypeLabelHint { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.Fields.Sku")]
            public string Product2Sku { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.Fields.Published")]
            public bool Product2Published { get; set; }
        }

        public class TierPriceModel : EntityModelBase
        {
            public int ProductId { get; set; }

            public int CustomerRoleId { get; set; }
            [SmartResourceDisplayName("Admin.Catalog.Products.TierPrices.Fields.CustomerRole")]
            [UIHint("TierPriceCustomer")]
            public string CustomerRole { get; set; }


            public int StoreId { get; set; }
            [SmartResourceDisplayName("Admin.Catalog.Products.TierPrices.Fields.Store")]
            [UIHint("TierPriceStore")]
            public string Store { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.TierPrices.Fields.Quantity")]
            public int Quantity { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.TierPrices.Fields.Price")]
            //we don't name it Price because Telerik has a small bug 
            //"if we have one more editor with the same name on a page, it doesn't allow editing"
            //in our case it's product.Price1
            public decimal Price1 { get; set; }

            public int CalculationMethodId { get; set; }
            [SmartResourceDisplayName("Admin.Catalog.Products.TierPrices.Fields.CalculationMethod")]
            [UIHint("TierPriceCalculationMethod")]
            public string CalculationMethod { get; set; }
        }

        public class ProductVariantAttributeModel : EntityModelBase
        {
            public int ProductId { get; set; }
            public int ProductAttributeId { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.ProductVariantAttributes.Attributes.Fields.Attribute")]
            [UIHint("ProductAttribute")]
            public string ProductAttribute { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.ProductVariantAttributes.Attributes.Fields.TextPrompt")]
            [AllowHtml]
            public string TextPrompt { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.ProductVariantAttributes.Attributes.Fields.CustomData")]
            [AllowHtml]
            public string CustomData { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.ProductVariantAttributes.Attributes.Fields.IsRequired")]
            public bool IsRequired { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Attributes.AttributeControlType")]
            [UIHint("AttributeControlType")]
            public string AttributeControlType { get; set; }
            public int AttributeControlTypeId { get; set; }

            // We don't name it DisplayOrder because Telerik has a small bug 
            // "if we have one more editor with the same name on a page, it doesn't allow editing"
            // in our case it's category.DisplayOrder.
            [SmartResourceDisplayName("Common.DisplayOrder")]
            public int DisplayOrder1 { get; set; }

            public string ViewEditUrl { get; set; }
            public string ViewEditText { get; set; }
            public string OptionsSets { get; set; }
            public int ValueCount { get; set; }
        }

        public class ProductVariantAttributeValueListModel : ModelBase
        {
            public int ProductId { get; set; }

            public string ProductName { get; set; }

            public int ProductVariantAttributeId { get; set; }

            public string ProductVariantAttributeName { get; set; }
        }

        // TODO: DRY. see ProductAttributeOptionModelBase
        [Validator(typeof(ProductVariantAttributeValueModelValidator))]
        public class ProductVariantAttributeValueModel : EntityModelBase, ILocalizedModel<ProductVariantAttributeValueLocalizedModel>
        {
            public ProductVariantAttributeValueModel()
            {
                Locales = new List<ProductVariantAttributeValueLocalizedModel>();
            }

            public int ProductId { get; set; }
            public int ProductVariantAttributeId { get; set; }

            [AllowHtml, SmartResourceDisplayName("Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.Alias")]
            public string Alias { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.Name")]
            [AllowHtml]
            public string Name { get; set; }
            public string NameString { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.ColorSquaresRgb")]
            [AllowHtml, UIHint("Color")]
            public string Color { get; set; }
            public bool IsListTypeAttribute { get; set; }

            [UIHint("Media"), AdditionalMetadata("album", "catalog")]
            [SmartResourceDisplayName("Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.Picture")]
            public int PictureId { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.PriceAdjustment")]
            public decimal PriceAdjustment { get; set; }
            [SmartResourceDisplayName("Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.PriceAdjustment")]
            public string PriceAdjustmentString { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.WeightAdjustment")]
            public decimal WeightAdjustment { get; set; }
            [SmartResourceDisplayName("Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.WeightAdjustment")]
            public string WeightAdjustmentString { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.IsPreSelected")]
            public bool IsPreSelected { get; set; }

            [SmartResourceDisplayName("Common.DisplayOrder")]
            public int DisplayOrder { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.ValueTypeId")]
            public int ValueTypeId { get; set; }
            [SmartResourceDisplayName("Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.ValueTypeId")]
            public string TypeName { get; set; }
            public string TypeNameClass { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.LinkedProduct")]
            public int LinkedProductId { get; set; }
            [SmartResourceDisplayName("Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.LinkedProduct")]
            public string LinkedProductName { get; set; }
            public string LinkedProductTypeName { get; set; }
            public string LinkedProductTypeLabelHint { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.Quantity")]
            public int Quantity { get; set; }
            public string QuantityInfo { get; set; }

            public IList<ProductVariantAttributeValueLocalizedModel> Locales { get; set; }
        }

        public class ProductVariantAttributeValueLocalizedModel : ILocalizedModelLocal
        {
            public int LanguageId { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.Alias")]
            public string Alias { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.Fields.Name")]
            [AllowHtml]
            public string Name { get; set; }
        }

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

        [SmartResourceDisplayName("Admin.Configuration.Seo.MetaKeywords")]
        [AllowHtml]
        public string MetaKeywords { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Seo.MetaDescription")]
        [AllowHtml]
        public string MetaDescription { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Seo.MetaTitle")]
        [AllowHtml]
        public string MetaTitle { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Seo.SeName")]
        [AllowHtml]
        public string SeName { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Fields.BundleTitleText")]
        public string BundleTitleText { get; set; }
    }

    public class DownloadVersion
    {
        public int? DownloadId { get; set; }

        public string FileName { get; set; }

        public string DownloadUrl { get; set; }

        public string FileVersion { get; set; }
    }

    public partial class ProductValidator : AbstractValidator<ProductModel>
    {
        public ProductValidator(Localizer T)
        {
            RuleFor(x => x.Name).NotEmpty();

            When(x => x.LoadedTabs != null && x.LoadedTabs.Contains("Inventory", StringComparer.OrdinalIgnoreCase), () =>
            {
                RuleFor(x => x.OrderMinimumQuantity).GreaterThan(0); // dont't remove "Admin.Validation.ValueGreaterZero" resource. It is used elsewhere.
                RuleFor(x => x.OrderMaximumQuantity).GreaterThan(0);
            });

            // validate PAnGV
            When(x => x.BasePriceEnabled && x.LoadedTabs != null && x.LoadedTabs.Contains("Price"), () =>
            {
                RuleFor(x => x.BasePriceMeasureUnit).NotEmpty().WithMessage(T("Admin.Catalog.Products.Fields.BasePriceMeasureUnit.Required"));
                RuleFor(x => x.BasePriceBaseAmount)
                    .NotEmpty().WithMessage(T("Admin.Catalog.Products.Fields.BasePriceBaseAmount.Required"))
                    .GreaterThan(0).WithMessage(T("Admin.Catalog.Products.Fields.BasePriceBaseAmount.Required"));
                RuleFor(x => x.BasePriceAmount)
                    .NotEmpty().WithMessage(T("Admin.Catalog.Products.Fields.BasePriceAmount.Required"))
                    .GreaterThan(0).WithMessage(T("Admin.Catalog.Products.Fields.BasePriceAmount.Required"));
            });

            RuleFor(x => x.TaxCategoryId)
                .NotNull()  // Nullable required for IsTaxExempt.
                .NotEqual(0)
                .When(x => !x.IsTaxExempt);

            RuleFor(x => x.DownloadFileVersion)
                .NotEmpty()
                .When(x => x.DownloadId != null && x.DownloadId != 0)
                .WithMessage(T("Admin.Catalog.Products.Download.SemanticVersion.NotValid"));

            RuleFor(x => x.NewVersion)
                .NotEmpty()
                .When(x => x.NewVersionDownloadId != null && x.NewVersionDownloadId != 0)
                .WithMessage(T("Admin.Catalog.Products.Download.SemanticVersion.NotValid"));
        }
    }

    public partial class ProductVariantAttributeValueModelValidator : AbstractValidator<ProductModel.ProductVariantAttributeValueModel>
    {
        public ProductVariantAttributeValueModelValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Quantity).GreaterThanOrEqualTo(1).When(x => x.ValueTypeId == (int)ProductVariantAttributeValueType.ProductLinkage);
        }
    }

    public class ProductMapper :
        IMapper<Product, ProductModel>
    {
        public void Map(Product from, ProductModel to)
        {
            MiniMapper.Map(from, to);
            to.SeName = from.GetSeName(0, true, false);
        }
    }
}