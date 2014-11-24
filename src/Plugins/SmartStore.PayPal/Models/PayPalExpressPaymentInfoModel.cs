using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.PayPal.Models
{
    public class PayPalExpressPaymentInfoModel : ModelBase
    {
        public PayPalExpressPaymentInfoModel()
        {
            
        }

        public bool CurrentPageIsBasket { get; set; }

        public string SubmitButtonImageUrl { get; set; }

    }
}