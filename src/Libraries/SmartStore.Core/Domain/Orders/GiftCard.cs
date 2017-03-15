using System;
using System.Collections.Generic;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Core.Domain.Orders
{
    /// <summary>
    /// Represents a gift card
    /// </summary>
    public partial class GiftCard : BaseEntity
    {
        private ICollection<GiftCardUsageHistory> _giftCardUsageHistory;
        
        /// <summary>
        /// Gets or sets the associated order item identifier
        /// </summary>
        public int? PurchasedWithOrderItemId { get; set; }

        /// <summary>
        /// Gets or sets the gift card type identifier
        /// </summary>
        public int GiftCardTypeId { get; set; }

        /// <summary>
        /// Gets or sets the amount
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gift card is activated
        /// </summary>
        public bool IsGiftCardActivated { get; set; }

        /// <summary>
        /// Gets or sets a gift card coupon code
        /// </summary>
        public string GiftCardCouponCode { get; set; }

        /// <summary>
        /// Gets or sets a recipient name
        /// </summary>
        public string RecipientName { get; set; }

        /// <summary>
        /// Gets or sets a recipient email
        /// </summary>
        public string RecipientEmail { get; set; }

        /// <summary>
        /// Gets or sets a sender name
        /// </summary>
        public string SenderName { get; set; }

        /// <summary>
        /// Gets or sets a sender email
        /// </summary>
        public string SenderEmail { get; set; }

        /// <summary>
        /// Gets or sets a message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether recipient is notified
        /// </summary>
        public bool IsRecipientNotified { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }
        
        /// <summary>
        /// Gets or sets the gift card usage history
        /// </summary>
        public virtual ICollection<GiftCardUsageHistory> GiftCardUsageHistory
        {
			get { return _giftCardUsageHistory ?? (_giftCardUsageHistory = new HashSet<GiftCardUsageHistory>()); }
            protected set { _giftCardUsageHistory = value; }
        }
        
        /// <summary>
        /// Gets or sets the gift card type
        /// </summary>
        public  GiftCardType GiftCardType
        {
            get
            {
                return (GiftCardType)this.GiftCardTypeId;
            }
            set
            {
                this.GiftCardTypeId = (int)value;
            }
        }
        
        /// <summary>
        /// Gets or sets the associated order item
        /// </summary>
        public virtual OrderItem PurchasedWithOrderItem { get; set; }

        #region Methods

        /// <summary>
        /// Gets a gift card remaining amount
        /// </summary>
        /// <returns>Gift card remaining amount</returns>
        public decimal GetGiftCardRemainingAmount()
        {
            decimal result = this.Amount;

            foreach (var gcuh in this.GiftCardUsageHistory)
                result -= gcuh.UsedValue;

            if (result < decimal.Zero)
                result = decimal.Zero;

            return result;
        }

        /// <summary>
        /// Is gift card valid
        /// </summary>
		/// <param name="storeId">Store identifier. 0 validates the gift card for all stores</param>
        /// <returns>Result</returns>
        public bool IsGiftCardValid(int storeId)
        {
            if (!this.IsGiftCardActivated)
                return false;

			if (storeId != 0 && 
				PurchasedWithOrderItemId.HasValue && PurchasedWithOrderItem != null &&
				PurchasedWithOrderItem.Order != null)
			{
				if (PurchasedWithOrderItem.Order.StoreId != storeId)
					return false;
			}

			decimal remainingAmount = GetGiftCardRemainingAmount();
            if (remainingAmount > decimal.Zero)
                return true;

            return false;
        }

        #endregion
    }
}
