using System.Collections.Generic;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Web.Models.Checkout
{
    public partial class CheckoutConfirmModel : ModelBase
    {
        public CheckoutConfirmModel()
        {
            Warnings = new List<string>();
        }

        public string MinOrderTotalWarning { get; set; }

        //codehint: sm-add
        public bool TermsOfServiceEnabled { get; set; }

        public IList<string> Warnings { get; set; }

        //codehint: sm-add
        public bool ShowConfirmOrderLegalHint { get; set; }
    }
}