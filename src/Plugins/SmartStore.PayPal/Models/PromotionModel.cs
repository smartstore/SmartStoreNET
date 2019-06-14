using SmartStore.PayPal.Services;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.PayPal.Models
{
    public class PromotionModel : ModelBase
    {
        public PayPalPromotion Promotion { get; set; }
    }
}