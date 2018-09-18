using System;
using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Orders;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.UI.Choices;
using SmartStore.Web.Models.Common;
using SmartStore.Web.Models.Media;
using SmartStore.Web.Models.Catalog;
using SmartStore.Services.Catalog.Modelling;
using SmartStore.Services.Localization;

namespace SmartStore.Web.Models.ShoppingCart
{
    public partial class ShoppingCartModel : ModelBase
    {
        public ShoppingCartModel()
        {
            Items = new List<ShoppingCartItemModel>();
            Warnings = new List<string>();
            EstimateShipping = new EstimateShippingModel();
            DiscountBox = new DiscountBoxModel();
            GiftCardBox = new GiftCardBoxModel();
            RewardPoints = new RewardPointsBoxModel();
            CheckoutAttributes = new List<CheckoutAttributeModel>();
            OrderReviewData = new OrderReviewDataModel();

			ButtonPaymentMethods = new ButtonPaymentMethodModel();
        }

        public bool ShowSku { get; set; }
        public bool ShowProductImages { get; set; }
		public bool ShowProductBundleImages { get; set; }
        public bool IsEditable { get; set; }
        public IList<ShoppingCartItemModel> Items { get; set; }

        public string CheckoutAttributeInfo { get; set; }
        public IList<CheckoutAttributeModel> CheckoutAttributes { get; set; }

        public IList<string> Warnings { get; set; }
		public bool IsValidMinOrderSubtotal { get; set; }
        public bool TermsOfServiceEnabled { get; set; }
        public EstimateShippingModel EstimateShipping { get; set; }
        public DiscountBoxModel DiscountBox { get; set; }
        public GiftCardBoxModel GiftCardBox { get; set; }
        public RewardPointsBoxModel RewardPoints { get; set; }
        public OrderReviewDataModel OrderReviewData { get; set; }

        public int MediaDimensions { get; set; }
		public int BundleThumbSize { get; set; }
        public bool DisplayDeliveryTime { get; set; }
        public bool DisplayShortDesc { get; set; }
        public bool DisplayWeight { get; set; }
        public bool DisplayBasePrice { get; set; }

		public ButtonPaymentMethodModel ButtonPaymentMethods { get; set; }

        public bool DisplayCommentBox { get; set; }
        public string CustomerComment { get; set; }
        public string MeasureUnitName { get; set; }

		public bool DisplayEsdRevocationWaiverBox { get; set; }
        public bool DisplayMoveToWishlistButton { get; set; }

        #region Nested Classes

        public partial class ShoppingCartItemModel : EntityModelBase, IQuantityInput
        {
            public ShoppingCartItemModel()
            {
                Picture = new PictureModel();
                AllowedQuantities = new List<SelectListItem>();
                Warnings = new List<string>();
				ChildItems = new List<ShoppingCartItemModel>();
				BundleItem = new BundleItemModel();
            }
            public string Sku { get; set; }

            public PictureModel Picture {get;set;}

            public int ProductId { get; set; }

            public LocalizedValue<string> ProductName { get; set; }

            public string ProductSeName { get; set; }

			public string ProductUrl { get; set; }

			public bool VisibleIndividually { get; set; }

			public ProductType ProductType { get; set; }

            public string UnitPrice { get; set; }

            public string SubTotal { get; set; }

            public string Discount { get; set; }

            public int EnteredQuantity { get; set; }

            public LocalizedValue<string> QuantityUnitName { get; set; }

			public List<SelectListItem> AllowedQuantities { get; set; }

            public int MinOrderAmount { get; set; }

            public int MaxOrderAmount { get; set; }

            public int QuantityStep { get; set; }

            public QuantityControlType QuantiyControlType { get; set; }

            public string AttributeInfo { get; set; }

            public string RecurringInfo { get; set; }

            public IList<string> Warnings { get; set; }

            public decimal Weight { get; set; }

            public bool IsShipEnabled { get; set; }

            public LocalizedValue<string> DeliveryTimeName { get; set; }
            
            public string DeliveryTimeHexValue { get; set; }

            public LocalizedValue<string> ShortDesc { get; set; }
            
            public string BasePrice { get; set; }

			public bool IsDownload { get; set; }
			public bool HasUserAgreement { get; set; }

			public bool IsEsd { get; set; }

			public bool BundlePerItemPricing { get; set; }
			public bool BundlePerItemShoppingCart { get; set; }
			public BundleItemModel BundleItem { get; set; }
			public IList<ShoppingCartItemModel> ChildItems { get; set; }

			public bool DisableWishlistButton { get; set; }

			public DateTime CreatedOnUtc { get; set; }
        }

		public partial class BundleItemModel : EntityModelBase
		{
			public string PriceWithDiscount { get; set; }
			public int DisplayOrder { get; set; }
			public bool HideThumbnail { get; set; }
		}

        public partial class CheckoutAttributeModel : ChoiceModel
        {
			public override string BuildControlId()
			{
				return CheckoutAttributeQueryItem.CreateKey(Id);
			}

			public override string GetFileUploadUrl(UrlHelper url)
			{
				return url.Action("UploadFileCheckoutAttribute", "ShoppingCart", new { controlId = BuildControlId() });
			}
		}

        public partial class CheckoutAttributeValueModel : ChoiceItemModel
        {
			public override string GetItemLabel()
			{
				var label = Name;

				if (PriceAdjustment.HasValue())
				{
					label += " ({0})".FormatWith(PriceAdjustment);
				}

				return label;
			}
		}

        public partial class DiscountBoxModel: ModelBase
        {
            public bool Display { get; set; }
            public string Message { get; set; }
            public string CurrentCode { get; set; }
			public bool IsWarning { get; set; }
		}

        public partial class GiftCardBoxModel : ModelBase
        {
            public bool Display { get; set; }
            public string Message { get; set; }
			public bool IsWarning { get; set; }
		}

        public partial class RewardPointsBoxModel : ModelBase
        {
            public bool DisplayRewardPoints { get; set; }
            public int RewardPointsBalance { get; set; }
            public string RewardPointsAmount { get; set; }
            public bool UseRewardPoints { get; set; }
        }

        public partial class OrderReviewDataModel : ModelBase
        {
            public OrderReviewDataModel()
            {
                this.BillingAddress = new AddressModel();
                this.ShippingAddress = new AddressModel();
            }
            public bool Display { get; set; }

            public AddressModel BillingAddress { get; set; }

            public bool IsShippable { get; set; }
            public AddressModel ShippingAddress { get; set; }
            public string ShippingMethod { get; set; }
            public bool DisplayShippingMethodChangeOption { get; set; }

            public string PaymentMethod { get; set; }
			public string PaymentSummary { get; set; }
            public bool DisplayPaymentMethodChangeOption { get; set; }

            public bool IsPaymentSelectionSkipped { get; set; }
        }
		#endregion
    }
}