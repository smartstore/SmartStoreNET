using System.Web.Routing;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Checkout
{
    public partial class CheckoutPaymentInfoModel : ModelBase
    {
        public string PaymentInfoActionName { get; set; }
        public string PaymentInfoControllerName { get; set; }
        public RouteValueDictionary PaymentInfoRouteValues { get; set; }

        /// <summary>
        /// Used on one-page checkout page
        /// </summary>
        public bool DisplayOrderTotals { get; set; }
    }
}