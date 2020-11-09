using SmartStore.AmazonPay.Services;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Models.Common;

namespace SmartStore.AmazonPay.Models
{
    public class AmazonPayViewModel : ModelBase
    {
        public AmazonPayViewModel()
        {
            IsShippable = true;
            RedirectAction = "Cart";
            RedirectController = "ShoppingCart";
            Result = AmazonPayResultType.PluginView;
            BillingAddress = new AddressModel();
        }

        public string SystemName => AmazonPayPlugin.SystemName;

        public string SellerId { get; set; }
        public string ClientId { get; set; }

        /// <summary>
        /// Amazon widget script URL
        /// </summary>
        public string WidgetUrl { get; set; }
        public string ButtonHandlerUrl { get; set; }

        public bool IsShippable { get; set; }
        public bool IsRecurring { get; set; }

        public string LanguageCode { get; set; }
        public AmazonPayRequestType Type { get; set; }
        public AmazonPayResultType Result { get; set; }

        public string RedirectAction { get; set; }
        public string RedirectController { get; set; }

        public string OrderReferenceId { get; set; }
        public string AddressConsentToken { get; set; }
        public string Warning { get; set; }
        public bool Logout { get; set; }

        public string ButtonType { get; set; }
        public string ButtonColor { get; set; }
        public string ButtonSize { get; set; }

        public string ShippingMethod { get; set; }
        public AddressModel BillingAddress { get; set; }

        // Confirmation flow.
        public bool IsConfirmed { get; set; }
        public string FormData { get; set; }
        public bool SubmitForm { get; set; }
    }
}