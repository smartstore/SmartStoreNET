using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Checkout
{
    public partial class CheckoutProgressModel : ModelBase
    {
        public CheckoutProgressStep CheckoutProgressStep { get; set; }

        public bool DisplayShippingOptions { get; set; }
    }

    public enum CheckoutProgressStep
    {
        Cart,
        Address,
        Shipping,
        Payment,
        Confirm,
        Complete
    }
}