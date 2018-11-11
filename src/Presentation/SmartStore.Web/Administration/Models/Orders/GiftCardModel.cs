using System;
using System.Web.Mvc;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Orders
{
    public class GiftCardModel: EntityModelBase
    {
        [SmartResourceDisplayName("Admin.GiftCards.Fields.GiftCardType")]
        public int GiftCardTypeId { get; set; }

        [SmartResourceDisplayName("Admin.GiftCards.Fields.Order")]
        public int? PurchasedWithOrderId { get; set; }

        [SmartResourceDisplayName("Admin.GiftCards.Fields.Amount")]
        public decimal Amount { get; set; }

        [SmartResourceDisplayName("Admin.GiftCards.Fields.Amount")]
        public string AmountStr { get; set; }

        [SmartResourceDisplayName("Admin.GiftCards.Fields.RemainingAmount")]
        public string RemainingAmountStr { get; set; }

        [SmartResourceDisplayName("Admin.GiftCards.Fields.IsGiftCardActivated")]
        public bool IsGiftCardActivated { get; set; }

        [SmartResourceDisplayName("Admin.GiftCards.Fields.GiftCardCouponCode")]
        [AllowHtml]
        public string GiftCardCouponCode { get; set; }

        [SmartResourceDisplayName("Admin.GiftCards.Fields.RecipientName")]
        [AllowHtml]
        public string RecipientName { get; set; }

        [SmartResourceDisplayName("Admin.GiftCards.Fields.RecipientEmail")]
        [AllowHtml]
        public string RecipientEmail { get; set; }

        [SmartResourceDisplayName("Admin.GiftCards.Fields.SenderName")]
        [AllowHtml]
        public string SenderName { get; set; }

        [SmartResourceDisplayName("Admin.GiftCards.Fields.SenderEmail")]
        [AllowHtml]
        public string SenderEmail { get; set; }

        [SmartResourceDisplayName("Admin.GiftCards.Fields.Message")]
        [AllowHtml]
        public string Message { get; set; }

        [SmartResourceDisplayName("Admin.GiftCards.Fields.IsRecipientNotified")]
        public bool IsRecipientNotified { get; set; }

        [SmartResourceDisplayName("Admin.GiftCards.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        public string PrimaryStoreCurrencyCode { get; set; }

        #region Nested classes

        public class GiftCardUsageHistoryModel : EntityModelBase
        {
            [SmartResourceDisplayName("Admin.GiftCards.History.UsedValue")]
            public string UsedValue { get; set; }

            [SmartResourceDisplayName("Admin.GiftCards.History.Order")]
            public int OrderId { get; set; }

            [SmartResourceDisplayName("Admin.GiftCards.History.CreatedOn")]
            public DateTime CreatedOn { get; set; }
        }

        #endregion
    }
}