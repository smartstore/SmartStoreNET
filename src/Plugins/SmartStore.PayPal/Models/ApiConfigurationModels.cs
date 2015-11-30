using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using SmartStore.PayPal.Settings;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.PayPal.Models
{
    public abstract class ApiConfigurationModel: ModelBase
	{
        public string[] ConfigGroups { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.PayPal.UseSandbox")]
		public bool UseSandbox { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPal.TransactMode")]
		public int TransactMode { get; set; }
		public SelectList TransactModeValues { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPal.ApiAccountName")]
		public string ApiAccountName { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPal.ApiAccountPassword")]
		[DataType(DataType.Password)]
		public string ApiAccountPassword { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPal.Signature")]
		public string Signature { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPal.AdditionalFee")]
		public decimal AdditionalFee { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPal.AdditionalFeePercentage")]
		public bool AdditionalFeePercentage { get; set; }
	}

    public class PayPalDirectConfigurationModel : ApiConfigurationModel
    {
        public void Copy(PayPalDirectPaymentSettings settings, bool fromSettings)
        {
            if (fromSettings)
            {
                UseSandbox = settings.UseSandbox;
                TransactMode = Convert.ToInt32(settings.TransactMode);
                ApiAccountName = settings.ApiAccountName;
                ApiAccountPassword = settings.ApiAccountPassword;
                Signature = settings.Signature;
                AdditionalFee = settings.AdditionalFee;
                AdditionalFeePercentage = settings.AdditionalFeePercentage;
            }
            else
            {
                settings.UseSandbox = UseSandbox;
                settings.TransactMode = (TransactMode)TransactMode;
                settings.ApiAccountName = ApiAccountName;
                settings.ApiAccountPassword = ApiAccountPassword;
                settings.Signature = Signature;
                settings.AdditionalFee = AdditionalFee;
                settings.AdditionalFeePercentage = AdditionalFeePercentage;
            }
        }
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

        public void Copy(PayPalExpressPaymentSettings settings, bool fromSettings)
        {
            if (fromSettings)
            {
                UseSandbox = settings.UseSandbox;
                TransactMode = Convert.ToInt32(settings.TransactMode);
                ApiAccountName = settings.ApiAccountName;
                ApiAccountPassword = settings.ApiAccountPassword;
                Signature = settings.Signature;
                AdditionalFee = settings.AdditionalFee;
                AdditionalFeePercentage = settings.AdditionalFeePercentage;
                DisplayCheckoutButton = settings.DisplayCheckoutButton;
                ConfirmedShipment = settings.ConfirmedShipment;
                NoShipmentAddress = settings.NoShipmentAddress;
                CallbackTimeout = settings.CallbackTimeout;
                DefaultShippingPrice = settings.DefaultShippingPrice;
            }
            else
			{
                settings.UseSandbox = UseSandbox;
                settings.TransactMode = (TransactMode)TransactMode;
                settings.ApiAccountName = ApiAccountName;
                settings.ApiAccountPassword = ApiAccountPassword;
                settings.Signature = Signature;
                settings.AdditionalFee = AdditionalFee;
                settings.AdditionalFeePercentage = AdditionalFeePercentage;
                settings.DisplayCheckoutButton = DisplayCheckoutButton;
                settings.ConfirmedShipment = ConfirmedShipment;
                settings.NoShipmentAddress = NoShipmentAddress;
                settings.CallbackTimeout = CallbackTimeout;
                settings.DefaultShippingPrice = DefaultShippingPrice;
            }
        }

    }


}