using SmartStore.Core.Configuration;
using SmartStore.PayPal.Services;

namespace SmartStore.PayPal.Settings
{
    public class PayPalInstalmentsSettings : PayPalApiSettingsBase, ISettings
    {
        public PayPalInstalmentsSettings()
        {
            TransactMode = TransactMode.Authorize;
            FinancingMin = 99.0M;
            FinancingMax = 5000.0M;
            ProductPagePromotion = PayPalPromotion.FinancingExample;
            CartPagePromotion = PayPalPromotion.FinancingExample;
        }

        public decimal FinancingMin { get; set; }
        public decimal FinancingMax { get; set; }

        //public bool Promote { get; set; }
        //public string PromotionWidgetZones { get; set; }
        //public int PromotionDisplayOrder { get; set; }

        public PayPalPromotion? ProductPagePromotion { get; set; }
        public PayPalPromotion? CartPagePromotion { get; set; }
        public PayPalPromotion? PaymentListPromotion { get; set; }
        public string Lender { get; set; }

        public bool IsAmountFinanceable(decimal amount)
        {
            if (amount == decimal.Zero)
            {
                return false;
            }

            return amount >= FinancingMin && amount <= FinancingMax;
        }
    }
}