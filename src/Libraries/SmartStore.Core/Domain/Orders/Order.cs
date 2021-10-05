using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Serialization;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Tax;

namespace SmartStore.Core.Domain.Orders
{
    /// <summary>
    /// Represents an order
    /// </summary>
    [DataContract]
    public partial class Order : BaseEntity, IAuditable, ISoftDeletable
    {
        private ICollection<WalletHistory> _walletHistory;
        private ICollection<DiscountUsageHistory> _discountUsageHistory;
        private ICollection<GiftCardUsageHistory> _giftCardUsageHistory;
        private ICollection<OrderNote> _orderNotes;
        private ICollection<OrderItem> _orderItems;
        private ICollection<Shipment> _shipments;

        #region Utilities

        protected virtual SortedDictionary<decimal, decimal> ParseTaxRates(string taxRatesStr)
        {
            var taxRatesDictionary = new SortedDictionary<decimal, decimal>();
            if (String.IsNullOrEmpty(taxRatesStr))
                return taxRatesDictionary;

            string[] lines = taxRatesStr.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                if (String.IsNullOrEmpty(line.Trim()))
                    continue;

                string[] taxes = line.Split(new char[] { ':' });
                if (taxes.Length == 2)
                {
                    try
                    {
                        decimal taxRate = decimal.Parse(taxes[0].Trim(), CultureInfo.InvariantCulture);
                        decimal taxValue = decimal.Parse(taxes[1].Trim(), CultureInfo.InvariantCulture);
                        taxRatesDictionary.Add(taxRate, taxValue);
                    }
                    catch (Exception exc)
                    {
                        Debug.WriteLine(exc.ToString());
                    }
                }
            }

            //add at least one tax rate (0%)
            if (taxRatesDictionary.Count == 0)
                taxRatesDictionary.Add(decimal.Zero, decimal.Zero);

            return taxRatesDictionary;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the (formatted) order number
        /// </summary>
		[DataMember]
        public string OrderNumber { get; set; }

        /// <summary>
        /// Gets or sets the order identifier
        /// </summary>
        [DataMember]
        public Guid OrderGuid { get; set; }

        /// <summary>
        /// Gets or sets the store identifier
        /// </summary>
        [DataMember]
        public int StoreId { get; set; }

        /// <summary>
        /// Gets or sets the customer identifier
        /// </summary>
		[DataMember]
        public int CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the billing address identifier
        /// </summary>
		[DataMember]
        public int BillingAddressId { get; set; }

        /// <summary>
        /// Gets or sets the shipping address identifier
        /// </summary>
		[DataMember]
        public int? ShippingAddressId { get; set; }

        /// <summary>
        /// Gets or sets an order status identifier
        /// </summary>
        [DataMember]
        public int OrderStatusId { get; set; }

        /// <summary>
        /// Gets or sets the shipping status identifier
        /// </summary>
		[DataMember]
        public int ShippingStatusId { get; set; }

        /// <summary>
        /// Gets or sets the payment status identifier
        /// </summary>
		[DataMember]
        public int PaymentStatusId { get; set; }

        /// <summary>
        /// Gets or sets the payment method system name
        /// </summary>
        [DataMember]
        public string PaymentMethodSystemName { get; set; }

        /// <summary>
        /// Gets or sets the customer currency code (at the moment of order placing)
        /// </summary>
        [DataMember]
        public string CustomerCurrencyCode { get; set; }

        /// <summary>
        /// Gets or sets the currency rate
        /// </summary>
        [DataMember]
        public decimal CurrencyRate { get; set; }

        /// <summary>
        /// Gets or sets the customer tax display type identifier
        /// </summary>
		[DataMember]
        public virtual int CustomerTaxDisplayTypeId { get; set; }

        /// <summary>
        /// Gets or sets the VAT number (the European Union Value Added Tax)
        /// </summary>
        [DataMember]
        public string VatNumber { get; set; }

        /// <summary>
        /// Gets or sets the order subtotal (incl tax)
        /// </summary>
        [DataMember]
        public decimal OrderSubtotalInclTax { get; set; }

        /// <summary>
        /// Gets or sets the order subtotal (excl tax)
        /// </summary>
        [DataMember]
        public decimal OrderSubtotalExclTax { get; set; }

        /// <summary>
        /// Gets or sets the order subtotal discount (incl tax)
        /// </summary>
        [DataMember]
        public decimal OrderSubTotalDiscountInclTax { get; set; }

        /// <summary>
        /// Gets or sets the order subtotal discount (excl tax)
        /// </summary>
        [DataMember]
        public decimal OrderSubTotalDiscountExclTax { get; set; }

        /// <summary>
        /// Gets or sets the order shipping (incl tax)
        /// </summary>
        [DataMember]
        public decimal OrderShippingInclTax { get; set; }

        /// <summary>
        /// Gets or sets the order shipping (excl tax)
        /// </summary>
        [DataMember]
        public decimal OrderShippingExclTax { get; set; }

        /// <summary>
        /// Gets or sets the tax rate for order shipping
        /// </summary>
        [DataMember]
        public decimal OrderShippingTaxRate { get; set; }

        /// <summary>
        /// Gets or sets the payment method additional fee (incl tax)
        /// </summary>
        [DataMember]
        public decimal PaymentMethodAdditionalFeeInclTax { get; set; }

        /// <summary>
        /// Gets or sets the payment method additional fee (excl tax)
        /// </summary>
        [DataMember]
        public decimal PaymentMethodAdditionalFeeExclTax { get; set; }

        /// <summary>
        /// Gets or sets the tax rate for payment method additional fee
        /// </summary>
        [DataMember]
        public decimal PaymentMethodAdditionalFeeTaxRate { get; set; }

        /// <summary>
        /// Gets or sets the tax rates
        /// </summary>
        [DataMember]
        public string TaxRates { get; set; }

        /// <summary>
        /// Gets or sets the order tax
        /// </summary>
        [DataMember]
        public decimal OrderTax { get; set; }

        /// <summary>
        /// Gets or sets the order discount (applied to order total)
        /// </summary>
        [DataMember]
        public decimal OrderDiscount { get; set; }

        /// <summary>
        /// Gets or sets the wallet credit amount used to (partially) pay this order.
        /// </summary>
        [DataMember]
        public decimal CreditBalance { get; set; }

        /// <summary>
        /// Gets or sets the order total rounding amount
        /// </summary>
        [DataMember]
        public decimal OrderTotalRounding { get; set; }

        /// <summary>
        /// Gets or sets the order total
        /// </summary>
        [DataMember]
        public decimal OrderTotal { get; set; }

        /// <summary>
        /// Gets or sets the refunded amount
        /// </summary>
        [DataMember]
        public decimal RefundedAmount { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether reward points were earned for this order
        /// </summary>
        [DataMember]
        public bool RewardPointsWereAdded { get; set; }

        /// <summary>
        /// Gets or sets the checkout attribute description
        /// </summary>
        [DataMember]
        public string CheckoutAttributeDescription { get; set; }

        /// <summary>
        /// Gets or sets the checkout attributes in XML format
        /// </summary>
        [DataMember]
        public string CheckoutAttributesXml { get; set; }

        /// <summary>
        /// Gets or sets the customer language identifier
        /// </summary>
        [DataMember]
        public int CustomerLanguageId { get; set; }

        /// <summary>
        /// Gets or sets the affiliate identifier
        /// </summary>
		[DataMember]
        public int AffiliateId { get; set; }

        /// <summary>
        /// Gets or sets the customer IP address
        /// </summary>
        [DataMember]
        public string CustomerIp { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether storing of credit card number is allowed
        /// </summary>
		[DataMember]
        public bool AllowStoringCreditCardNumber { get; set; }

        /// <summary>
        /// Gets or sets the card type
        /// </summary>
        [DataMember]
        public string CardType { get; set; }

        /// <summary>
        /// Gets or sets the card name
        /// </summary>
        public string CardName { get; set; }

        /// <summary>
        /// Gets or sets the card number
        /// </summary>
        public string CardNumber { get; set; }

        /// <summary>
        /// Gets or sets the masked credit card number
        /// </summary>
        public string MaskedCreditCardNumber { get; set; }

        /// <summary>
        /// Gets or sets the card CVV2
        /// </summary>
        public string CardCvv2 { get; set; }

        /// <summary>
        /// Gets or sets the card expiration month
        /// </summary>
        public string CardExpirationMonth { get; set; }

        /// <summary>
        /// Gets or sets the card expiration year
        /// </summary>
        public string CardExpirationYear { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether storing of credit card number is allowed
        /// </summary>
        public bool AllowStoringDirectDebit { get; set; }

        /// <summary>
        /// Gets or sets the direct debit account holder
        /// </summary>
        public string DirectDebitAccountHolder { get; set; }

        /// <summary>
        /// Gets or sets the direct debit account number
        /// </summary>
        public string DirectDebitAccountNumber { get; set; }

        /// <summary>
        /// Gets or sets the direct debit bank code
        /// </summary>
        public string DirectDebitBankCode { get; set; }

        /// <summary>
        /// Gets or sets the direct debit bank name
        /// </summary>
        public string DirectDebitBankName { get; set; }

        /// <summary>
        /// Gets or sets the direct debit bic
        /// </summary>
        public string DirectDebitBIC { get; set; }

        /// <summary>
        /// Gets or sets the direct debit country
        /// </summary>
        public string DirectDebitCountry { get; set; }

        /// <summary>
        /// Gets or sets the direct debit iban
        /// </summary>
        public string DirectDebitIban { get; set; }

        /// <summary>
        /// Gets or sets the customer order comment
        /// </summary>
        [DataMember]
        public string CustomerOrderComment { get; set; }

        /// <summary>
        /// Gets or sets the authorization transaction identifier
        /// </summary>
		[DataMember]
        public string AuthorizationTransactionId { get; set; }

        /// <summary>
        /// Gets or sets the authorization transaction code
        /// </summary>
		[DataMember]
        public string AuthorizationTransactionCode { get; set; }

        /// <summary>
        /// Gets or sets the authorization transaction result
        /// </summary>
		[DataMember]
        public string AuthorizationTransactionResult { get; set; }

        /// <summary>
        /// Gets or sets the capture transaction identifier
        /// </summary>
		[DataMember]
        public string CaptureTransactionId { get; set; }

        /// <summary>
        /// Gets or sets the capture transaction result
        /// </summary>
		[DataMember]
        public string CaptureTransactionResult { get; set; }

        /// <summary>
        /// Gets or sets the subscription transaction identifier
        /// </summary>
		[DataMember]
        public string SubscriptionTransactionId { get; set; }

        /// <summary>
        /// Gets or sets the purchase order number
        /// </summary>
		[DataMember]
        public string PurchaseOrderNumber { get; set; }

        /// <summary>
        /// Gets or sets the paid date and time
        /// </summary>
		[DataMember]
        public DateTime? PaidDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the shipping method
        /// </summary>
        [DataMember]
        public string ShippingMethod { get; set; }

        /// <summary>
        /// Gets or sets the shipping rate computation method identifier
        /// </summary>
		[DataMember]
        public string ShippingRateComputationMethodSystemName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity has been deleted
        /// </summary>
		[Index]
        public bool Deleted { get; set; }

        /// <summary>
        /// Gets or sets the date and time of order creation
        /// </summary>
		[DataMember]
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time when order was updated
        /// </summary>
        [DataMember]
        public DateTime UpdatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the amount of remaing reward points
        /// </summary>
        [DataMember]
        public int? RewardPointsRemaining { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a new payment notification arrived (IPN, webhook, callback etc.)
        /// </summary>
        [DataMember]
        public bool HasNewPaymentNotification { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the customer accepted to hand over email address to third party
        /// </summary>
        [DataMember]
        public bool AcceptThirdPartyEmailHandOver { get; set; }

        #endregion

        #region Navigation properties

        /// <summary>
        /// Gets or sets the customer
        /// </summary>
        [DataMember]
        public virtual Customer Customer { get; set; }

        /// <summary>
        /// Gets or sets the billing address
        /// </summary>
		[DataMember]
        public virtual Address BillingAddress { get; set; }

        /// <summary>
        /// Gets or sets the shipping address
        /// </summary>
		[DataMember]
        public virtual Address ShippingAddress { get; set; }

        /// <summary>
        /// Gets or sets the reward points history record
        /// </summary>
        [DataMember]
        public virtual RewardPointsHistory RedeemedRewardPointsEntry { get; set; }

        /// <summary>
        /// Gets or sets the wallet history.
        /// </summary>
        public virtual ICollection<WalletHistory> WalletHistory
        {
            get => _walletHistory ?? (_walletHistory = new HashSet<WalletHistory>());
            protected set => _walletHistory = value;
        }

        /// <summary>
        /// Gets or sets discount usage history
        /// </summary>
        public virtual ICollection<DiscountUsageHistory> DiscountUsageHistory
        {
            get => _discountUsageHistory ?? (_discountUsageHistory = new HashSet<DiscountUsageHistory>());
            protected set => _discountUsageHistory = value;
        }

        /// <summary>
        /// Gets or sets gift card usage history (gift card that were used with this order)
        /// </summary>
        public virtual ICollection<GiftCardUsageHistory> GiftCardUsageHistory
        {
            get => _giftCardUsageHistory ?? (_giftCardUsageHistory = new HashSet<GiftCardUsageHistory>());
            protected set => _giftCardUsageHistory = value;
        }

        /// <summary>
        /// Gets or sets order notes
        /// </summary>
		[DataMember]
        public virtual ICollection<OrderNote> OrderNotes
        {
            get => _orderNotes ?? (_orderNotes = new HashSet<OrderNote>());
            protected set => _orderNotes = value;
        }

        /// <summary>
        /// Gets or sets order items
        /// </summary>
		[DataMember]
        public virtual ICollection<OrderItem> OrderItems
        {
            get => _orderItems ?? (_orderItems = new HashSet<OrderItem>());
            protected internal set => _orderItems = value;
        }

        /// <summary>
        /// Gets or sets shipments
        /// </summary>
		[DataMember]
        public virtual ICollection<Shipment> Shipments
        {
            get => _shipments ?? (_shipments = new HashSet<Shipment>());
            protected set => _shipments = value;
        }

        #endregion

        #region Custom properties

        /// <summary>
        /// Gets or sets the order status
        /// </summary>
		[DataMember]
        public OrderStatus OrderStatus
        {
            get => (OrderStatus)this.OrderStatusId;
            set => this.OrderStatusId = (int)value;
        }

        /// <summary>
        /// Gets or sets the payment status
        /// </summary>
		[DataMember]
        public PaymentStatus PaymentStatus
        {
            get => (PaymentStatus)this.PaymentStatusId;
            set => this.PaymentStatusId = (int)value;
        }

        /// <summary>
        /// Gets or sets the shipping status
        /// </summary>
		[DataMember]
        public ShippingStatus ShippingStatus
        {
            get => (ShippingStatus)this.ShippingStatusId;
            set => this.ShippingStatusId = (int)value;
        }

        /// <summary>
        /// Gets or sets the customer tax display type
        /// </summary>
		[DataMember]
        public TaxDisplayType CustomerTaxDisplayType
        {
            get => (TaxDisplayType)this.CustomerTaxDisplayTypeId;
            set => this.CustomerTaxDisplayTypeId = (int)value;
        }

        /// <summary>
        /// Gets the applied tax rates
        /// </summary>
        public SortedDictionary<decimal, decimal> TaxRatesDictionary => ParseTaxRates(this.TaxRates);

        #endregion
    }
}
