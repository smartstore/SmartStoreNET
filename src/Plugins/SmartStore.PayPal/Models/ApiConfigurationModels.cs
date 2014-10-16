using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.PayPal.Models
{
    public abstract class ApiConfigurationModel: ModelBase
	{
		[SmartResourceDisplayName("Plugins.Payments.PayPalDirect.Fields.UseSandbox")]
		public bool UseSandbox { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPalDirect.Fields.TransactMode")]
		public int TransactMode { get; set; }
		public SelectList TransactModeValues { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPalDirect.Fields.ApiAccountName")]
		public string ApiAccountName { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPalDirect.Fields.ApiAccountPassword")]
		public string ApiAccountPassword { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPalDirect.Fields.Signature")]
		public string Signature { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPalDirect.Fields.AdditionalFee")]
		public decimal AdditionalFee { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPalDirect.Fields.AdditionalFeePercentage")]
		public bool AdditionalFeePercentage { get; set; }
	}

    public class PayPalDirectConfigurationModel : ApiConfigurationModel
    { 
    }

    public class PayPalExpressConfigurationModel : ApiConfigurationModel
    {
        [SmartResourceDisplayName("Plugins.Payments.PayPalExpress.Fields.DisplayCheckoutButton")]
        public bool DisplayCheckoutButton { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.PayPalExpress.Fields.ConfirmedShipment")]
        public bool ConfirmedShipment { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.PayPalExpress.Fields.NoShipmentAddress")]
        public bool NoShipmentAddress { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.PayPalExpress.Fields.CallbackTimeout")]
        public int CallbackTimeout { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.PayPalExpress.Fields.DefaultShippingPrice")]
        public decimal DefaultShippingPrice { get; set; }
    }
}