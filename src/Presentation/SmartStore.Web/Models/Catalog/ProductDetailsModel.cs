using System;
using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Directory;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;
using SmartStore.Web.Models.Media;

namespace SmartStore.Web.Models.Catalog
{
    public partial class ProductDetailsModel : EntityModelBase
    {
		private ProductDetailsPictureModel _detailsPictureModel;	// codehint: sm-add

        public ProductDetailsModel()
        {
            ProductVariantModels = new List<ProductVariantModel>();
            SpecificationAttributeModels = new List<ProductSpecificationModel>();
            //codehint: sm-edit
            //Manufacturers = new List<ProductManufacturer>();
            Manufacturers = new List<ManufacturerOverviewModel>();
        }

        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }
        public string ProductTemplateViewPath { get; set; }
        public string MetaKeywords { get; set; }
        public string MetaDescription { get; set; }
        public string MetaTitle { get; set; }
        public string SeName { get; set; }

        //codehint: sm-ad
        public bool DisplayAdminLink { get; set; }

        //picture(s)
		//codehint: sm-edit (refactored)
		public ProductDetailsPictureModel DetailsPictureModel {
			get {
				if (_detailsPictureModel == null)
					_detailsPictureModel = new ProductDetailsPictureModel();
				return _detailsPictureModel;
			}
		}
        //codehint: sm-add
        public IList<ManufacturerOverviewModel> Manufacturers { get; set; }
        public int ReviewCount { get; set; }

        //product variant(s)
        public IList<ProductVariantModel> ProductVariantModels { get; set; }
        //specification attributes
        public IList<ProductSpecificationModel> SpecificationAttributeModels { get; set; }

		#region Nested Classes

        public partial class ProductBreadcrumbModel : ModelBase
        {
            public ProductBreadcrumbModel()
            {
                CategoryBreadcrumb = new List<CategoryModel>();
            }

            public int ProductId { get; set; }
            public string ProductName { get; set; }
            public string ProductSeName { get; set; }
            public IList<CategoryModel> CategoryBreadcrumb { get; set; }
        }

        public partial class ProductVariantModel : EntityModelBase
        {
            public ProductVariantModel()
            {
                GiftCard = new GiftCardModel();
                ProductVariantPrice = new ProductVariantPriceModel();
                PictureModel = new PictureModel();
                AddToCart = new AddToCartModel();
                ProductVariantAttributes = new List<ProductVariantAttributeModel>();
				Combinations = new List<ProductVariantAttributeCombination>();
            }

            public string Name { get; set; }

            public bool ShowSku { get; set; }
            public string Sku { get; set; }

            public string Description { get; set; }

            public bool ShowManufacturerPartNumber { get; set; }
            public string ManufacturerPartNumber { get; set; }

            public bool ShowGtin { get; set; }
            public string Gtin { get; set; }

            public bool HasSampleDownload { get; set; }

            public GiftCardModel GiftCard { get; set; }

            public string StockAvailablity { get; set; }

            public bool IsCurrentCustomerRegistered { get; set; }
            public bool DisplayBackInStockSubscription { get; set; }
            public bool BackInStockAlreadySubscribed { get; set; }

            public ProductVariantPriceModel ProductVariantPrice { get; set; }

            public AddToCartModel AddToCart { get; set; }

            public PictureModel PictureModel { get; set; }

            public IList<ProductVariantAttributeModel> ProductVariantAttributes { get; set; }

            //codehint: sm-edit begin
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
            public DeliveryTime DeliveryTime { get; set; }
            public bool DisplayDeliveryTime { get; set; }
            public bool IsShipEnabled { get; set; }
            public bool DisplayDeliveryTimeAccordingToStock { get; set; }
            public bool IsBasePriceEnabled { get; set; }
            public string BasePriceInfo { get; set; }

			public IList<ProductVariantAttributeCombination> Combinations { get; set; }
			public ProductVariantAttributeCombination CombinationSelected { get; set; }
			public bool IsUnavailable { get; set; }
            //codehint: sm-edit end

            #region Nested Classes

            public partial class AddToCartModel : ModelBase
            {
                public AddToCartModel()
                {
                    this.AllowedQuantities = new List<SelectListItem>();
                }
                public int ProductVariantId { get; set; }

                [SmartResourceDisplayName("Products.Qty")]
                public int EnteredQuantity { get; set; }

                [SmartResourceDisplayName("Products.EnterProductPrice")]
                public bool CustomerEntersPrice { get; set; }
                [SmartResourceDisplayName("Products.EnterProductPrice")]
                public decimal CustomerEnteredPrice { get; set; }
                public String CustomerEnteredPriceRange { get; set; }
                
                public bool DisableBuyButton { get; set; }
                public bool DisableWishlistButton { get; set; }
                public List<SelectListItem> AllowedQuantities { get; set; }
                public bool AvailableForPreOrder { get; set; }
            }

            public partial class ProductVariantPriceModel : ModelBase
            {
                public string OldPrice { get; set; }

                public string Price { get; set; }
                public string PriceWithDiscount { get; set; }

                public decimal PriceValue { get; set; }
                public decimal PriceWithDiscountValue { get; set; }

                public bool CustomerEntersPrice { get; set; }

                public bool CallForPrice { get; set; }

                public int ProductVariantId { get; set; }

                public bool HidePrices { get; set; }

                public bool DynamicPriceUpdate { get; set; }
            }

            public partial class GiftCardModel : ModelBase
            {
                public bool IsGiftCard { get; set; }

                [SmartResourceDisplayName("Products.GiftCard.RecipientName")]
                [AllowHtml]
                public string RecipientName { get; set; }
                [SmartResourceDisplayName("Products.GiftCard.RecipientEmail")]
                [AllowHtml]
                public string RecipientEmail { get; set; }
                [SmartResourceDisplayName("Products.GiftCard.SenderName")]
                [AllowHtml]
                public string SenderName { get; set; }
                [SmartResourceDisplayName("Products.GiftCard.SenderEmail")]
                [AllowHtml]
                public string SenderEmail { get; set; }
                [SmartResourceDisplayName("Products.GiftCard.Message")]
                [AllowHtml]
                public string Message { get; set; }

                public GiftCardType GiftCardType { get; set; }
            }

            public partial class TierPriceModel : ModelBase
            {
                public string Price { get; set; }

                public int Quantity { get; set; }
            }

            public partial class ProductVariantAttributeModel : EntityModelBase
            {
                public ProductVariantAttributeModel()
                {
                    AllowedFileExtensions = new List<string>();
                    Values = new List<ProductVariantAttributeValueModel>();
                }

                public int ProductVariantId { get; set; }

                public int ProductAttributeId { get; set; }

                public string Alias { get; set; }

                public string Name { get; set; }

                public string Description { get; set; }

                public string TextPrompt { get; set; }

                public bool IsRequired { get; set; }

                /// <summary>
                /// Selected value for textboxes
                /// </summary>
                public string TextValue { get; set; }
                /// <summary>
                /// Selected day value for datepicker
                /// </summary>
                public int? SelectedDay { get; set; }
                /// <summary>
                /// Selected month value for datepicker
                /// </summary>
                public int? SelectedMonth { get; set; }
                /// <summary>
                /// Selected year value for datepicker
                /// </summary>
                public int? SelectedYear { get; set; }
                /// <summary>
                /// Allowed file extensions for customer uploaded files
                /// </summary>
                public IList<string> AllowedFileExtensions { get; set; }

                public AttributeControlType AttributeControlType { get; set; }
                
                public IList<ProductVariantAttributeValueModel> Values { get; set; }

            }

            public partial class ProductVariantAttributeValueModel : EntityModelBase
            {
                public string Name { get; set; }

                public string Alias { get; set; }

                public string ColorSquaresRgb { get; set; }

                public string PriceAdjustment { get; set; }

                public decimal PriceAdjustmentValue { get; set; }

                public bool IsPreSelected { get; set; }
            }
            #endregion
        }

		#endregion
    }

	/// <remarks>codehint: sm-add</remarks>
	public partial class ProductDetailsPictureModel : ModelBase
	{
		public ProductDetailsPictureModel() {
			PictureModels = new List<PictureModel>();
		}

		public string Name { get; set; }
		public string AlternateText { get; set; }
		public bool DefaultPictureZoomEnabled { get; set; }
        public string PictureZoomType { get; set; }
		public PictureModel DefaultPictureModel { get; set; }
		public IList<PictureModel> PictureModels { get; set; }
		public int GalleryStartIndex { get; set; }
	}

}