using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Web.Models.Checkout
{
    public partial class OnePageCheckoutModel : ModelBase
    {
        public bool ShippingRequired { get; set; }
    }
}