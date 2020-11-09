using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Common;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Tax;
using SmartStore.Services.Catalog.Modelling;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Orders
{
    public class OrderModel : TabbableModel
    {
        public OrderModel()
        {
            TaxRates = new List<TaxRate>();
            GiftCards = new List<GiftCard>();
            Items = new List<OrderItemModel>();
            AutoUpdateOrderItem = new AutoUpdateOrderItemModel();
        }

        //identifiers
        [SmartResourceDisplayName("Admin.Orders.Fields.ID")]
        public override int Id { get; set; }
        [SmartResourceDisplayName("Admin.Orders.Fields.OrderNumber")]
        public string OrderNumber { get; set; }
        [SmartResourceDisplayName("Admin.Orders.Fields.OrderGuid")]
        public Guid OrderGuid { get; set; }

        //store
        [SmartResourceDisplayName("Admin.Orders.Fields.Store")]
        public string StoreName { get; set; }
        public string FromStore { get; set; }

        //customer info
        [SmartResourceDisplayName("Admin.Orders.Fields.Customer")]
        public int CustomerId { get; set; }

        [SmartResourceDisplayName("Admin.Orders.List.CustomerName")]
        public string CustomerName { get; set; }

        [SmartResourceDisplayName("Admin.Orders.Fields.CustomerEmail")]
        public string CustomerEmail { get; set; }

        [SmartResourceDisplayName("Admin.Orders.Fields.CustomerIP")]
        public string CustomerIp { get; set; }

        [SmartResourceDisplayName("Admin.Orders.Fields.Affiliate")]
        public int AffiliateId { get; set; }
        public string AffiliateFullName { get; set; }

        //totals
        public bool AllowCustomersToSelectTaxDisplayType { get; set; }
        public TaxDisplayType TaxDisplayType { get; set; }
        [SmartResourceDisplayName("Admin.Orders.Fields.OrderSubtotalInclTax")]
        public string OrderSubtotalInclTax { get; set; }
        [SmartResourceDisplayName("Admin.Orders.Fields.OrderSubtotalExclTax")]
        public string OrderSubtotalExclTax { get; set; }
        [SmartResourceDisplayName("Admin.Orders.Fields.OrderSubTotalDiscountInclTax")]
        public string OrderSubTotalDiscountInclTax { get; set; }
        [SmartResourceDisplayName("Admin.Orders.Fields.OrderSubTotalDiscountExclTax")]
        public string OrderSubTotalDiscountExclTax { get; set; }
        [SmartResourceDisplayName("Admin.Orders.Fields.OrderShippingInclTax")]
        public string OrderShippingInclTax { get; set; }
        [SmartResourceDisplayName("Admin.Orders.Fields.OrderShippingExclTax")]
        public string OrderShippingExclTax { get; set; }
        [SmartResourceDisplayName("Admin.Orders.Fields.PaymentMethodAdditionalFeeInclTax")]
        public string PaymentMethodAdditionalFeeInclTax { get; set; }
        [SmartResourceDisplayName("Admin.Orders.Fields.PaymentMethodAdditionalFeeExclTax")]
        public string PaymentMethodAdditionalFeeExclTax { get; set; }
        [SmartResourceDisplayName("Admin.Orders.Fields.Tax")]
        public string Tax { get; set; }
        public IList<TaxRate> TaxRates { get; set; }
        public bool DisplayTax { get; set; }
        public bool DisplayTaxRates { get; set; }
        [SmartResourceDisplayName("Admin.Orders.Fields.OrderTotalDiscount")]
        public string OrderTotalDiscount { get; set; }
        [SmartResourceDisplayName("Admin.Orders.Fields.RedeemedRewardPoints")]
        public int RedeemedRewardPoints { get; set; }
        [SmartResourceDisplayName("Admin.Orders.Fields.RedeemedRewardPoints")]
        public string RedeemedRewardPointsAmount { get; set; }

        [SmartResourceDisplayName("Admin.Orders.Fields.CreditBalance")]
        public string CreditBalance { get; set; }

        [SmartResourceDisplayName("Admin.Orders.Fields.OrderTotalRounding")]
        public string OrderTotalRounding { get; set; }
        [SmartResourceDisplayName("Admin.Orders.Fields.OrderTotal")]
        public string OrderTotal { get; set; }
        [SmartResourceDisplayName("Admin.Orders.Fields.RefundedAmount")]
        public string RefundedAmount { get; set; }

        // Quantity is only filled in context of product details (orders) to display an additional column
        // It is empty (null) in orders list
        [SmartResourceDisplayName("Admin.Orders.Products.Quantity")]
        public int? Quantity { get; set; }

        //edit totals
        [SmartResourceDisplayName("Admin.Orders.Fields.Edit.OrderSubtotal")]
        public decimal OrderSubtotalInclTaxValue { get; set; }
        [SmartResourceDisplayName("Admin.Orders.Fields.Edit.OrderSubtotal")]
        public decimal OrderSubtotalExclTaxValue { get; set; }
        [SmartResourceDisplayName("Admin.Orders.Fields.Edit.OrderSubTotalDiscount")]
        public decimal OrderSubTotalDiscountInclTaxValue { get; set; }
        [SmartResourceDisplayName("Admin.Orders.Fields.Edit.OrderSubTotalDiscount")]
        public decimal OrderSubTotalDiscountExclTaxValue { get; set; }
        [SmartResourceDisplayName("Admin.Orders.Fields.Edit.OrderShipping")]
        public decimal OrderShippingInclTaxValue { get; set; }
        [SmartResourceDisplayName("Admin.Orders.Fields.Edit.OrderShipping")]
        public decimal OrderShippingExclTaxValue { get; set; }
        [SmartResourceDisplayName("Admin.Orders.Fields.Edit.PaymentMethodAdditionalFee")]
        public decimal PaymentMethodAdditionalFeeInclTaxValue { get; set; }
        [SmartResourceDisplayName("Admin.Orders.Fields.Edit.PaymentMethodAdditionalFee")]
        public decimal PaymentMethodAdditionalFeeExclTaxValue { get; set; }
        [SmartResourceDisplayName("Admin.Orders.Fields.Edit.Tax")]
        public decimal TaxValue { get; set; }
        [SmartResourceDisplayName("Admin.Orders.Fields.Edit.TaxRates")]
        public string TaxRatesValue { get; set; }
        [SmartResourceDisplayName("Admin.Orders.Fields.Edit.OrderTotalDiscount")]
        public decimal OrderTotalDiscountValue { get; set; }

        [SmartResourceDisplayName("Admin.Orders.Fields.CreditBalance")]
        public decimal CreditBalanceValue { get; set; }

        [SmartResourceDisplayName("Admin.Orders.Fields.OrderTotalRounding")]
        public decimal OrderTotalRoundingValue { get; set; }
        [SmartResourceDisplayName("Admin.Orders.Fields.Edit.OrderTotal")]
        public decimal OrderTotalValue { get; set; }

        //associated recurring payment id
        [SmartResourceDisplayName("Admin.Orders.Fields.RecurringPayment")]
        public int RecurringPaymentId { get; set; }

        //order status
        [SmartResourceDisplayName("Admin.Orders.Fields.OrderStatus")]
        public string OrderStatus { get; set; }
        public OrderStatus StatusOrder { get; set; }

        public string OrderStatusLabelClass
        {
            get
            {
                switch (StatusOrder)
                {
                    case Core.Domain.Orders.OrderStatus.Pending:
                        return "fw-600";
                    case Core.Domain.Orders.OrderStatus.Processing:
                        return "";
                    case Core.Domain.Orders.OrderStatus.Complete:
                        return "text-success";
                    case Core.Domain.Orders.OrderStatus.Cancelled:
                        return "muted";
                    default:
                        return "";
                }
            }
        }

        //payment info
        [SmartResourceDisplayName("Admin.Orders.Fields.PaymentStatus")]
        public string PaymentStatus { get; set; }
        public PaymentStatus StatusPayment { get; set; }

        public bool HasPaymentMethod { get; set; }
        [SmartResourceDisplayName("Admin.Orders.Fields.PaymentMethod")]
        public string PaymentMethod { get; set; }
        public string WithPaymentMethod { get; set; }
        public string PaymentMethodSystemName { get; set; }

        public bool HasNewPaymentNotification { get; set; }

        public string PaymentStatusLabelClass
        {
            get
            {
                switch (StatusPayment)
                {
                    case Core.Domain.Payments.PaymentStatus.Pending:
                        return "fa fa-fw fa-circle text-danger";
                    case Core.Domain.Payments.PaymentStatus.Authorized:
                        return "fa fa-fw fa-circle text-warning";
                    case Core.Domain.Payments.PaymentStatus.Paid:
                        return "fa fa-fw fa-check text-success";
                    case Core.Domain.Payments.PaymentStatus.PartiallyRefunded:
                        return "fa fa-fw fa-exchange-alt text-warning";
                    case Core.Domain.Payments.PaymentStatus.Refunded:
                        return "fa fa-fw fa-exchange-alt text-success";
                    case Core.Domain.Payments.PaymentStatus.Voided:
                        return "fa fa-fw fa-ban muted";
                    default:
                        return "";
                }
            }
        }

        //credit card info
        public bool AllowStoringCreditCardNumber { get; set; }
        [SmartResourceDisplayName("Admin.Orders.Fields.CardType")]
        [AllowHtml]
        public string CardType { get; set; }
        [SmartResourceDisplayName("Admin.Orders.Fields.CardName")]
        [AllowHtml]
        public string CardName { get; set; }
        [SmartResourceDisplayName("Admin.Orders.Fields.CardNumber")]
        [AllowHtml]
        public string CardNumber { get; set; }
        [SmartResourceDisplayName("Admin.Orders.Fields.CardCVV2")]
        [AllowHtml]
        public string CardCvv2 { get; set; }
        [SmartResourceDisplayName("Admin.Orders.Fields.CardExpirationMonth")]
        [AllowHtml]
        public string CardExpirationMonth { get; set; }
        [SmartResourceDisplayName("Admin.Orders.Fields.CardExpirationYear")]
        [AllowHtml]
        public string CardExpirationYear { get; set; }

        public bool AllowStoringDirectDebit { get; set; }
        [SmartResourceDisplayName("Admin.Orders.Fields.DirectDebitAccountHolder")]
        [AllowHtml]
        public string DirectDebitAccountHolder { get; set; }

        [SmartResourceDisplayName("Admin.Orders.Fields.DirectDebitAccountNumber")]
        [AllowHtml]
        public string DirectDebitAccountNumber { get; set; }

        [SmartResourceDisplayName("Admin.Orders.Fields.DirectDebitBankCode")]
        [AllowHtml]
        public string DirectDebitBankCode { get; set; }

        [SmartResourceDisplayName("Admin.Orders.Fields.DirectDebitBankName")]
        [AllowHtml]
        public string DirectDebitBankName { get; set; }

        [SmartResourceDisplayName("Admin.Orders.Fields.DirectDebitBIC")]
        [AllowHtml]
        public string DirectDebitBIC { get; set; }

        [SmartResourceDisplayName("Admin.Orders.Fields.DirectDebitCountry")]
        [AllowHtml]
        public string DirectDebitCountry { get; set; }

        [SmartResourceDisplayName("Admin.Orders.Fields.DirectDebitIban")]
        [AllowHtml]
        public string DirectDebitIban { get; set; }

        //misc payment info
        public bool DisplayCompletePaymentNote { get; set; }
        public bool DisplayPurchaseOrderNumber { get; set; }

        [SmartResourceDisplayName("Admin.Orders.Fields.PurchaseOrderNumber")]
        public string PurchaseOrderNumber { get; set; }
        [SmartResourceDisplayName("Admin.Orders.Fields.AuthorizationTransactionID")]
        public string AuthorizationTransactionId { get; set; }
        [SmartResourceDisplayName("Admin.Orders.Fields.CaptureTransactionID")]
        public string CaptureTransactionId { get; set; }
        [SmartResourceDisplayName("Admin.Orders.Fields.SubscriptionTransactionID")]
        public string SubscriptionTransactionId { get; set; }

        [SmartResourceDisplayName("Admin.Orders.Fields.AuthorizationTransactionResult")]
        public string AuthorizationTransactionResult { get; set; }
        [SmartResourceDisplayName("Admin.Orders.Fields.CaptureTransactionResult")]
        public string CaptureTransactionResult { get; set; }

        //shipping info
        public bool IsShippable { get; set; }
        [SmartResourceDisplayName("Admin.Orders.Fields.ShippingStatus")]
        public string ShippingStatus { get; set; }
        public ShippingStatus StatusShipping { get; set; }

        [SmartResourceDisplayName("Admin.Orders.Fields.ShippingAddress")]
        public AddressModel ShippingAddress { get; set; }
        public string ShippingAddressString { get; set; }

        [SmartResourceDisplayName("Admin.Orders.Fields.OrderWeight")]
        public decimal OrderWeight { get; set; }
        public string BaseWeightIn { get; set; }

        [SmartResourceDisplayName("Admin.Orders.Fields.ShippingMethod")]
        public string ShippingMethod { get; set; }
        public string ViaShippingMethod { get; set; }
        public string ShippingAddressGoogleMapsUrl { get; set; }
        public bool CanAddNewShipments { get; set; }

        public string ShippingStatusLabelClass
        {
            get
            {
                switch (StatusShipping)
                {
                    case Core.Domain.Shipping.ShippingStatus.ShippingNotRequired:
                        return "fa fa-fw fa-download muted";
                    case Core.Domain.Shipping.ShippingStatus.NotYetShipped:
                        return "fa fa-fw fa-circle text-danger";
                    case Core.Domain.Shipping.ShippingStatus.PartiallyShipped:
                        return "fa fa-fw fa-truck fa-flip-horizontal text-warning";
                    case Core.Domain.Shipping.ShippingStatus.Shipped:
                        return "fa fa-fw fa-truck fa-flip-horizontal text-success";
                    case Core.Domain.Shipping.ShippingStatus.Delivered:
                        return "fa fa-fw fa-check text-success";
                    default:
                        return "";
                }
            }
        }

        //billing info
        [SmartResourceDisplayName("Admin.Orders.Fields.BillingAddress")]
        public AddressModel BillingAddress { get; set; }
        [SmartResourceDisplayName("Admin.Orders.Fields.VatNumber")]
        public string VatNumber { get; set; }

        //gift cards
        public IList<GiftCard> GiftCards { get; set; }

        //items
        public bool HasDownloadableProducts { get; set; }
        public IList<OrderItemModel> Items { get; set; }

        [SmartResourceDisplayName("Common.CreatedOn")]
        public DateTime CreatedOn { get; set; }
        public string CreatedOnString { get; set; }

        [SmartResourceDisplayName("Common.UpdatedOn")]
        public DateTime UpdatedOn { get; set; }

        [SmartResourceDisplayName("Admin.Orders.Fields.AcceptThirdPartyEmailHandOver")]
        public bool AcceptThirdPartyEmailHandOver { get; set; }

        public string CustomerComment { get; set; }

        //checkout attributes
        public string CheckoutAttributeInfo { get; set; }


        //order notes
        [SmartResourceDisplayName("Admin.Orders.OrderNotes.Fields.AddOrderNoteDisplayToCustomer")]
        public bool AddOrderNoteDisplayToCustomer { get; set; }
        [SmartResourceDisplayName("Admin.Orders.OrderNotes.Fields.AddOrderNoteMessage")]
        [AllowHtml]
        public string AddOrderNoteMessage { get; set; }

        public bool DisplayPdfInvoice { get; set; }


        //refund info
        [SmartResourceDisplayName("Admin.Orders.Fields.PartialRefund.AmountToRefund")]
        public decimal AmountToRefund { get; set; }
        public decimal MaxAmountToRefund { get; set; }
        public string MaxAmountToRefundFormatted { get; set; }

        //workflow info
        public bool CanCancelOrder { get; set; }
        public bool CanCompleteOrder { get; set; }
        public bool CanCapture { get; set; }
        public bool CanMarkOrderAsPaid { get; set; }
        public bool CanRefund { get; set; }
        public bool CanRefundOffline { get; set; }
        public bool CanPartiallyRefund { get; set; }
        public bool CanPartiallyRefundOffline { get; set; }
        public bool CanVoid { get; set; }
        public bool CanVoidOffline { get; set; }

        public string AggregatorProfit { get; set; }
        public string AggregatorTax { get; set; }
        public string AggregatorTotal { get; set; }

        public AutoUpdateOrderItemModel AutoUpdateOrderItem { get; set; }
        public string AutoUpdateOrderItemInfo { get; set; }

        #region Nested Classes

        public class OrderItemModel : EntityModelBase
        {
            public OrderItemModel()
            {
                PurchasedGiftCardIds = new List<int>();
                ReturnRequests = new List<ReturnRequestModel>();
                BundleItems = new List<BundleItemModel>();
            }
            public int ProductId { get; set; }
            public string ProductName { get; set; }
            public string Sku { get; set; }
            public ProductType ProductType { get; set; }
            public string ProductTypeName { get; set; }
            public string ProductTypeLabelHint { get; set; }

            public string UnitPriceInclTax { get; set; }
            public string UnitPriceExclTax { get; set; }
            public decimal UnitPriceInclTaxValue { get; set; }
            public decimal UnitPriceExclTaxValue { get; set; }
            public decimal TaxRate { get; set; }

            public int Quantity { get; set; }

            public string DiscountInclTax { get; set; }
            public string DiscountExclTax { get; set; }
            public decimal DiscountInclTaxValue { get; set; }
            public decimal DiscountExclTaxValue { get; set; }

            public string SubTotalInclTax { get; set; }
            public string SubTotalExclTax { get; set; }
            public decimal SubTotalInclTaxValue { get; set; }
            public decimal SubTotalExclTaxValue { get; set; }

            public string AttributeInfo { get; set; }
            public string RecurringInfo { get; set; }
            public IList<int> PurchasedGiftCardIds { get; set; }

            public bool IsDownload { get; set; }
            public int DownloadCount { get; set; }
            public DownloadActivationType DownloadActivationType { get; set; }
            public bool IsDownloadActivated { get; set; }
            public int? LicenseDownloadId { get; set; }

            public bool BundlePerItemPricing { get; set; }
            public bool BundlePerItemShoppingCart { get; set; }

            public IList<BundleItemModel> BundleItems { get; set; }
            public IList<ReturnRequestModel> ReturnRequests { get; set; }

            public bool IsReturnRequestPossible
            {
                get
                {
                    if (ReturnRequests != null && ReturnRequests.Count > 0)
                    {
                        return (ReturnRequests.Sum(x => x.Quantity) < Quantity);
                    }
                    return true;
                }
            }
        }

        public class ReturnRequestModel : EntityModelBase
        {
            public ReturnRequestStatus Status { get; set; }
            public int Quantity { get; set; }
            public string StatusString { get; set; }
            public string StatusLabel
            {
                get
                {
                    if (Status >= ReturnRequestStatus.RequestRejected)
                        return "warning";

                    if (Status >= ReturnRequestStatus.ReturnAuthorized)
                        return "success";

                    if (Status == ReturnRequestStatus.Received)
                        return "info";

                    if (Status == ReturnRequestStatus.Pending)
                        return "danger";

                    return "light";
                }
            }
        }

        public class BundleItemModel : ModelBase
        {
            public int ProductId { get; set; }
            public string Sku { get; set; }
            public string ProductName { get; set; }
            public string ProductSeName { get; set; }
            public bool VisibleIndividually { get; set; }
            public int Quantity { get; set; }
            public int DisplayOrder { get; set; }
            public string PriceWithDiscount { get; set; }
            public string AttributeInfo { get; set; }
        }

        public class TaxRate : ModelBase
        {
            public string Rate { get; set; }
            public string Value { get; set; }
        }

        public class GiftCard : ModelBase
        {
            [SmartResourceDisplayName("Admin.Orders.Fields.GiftCardInfo")]
            public string CouponCode { get; set; }
            public string Amount { get; set; }
        }

        public class OrderNote : EntityModelBase
        {
            public int OrderId { get; set; }

            [SmartResourceDisplayName("Admin.Orders.OrderNotes.Fields.DisplayToCustomer")]
            public bool DisplayToCustomer { get; set; }

            [SmartResourceDisplayName("Admin.Orders.OrderNotes.Fields.Note")]
            public string Note { get; set; }

            [SmartResourceDisplayName("Common.CreatedOn")]
            public DateTime CreatedOn { get; set; }
        }

        public class UploadLicenseModel : ModelBase
        {
            public int OrderId { get; set; }

            public int OrderItemId { get; set; }

            [UIHint("Download")]
            public int LicenseDownloadId { get; set; }

            public int OldLicenseDownloadId { get; set; }
        }

        public class AddOrderProductModel : ModelBase
        {
            public AddOrderProductModel()
            {
                AvailableCategories = new List<SelectListItem>();
                AvailableManufacturers = new List<SelectListItem>();
                AvailableProductTypes = new List<SelectListItem>();
            }

            [SmartResourceDisplayName("Admin.Catalog.Products.List.SearchProductName")]
            [AllowHtml]
            public string SearchProductName { get; set; }
            [SmartResourceDisplayName("Admin.Catalog.Products.List.SearchCategory")]
            public int SearchCategoryId { get; set; }
            [SmartResourceDisplayName("Admin.Catalog.Products.List.SearchManufacturer")]
            public int SearchManufacturerId { get; set; }
            [SmartResourceDisplayName("Admin.Catalog.Products.List.SearchProductType")]
            public int SearchProductTypeId { get; set; }

            public IList<SelectListItem> AvailableCategories { get; set; }
            public IList<SelectListItem> AvailableManufacturers { get; set; }
            public IList<SelectListItem> AvailableProductTypes { get; set; }

            public int OrderId { get; set; }

            #region Nested classes

            public class ProductModel : EntityModelBase
            {
                [SmartResourceDisplayName("Admin.Orders.Products.AddNew.Name")]
                [AllowHtml]
                public string Name { get; set; }

                [SmartResourceDisplayName("Admin.Orders.Products.AddNew.SKU")]
                [AllowHtml]
                public string Sku { get; set; }

                public string ProductTypeName { get; set; }
                public string ProductTypeLabelHint { get; set; }
            }

            public class ProductDetailsModel : ModelBase
            {
                public ProductDetailsModel()
                {
                    ProductVariantAttributes = new List<ProductVariantAttributeModel>();
                    GiftCard = new GiftCardModel();
                    Warnings = new List<string>();
                }

                public int ProductId { get; set; }

                public int OrderId { get; set; }

                public ProductType ProductType { get; set; }

                public string Name { get; set; }

                public string GiftCardFieldPrefix => GiftCardQueryItem.CreateKey(ProductId, 0, null);

                [SmartResourceDisplayName("Admin.Orders.Products.AddNew.UnitPriceInclTax")]
                public decimal UnitPriceInclTax { get; set; }
                [SmartResourceDisplayName("Admin.Orders.Products.AddNew.UnitPriceExclTax")]
                public decimal UnitPriceExclTax { get; set; }

                [SmartResourceDisplayName("Admin.Orders.Products.AddNew.TaxRate")]
                public decimal TaxRate { get; set; }

                [SmartResourceDisplayName("Admin.Orders.Products.AddNew.Quantity")]
                public int Quantity { get; set; }

                [SmartResourceDisplayName("Admin.Orders.Products.AddNew.SubTotalInclTax")]
                public decimal SubTotalInclTax { get; set; }
                [SmartResourceDisplayName("Admin.Orders.Products.AddNew.SubTotalExclTax")]
                public decimal SubTotalExclTax { get; set; }

                //product attrbiutes
                public IList<ProductVariantAttributeModel> ProductVariantAttributes { get; set; }
                //gift card info
                public GiftCardModel GiftCard { get; set; }

                public List<string> Warnings { get; set; }

                public bool ShowUpdateTotals { get; set; }

                [SmartResourceDisplayName("Admin.Orders.OrderItem.AutoUpdate.AdjustInventory")]
                public bool AdjustInventory { get; set; }

                [SmartResourceDisplayName("Admin.Orders.OrderItem.AutoUpdate.UpdateTotals")]
                public bool UpdateTotals { get; set; }
            }

            public class ProductVariantAttributeModel : EntityModelBase
            {
                public ProductVariantAttributeModel()
                {
                    Values = new List<ProductVariantAttributeValueModel>();
                }

                public int ProductAttributeId { get; set; }

                public string Name { get; set; }

                public string TextPrompt { get; set; }

                public bool IsRequired { get; set; }

                public AttributeControlType AttributeControlType { get; set; }

                public IList<ProductVariantAttributeValueModel> Values { get; set; }

                public string GetControlId(int productId, int bundleItemId)
                {
                    return ProductVariantQueryItem.CreateKey(productId, bundleItemId, ProductAttributeId, Id);
                }
            }

            public class ProductVariantAttributeValueModel : EntityModelBase
            {
                public string Name { get; set; }

                public bool IsPreSelected { get; set; }
            }


            public class GiftCardModel : ModelBase
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
            #endregion
        }

        #endregion
    }
}