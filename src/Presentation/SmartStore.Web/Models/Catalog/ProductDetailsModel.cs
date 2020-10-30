using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Directory;
using SmartStore.Services.Catalog.Modelling;
using SmartStore.Services.Localization;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.UI.Choices;
using SmartStore.Web.Models.Media;

namespace SmartStore.Web.Models.Catalog
{
    public partial class ProductDetailsModel : EntityModelBase
    {
        public ProductDetailsModel()
        {
            Manufacturers = new List<ManufacturerOverviewModel>();
            GiftCard = new GiftCardModel();
            ProductPrice = new ProductPriceModel();
            AddToCart = new AddToCartModel();
            ProductVariantAttributes = new List<ProductVariantAttributeModel>();
            AssociatedProducts = new List<ProductDetailsModel>();
            BundledItems = new List<ProductDetailsModel>();
            BundleItem = new ProductBundleItemModel();
            ActionItems = new Dictionary<string, ActionItemModel>();
            IsAvailable = true;
        }

        public MediaGalleryModel MediaGalleryModel { get; set; }

        public LocalizedValue<string> Name { get; set; }
        public LocalizedValue<string> ShortDescription { get; set; }
        public LocalizedValue<string> FullDescription { get; set; }
        public string ProductTemplateViewPath { get; set; }
        public LocalizedValue<string> MetaKeywords { get; set; }
        public LocalizedValue<string> MetaDescription { get; set; }
        public LocalizedValue<string> MetaTitle { get; set; }
        public string SeName { get; set; }
        public ProductType ProductType { get; set; }
        public bool VisibleIndividually { get; set; }

        public int PictureSize { get; set; }
        public bool CanonicalUrlsEnabled { get; set; }

        public ProductCondition Condition { get; set; }
        public bool ShowCondition { get; set; }
        public string LocalizedCondition { get; set; }

        public bool ShowSku { get; set; }
        public string Sku { get; set; }

        public bool ShowManufacturerPartNumber { get; set; }
        public string ManufacturerPartNumber { get; set; }

        public bool ShowGtin { get; set; }
        public string Gtin { get; set; }

        public bool HasSampleDownload { get; set; }

        public GiftCardModel GiftCard { get; set; }
        public string GiftCardFieldPrefix => GiftCardQueryItem.CreateKey(Id, BundleItem.Id, null);

        public string StockAvailability { get; set; }
        public bool IsAvailable { get; set; }

        public bool IsCurrentCustomerRegistered { get; set; }
        public bool DisplayBackInStockSubscription { get; set; }
        public bool BackInStockAlreadySubscribed { get; set; }

        public ProductPriceModel ProductPrice { get; set; }
        public AddToCartModel AddToCart { get; set; }
        public IList<ProductVariantAttributeModel> ProductVariantAttributes { get; set; }
        public string AttributeInfo { get; set; }

        public bool DisplayAdminLink { get; set; }
        public bool ShowLegalInfo { get; set; }
        public string LegalInfo { get; set; }
        public bool ShowWeight { get; set; }
        public bool ShowDimensions { get; set; }
        public decimal WeightValue { get; set; }
        public string Weight { get; set; }
        public string Length { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
        public int ThumbDimensions { get; set; }
        public LocalizedValue<string> QuantityUnitName { get; set; }
        public bool DisplayProductReviews { get; set; }
        public bool IsBasePriceEnabled { get; set; }
        public string BasePriceInfo { get; set; }
        public string BundleTitleText { get; set; }
        public bool BundlePerItemShipping { get; set; }
        public bool BundlePerItemPricing { get; set; }
        public bool BundlePerItemShoppingCart { get; set; }
        public bool DisplayTextForZeroPrices { get; set; }
        public PriceDisplayStyle PriceDisplayStyle { get; set; }

        public bool IsShipEnabled { get; set; }
        public DeliveryTimesPresentation DeliveryTimesPresentation { get; set; }
        public bool DisplayDeliveryTimeAccordingToStock { get; set; }
        public string DeliveryTimeName { get; set; }
        public string DeliveryTimeHexValue { get; set; }
        public string DeliveryTimeDate { get; set; }

        public ProductVariantAttributeCombination SelectedCombination { get; set; }

        public IList<ManufacturerOverviewModel> Manufacturers { get; set; }
        public int ReviewCount { get; set; }

        // A list of associated products. For example, "Grouped" products could have several child "simple" products
        public IList<ProductDetailsModel> AssociatedProducts { get; set; }
        public bool IsAssociatedProduct { get; set; }

        public IList<ProductDetailsModel> BundledItems { get; set; }
        public ProductBundleItemModel BundleItem { get; set; }
        public bool IsBundlePart { get; set; }

		public bool CompareEnabled { get; set; }
		public bool TellAFriendEnabled { get; set; }
		public bool AskQuestionEnabled { get; set; }
		public string HotlineTelephoneNumber { get; set; }
		public string ProductShareCode { get; set; }

        public IDictionary<string, ActionItemModel> ActionItems { get; set; }

        #region Nested Classes

        public partial class ActionItemModel : ModelBase
        {
            public string Key { get; set; }
            public string Title { get; set; }
            public string Tooltip { get; set; }
            public string Href { get; set; }
            public string CssClass { get; set; }
            public string IconCssClass { get; set; }
            public bool IsPrimary { get; set; }
            public string PrimaryActionColor { get; set; }
            public int Priority { get; set; }
        }

        public partial class AddToCartModel : ModelBase, IQuantityInput
        {
            public AddToCartModel()
            {
                AllowedQuantities = new List<SelectListItem>();
            }
            public int ProductId { get; set; }

            [SmartResourceDisplayName("Products.Qty")]
            public int EnteredQuantity { get; set; }

            [SmartResourceDisplayName("Products.EnterProductPrice")]
            public bool CustomerEntersPrice { get; set; }
            [SmartResourceDisplayName("Products.EnterProductPrice")]
            public decimal CustomerEnteredPrice { get; set; }
            public string CustomerEnteredPriceRange { get; set; }

            public int MinOrderAmount { get; set; }
            public int MaxOrderAmount { get; set; }
            public LocalizedValue<string> QuantityUnitName { get; set; }
            public int QuantityStep { get; set; }
            public bool HideQuantityControl { get; set; }
            public QuantityControlType QuantiyControlType { get; set; }

            public bool DisableBuyButton { get; set; }
            public bool DisableWishlistButton { get; set; }
            public List<SelectListItem> AllowedQuantities { get; set; }
            public bool AvailableForPreOrder { get; set; }
        }

        public partial class ProductPriceModel : ModelBase
        {
            public string OldPrice { get; set; }
            public decimal OldPriceValue { get; set; }

            public string Price { get; set; }
            public string PriceWithDiscount { get; set; }

            public decimal PriceValue { get; set; }
            public decimal PriceWithDiscountValue { get; set; }

            public float SavingPercent { get; set; }
            public string SavingAmount { get; set; }

            public bool CustomerEntersPrice { get; set; }
            public bool CallForPrice { get; set; }

            public int ProductId { get; set; }

            public bool HidePrices { get; set; }
            public bool ShowLoginNote { get; set; }

            public bool DynamicPriceUpdate { get; set; }
            public bool BundleItemShowBasePrice { get; set; }

            public string NoteWithDiscount { get; set; }
            public string NoteWithoutDiscount { get; set; }
                        
            public string PriceValidUntilUtc { get; set; }
        }

        public partial class GiftCardModel : ModelBase
        {
            public bool IsGiftCard { get; set; }

            [SmartResourceDisplayName("Products.GiftCard.RecipientName")]
            public string RecipientName { get; set; }

            [SmartResourceDisplayName("Products.GiftCard.RecipientEmail")]
            public string RecipientEmail { get; set; }

            [SmartResourceDisplayName("Products.GiftCard.SenderName")]
            public string SenderName { get; set; }

            [SmartResourceDisplayName("Products.GiftCard.SenderEmail")]
            public string SenderEmail { get; set; }

            [SmartResourceDisplayName("Products.GiftCard.Message")]
            [SanitizeHtml]
            public string Message { get; set; }

            public GiftCardType GiftCardType { get; set; }
        }

        public partial class TierPriceModel : ModelBase
        {
            public string Price { get; set; }
            public int Quantity { get; set; }
        }

        public partial class ProductVariantAttributeModel : ChoiceModel
        {
            public int ProductId { get; set; }
            public int BundleItemId { get; set; }
            public int ProductAttributeId { get; set; }
            public ProductVariantAttribute ProductAttribute { get; set; }

            public override string BuildControlId()
            {
                return ProductVariantQueryItem.CreateKey(ProductId, BundleItemId, ProductAttributeId, Id);
            }

            public override string GetFileUploadUrl(UrlHelper url)
            {
                return url.Action("UploadFileProductAttribute", "ShoppingCart", new { productId = this.ProductId, productAttributeId = this.ProductAttributeId });
            }
        }

        public partial class ProductVariantAttributeValueModel : ChoiceItemModel
        {
            public ProductVariantAttributeValue ProductAttributeValue { get; set; }

            public override string GetItemLabel()
            {
                var label = Name;

                if (QuantityInfo > 1)
                {
                    label = "{0} x {1}".FormatCurrentUI(QuantityInfo, label);
                }

                if (PriceAdjustment.HasValue())
                {
                    label += " ({0})".FormatWith(PriceAdjustment);
                }

                return label;
            }
        }

        public partial class ProductBundleItemModel : EntityModelBase
        {
            public int Quantity { get; set; }
            public bool HideThumbnail { get; set; }
            public bool Visible { get; set; }
            public bool IsBundleItemPricing { get; set; }
        }

        #endregion
    }
}