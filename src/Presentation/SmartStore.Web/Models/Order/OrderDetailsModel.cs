using System;
using System.Collections.Generic;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Models.Common;
using SmartStore.Web.Models.Media;
using SmartStore.Services.Localization;

namespace SmartStore.Web.Models.Order
{
	public partial class OrderDetailsModel : EntityModelBase
    {
        public OrderDetailsModel()
        {
			MerchantCompanyInfo = new CompanyInformationSettings();
			
			TaxRates = new List<TaxRate>();
            GiftCards = new List<GiftCard>();
            Items = new List<OrderItemModel>();
            OrderNotes = new List<OrderNote>();
            Shipments = new List<ShipmentBriefModel>();

            BillingAddress = new AddressModel();
            ShippingAddress = new AddressModel();
        }

		public int StoreId { get; set; }

		public CompanyInformationSettings MerchantCompanyInfo { get; set; }

        public string OrderNumber { get; set; }
        public bool DisplayPdfInvoice { get; set; }
		public bool RenderOrderNotes { get; set; }

        public DateTime CreatedOn { get; set; }

        public string OrderStatus { get; set; }

        public bool IsReOrderAllowed { get; set; }

        public bool IsReturnRequestAllowed { get; set; }

        public bool IsShippable { get; set; }
        public string ShippingStatus { get; set; }
        public AddressModel ShippingAddress { get; set; }
        public string ShippingMethod { get; set; }
        public IList<ShipmentBriefModel> Shipments { get; set; }

        public AddressModel BillingAddress { get; set; }

        public string VatNumber { get; set; }

        public string PaymentMethod { get; set; }
        public bool CanRePostProcessPayment { get; set; }
        public bool DisplayPurchaseOrderNumber { get; set; }
        public string PurchaseOrderNumber { get; set; }

        public string OrderSubtotal { get; set; }
        public string OrderSubTotalDiscount { get; set; }
        public string OrderShipping { get; set; }
        public string PaymentMethodAdditionalFee { get; set; }
        public string CheckoutAttributeInfo { get; set; }
        public string Tax { get; set; }
        public IList<TaxRate> TaxRates { get; set; }
        public bool DisplayTax { get; set; }
        public bool DisplayTaxRates { get; set; }
        public string OrderTotalDiscount { get; set; }
        public int RedeemedRewardPoints { get; set; }
        public string RedeemedRewardPointsAmount { get; set; }
		public string CreditBalance { get; set; }
		public string OrderTotalRounding { get; set; }
        public string OrderTotal { get; set; }
        public string CustomerComment { get; set; }

        public IList<GiftCard> GiftCards { get; set; }

        public bool ShowSku { get; set; }
		public bool ShowProductImages { get; set; }
		public IList<OrderItemModel> Items { get; set; }

        public IList<OrderNote> OrderNotes { get; set; }

        #region Nested Classes

        public partial class OrderItemModel : EntityModelBase
        {
			public OrderItemModel()
			{
				BundleItems = new List<BundleItemModel>();
			}

            public string Sku { get; set; }
            public int ProductId { get; set; }
            public LocalizedValue<string> ProductName { get; set; }
            public string ProductSeName { get; set; }
			public string ProductUrl { get; set; }
			public ProductType ProductType { get; set; }
            public string UnitPrice { get; set; }
            public string SubTotal { get; set; }
            public int Quantity { get; set; }
            public string QuantityUnit { get; set; }
            public string AttributeInfo { get; set; }
			public bool BundlePerItemPricing { get; set; }
			public bool BundlePerItemShoppingCart { get; set; }
			public PictureModel Picture { get; set; }

			public IList<BundleItemModel> BundleItems { get; set; }
        }

		public partial class BundleItemModel : ModelBase
		{
			public string Sku { get; set; }
			public string ProductName { get; set; }
			public string ProductSeName { get; set; }
			public string ProductUrl { get; set; }
			public bool VisibleIndividually { get; set; }
			public int Quantity { get; set; }
			public int DisplayOrder { get; set; }
			public string PriceWithDiscount { get; set; }
			public string AttributeInfo { get; set; }
		}

        public partial class TaxRate : ModelBase
        {
            public string Rate { get; set; }
            public string Value { get; set; }
			public string Label { get; set; }
        }

        public partial class GiftCard : ModelBase
        {
            public string CouponCode { get; set; }
            public string Amount { get; set; }
			public string Remaining { get; set; }
		}

        public partial class OrderNote : ModelBase
        {
            public string Note { get; set; }
            public DateTime CreatedOn { get; set; }
			public string FriendlyCreatedOn { get; set; }
		}

        public partial class ShipmentBriefModel : EntityModelBase
        {
            public string TrackingNumber { get; set; }
            public DateTime? ShippedDate { get; set; }
            public DateTime? DeliveryDate { get; set; }
        }
        #endregion
    }
}