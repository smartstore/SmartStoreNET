using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Web.Framework.Mvc;
using SmartStore.Web.Models.Common;
using SmartStore.Web.Models.Media;

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
        public string MinOrderSubtotalWarning { get; set; }
        public bool TermsOfServiceEnabled { get; set; }
        public EstimateShippingModel EstimateShipping { get; set; }
        public DiscountBoxModel DiscountBox { get; set; }
        public GiftCardBoxModel GiftCardBox { get; set; }
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

		#region Nested Classes

        public partial class ShoppingCartItemModel : EntityModelBase
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

            public string ProductName { get; set; }

            public string ProductSeName { get; set; }

			public bool VisibleIndividually { get; set; }

			public ProductType ProductType { get; set; }

            public string UnitPrice { get; set; }

            public string SubTotal { get; set; }

            public string Discount { get; set; }

            public int Quantity { get; set; }
            public List<SelectListItem> AllowedQuantities { get; set; }
            
            public string AttributeInfo { get; set; }

            public string RecurringInfo { get; set; }

            public IList<string> Warnings { get; set; }

            public decimal Weight { get; set; }

            public bool IsShipEnabled { get; set; }

            public string QuantityUnit { get; set; }

            public string DeliveryTimeName { get; set; }
            
            public string DeliveryTimeHexValue { get; set; }

            public string ShortDesc { get; set; }
            
            public string BasePrice { get; set; }

			public bool BundlePerItemPricing { get; set; }
			public bool BundlePerItemShoppingCart { get; set; }
			public BundleItemModel BundleItem { get; set; }
			public IList<ShoppingCartItemModel> ChildItems { get; set; }
        }

		public partial class BundleItemModel : EntityModelBase
		{
			public string PriceWithDiscount { get; set; }
			public int DisplayOrder { get; set; }
			public bool HideThumbnail { get; set; }
		}

        public partial class CheckoutAttributeModel : EntityModelBase
        {
            public CheckoutAttributeModel()
            {
                Values = new List<CheckoutAttributeValueModel>();
            }

            public string Name { get; set; }

            public string DefaultValue { get; set; }

            public string TextPrompt { get; set; }

            public bool IsRequired { get; set; }

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

            public AttributeControlType AttributeControlType { get; set; }

            public IList<CheckoutAttributeValueModel> Values { get; set; }
        }

        public partial class CheckoutAttributeValueModel : EntityModelBase
        {
            public string Name { get; set; }

            public string PriceAdjustment { get; set; }

            public bool IsPreSelected { get; set; }
        }

        public partial class DiscountBoxModel: ModelBase
        {
            public bool Display { get; set; }
            public string Message { get; set; }
            public string CurrentCode { get; set; }
        }

        public partial class GiftCardBoxModel : ModelBase
        {
            public bool Display { get; set; }
            public string Message { get; set; }
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

            public string PaymentMethod { get; set; }
			public string PaymentSummary { get; set; }

			public bool IsPaymentSelectionSkipped { get; set; }
        }
		#endregion
    }
}