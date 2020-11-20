using System.Collections.Generic;
using SmartStore.Core.Domain.Orders;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Checkout
{
    public partial class CheckoutConfirmModel : ModelBase
    {
        public CheckoutConfirmModel()
        {
            Warnings = new List<string>();
        }

        public bool TermsOfServiceEnabled { get; set; }

        public IList<string> Warnings { get; set; }

        public bool ShowEsdRevocationWaiverBox { get; set; }

        public bool BypassPaymentMethodInfo { get; set; }

        public CheckoutNewsLetterSubscription NewsLetterSubscription { get; set; }
        public bool? SubscribeToNewsLetter { get; set; }

        public CheckoutThirdPartyEmailHandOver ThirdPartyEmailHandOver { get; set; }
        public string ThirdPartyEmailHandOverLabel { get; set; }
        public bool? AcceptThirdPartyEmailHandOver { get; set; }
    }
}