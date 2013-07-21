using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation.Attributes;
using SmartStore.Admin.Validators.Catalog;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Mvc;
using SmartStore.Admin.Models.Directory;

namespace SmartStore.Admin.Models.Catalog
{
    [Validator(typeof(ProductVariantValidator))]
    public class ProductVariantModel : EntityModelBase, ILocalizedModel<ProductVariantLocalizedModel>
    {
        public ProductVariantModel()
        {
            Locales = new List<ProductVariantLocalizedModel>();
            AvailableTaxCategories = new List<SelectListItem>();
            AvailableDeliveryTimes = new List<SelectListItem>();
            AvailableMeasureUnits = new List<SelectListItem>();
        }

        #region Standard properties

        [SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.ID")]
        public override int Id { get; set; }

        public int ProductId { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.Sku")]
        [AllowHtml]
        public string Sku { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.Description")]
        [AllowHtml]
        public string Description { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.AdminComment")]
        [AllowHtml]
        public string AdminComment { get; set; }

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
        public string RequiredProductVariantIds { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.AutomaticallyAddRequiredProductVariants")]
        public bool AutomaticallyAddRequiredProductVariants { get; set; }

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
        public int? TaxCategoryId { get; set; }
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
        public decimal? SpecialPrice { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.SpecialPriceStartDateTimeUtc")]
        public DateTime? SpecialPriceStartDateTimeUtc { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.SpecialPriceEndDateTimeUtc")]
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

        //codehint: sm-add
        [SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.DeliveryTime")]
        //[UIHint("DeliveryTime")]
        public int? DeliveryTimeId { get; set; }
        public IList<SelectListItem> AvailableDeliveryTimes { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.Picture")]
        [UIHint("Picture")]
        public int PictureId { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.AvailableStartDateTime")]
        public DateTime? AvailableStartDateTimeUtc { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.AvailableEndDateTime")]
        public DateTime? AvailableEndDateTimeUtc { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.Published")]
        public bool Published { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        #region BasePrice (PAnGV)
        // codehint: sm-add (BasePrice)

        [SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.BasePriceEnabled")]
        public bool BasePriceEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.BasePriceMeasureUnit")]
        public string BasePriceMeasureUnit { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.BasePriceAmount")]
        public decimal? BasePriceAmount { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.BasePriceBaseAmount")]
        public int? BasePriceBaseAmount { get; set; }

        //public string BasePriceInfo { get; set; }

        public IList<SelectListItem> AvailableMeasureUnits { get; set; }

        #endregion

        #endregion

        #region Model specific

        public string PrimaryStoreCurrencyCode { get; set; }
        public string BaseDimensionIn { get; set; }
        public string BaseWeightIn { get; set; }
        [SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.ProductName")]
        public string ProductName { get; set; }

        //product attributes
        public int NumberOfAvailableProductAttributes { get; set; }


        //locales
        public IList<ProductVariantLocalizedModel> Locales { get; set; }

        //discounts
        public List<Discount> AvailableDiscounts { get; set; }
        public int[] SelectedDiscountIds { get; set; }


        public bool HideNameAndDescriptionProperties { get; set; }
        public bool HidePublishedProperty { get; set; }
        public bool HideDisplayOrderProperty { get; set; }

        #endregion

        #region Nested classes

        public class TierPriceModel : EntityModelBase
        {
            public int ProductVariantId { get; set; }

            public int CustomerRoleId { get; set; }
            [SmartResourceDisplayName("Admin.Catalog.Products.Variants.TierPrices.Fields.CustomerRole")]
            [UIHint("TierPriceCustomer")]
            public string CustomerRole { get; set; }

			public int StoreId { get; set; }
			[SmartResourceDisplayName("Admin.Common.Store")]
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
            public int ProductVariantId { get; set; }

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
            public int ProductVariantId { get; set; }

            public string ProductVariantName { get; set; }

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

        #endregion

    }

    public class ProductVariantLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Products.Variants.Fields.Description")]
        [AllowHtml]
        public string Description { get; set; }
    }
}