using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Web.Mvc;
using SmartStore.PayPal.Services;
using SmartStore.PayPal.Settings;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.PayPal.Models
{
	public abstract class ApiConfigurationModel: ModelBase
	{
        public string[] ConfigGroups { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPal.UseSandbox")]
		public bool UseSandbox { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPal.IpnChangesPaymentStatus")]
		public bool IpnChangesPaymentStatus { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPal.TransactMode")]
		public int TransactMode { get; set; }
		public SelectList TransactModeValues { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPal.SecurityProtocol")]
		public SecurityProtocolType? SecurityProtocol { get; set; }
		public List<SelectListItem> AvailableSecurityProtocols { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPal.ApiAccountName")]
		public string ApiAccountName { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPal.ApiAccountPassword")]
		[DataType(DataType.Password)]
		public string ApiAccountPassword { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPal.Signature")]
		public string Signature { get; set; }

		[SmartResourceDisplayName("Plugins.SmartStore.PayPal.ClientId")]
		public string ClientId { get; set; }

		[SmartResourceDisplayName("Plugins.SmartStore.PayPal.Secret")]
		public string Secret { get; set; }

		[SmartResourceDisplayName("Plugins.SmartStore.PayPal.ExperienceProfileId")]
		public string ExperienceProfileId { get; set; }

		[SmartResourceDisplayName("Plugins.SmartStore.PayPal.WebhookId")]
		public string WebhookId { get; set; }

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
				SecurityProtocol = settings.SecurityProtocol;
				UseSandbox = settings.UseSandbox;
				IpnChangesPaymentStatus = settings.IpnChangesPaymentStatus;
                TransactMode = Convert.ToInt32(settings.TransactMode);
                ApiAccountName = settings.ApiAccountName;
                ApiAccountPassword = settings.ApiAccountPassword;
                Signature = settings.Signature;
                AdditionalFee = settings.AdditionalFee;
                AdditionalFeePercentage = settings.AdditionalFeePercentage;
            }
            else
            {
				settings.SecurityProtocol = SecurityProtocol;
				settings.UseSandbox = UseSandbox;
				settings.IpnChangesPaymentStatus = IpnChangesPaymentStatus;
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
		[SmartResourceDisplayName("Plugins.Payments.PayPalExpress.Fields.ShowButtonInMiniShoppingCart")]
		public bool ShowButtonInMiniShoppingCart { get; set; }

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
				SecurityProtocol = settings.SecurityProtocol;
				UseSandbox = settings.UseSandbox;
				IpnChangesPaymentStatus = settings.IpnChangesPaymentStatus;
				TransactMode = Convert.ToInt32(settings.TransactMode);
				ApiAccountName = settings.ApiAccountName;
                ApiAccountPassword = settings.ApiAccountPassword;
                Signature = settings.Signature;
                AdditionalFee = settings.AdditionalFee;
                AdditionalFeePercentage = settings.AdditionalFeePercentage;
				ShowButtonInMiniShoppingCart = settings.ShowButtonInMiniShoppingCart;
                ConfirmedShipment = settings.ConfirmedShipment;
                NoShipmentAddress = settings.NoShipmentAddress;
                CallbackTimeout = settings.CallbackTimeout;
                DefaultShippingPrice = settings.DefaultShippingPrice;
            }
            else
			{
				settings.SecurityProtocol = SecurityProtocol;
				settings.UseSandbox = UseSandbox;
				settings.IpnChangesPaymentStatus = IpnChangesPaymentStatus;
                settings.TransactMode = (TransactMode)TransactMode;
				settings.ApiAccountName = ApiAccountName;
                settings.ApiAccountPassword = ApiAccountPassword;
                settings.Signature = Signature;
                settings.AdditionalFee = AdditionalFee;
                settings.AdditionalFeePercentage = AdditionalFeePercentage;
				settings.ShowButtonInMiniShoppingCart = ShowButtonInMiniShoppingCart;
                settings.ConfirmedShipment = ConfirmedShipment;
                settings.NoShipmentAddress = NoShipmentAddress;
                settings.CallbackTimeout = CallbackTimeout;
                settings.DefaultShippingPrice = DefaultShippingPrice;
            }
        }
    }


	public class PayPalPlusConfigurationModel : ApiConfigurationModel
	{
		[SmartResourceDisplayName("Plugins.Payments.PayPalPlus.ThirdPartyPaymentMethods")]
		public List<string> ThirdPartyPaymentMethods { get; set; }
		public List<SelectListItem> AvailableThirdPartyPaymentMethods { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPalPlus.DisplayPaymentMethodLogo")]
		public bool DisplayPaymentMethodLogo { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPalPlus.DisplayPaymentMethodDescription")]
		public bool DisplayPaymentMethodDescription { get; set; }


		public void Copy(PayPalPlusPaymentSettings settings, bool fromSettings)
		{
			if (fromSettings)
			{
				SecurityProtocol = settings.SecurityProtocol;
				UseSandbox = settings.UseSandbox;
				TransactMode = (int)Settings.TransactMode.AuthorizeAndCapture;
				AdditionalFee = settings.AdditionalFee;
				AdditionalFeePercentage = settings.AdditionalFeePercentage;

				ClientId = settings.ClientId;
				Secret = settings.Secret;
				ExperienceProfileId = settings.ExperienceProfileId;
				WebhookId = settings.WebhookId;
				ThirdPartyPaymentMethods = settings.ThirdPartyPaymentMethods;
				DisplayPaymentMethodLogo = settings.DisplayPaymentMethodLogo;
				DisplayPaymentMethodDescription = settings.DisplayPaymentMethodDescription;
			}
			else
			{
				settings.SecurityProtocol = SecurityProtocol;
				settings.UseSandbox = UseSandbox;
				settings.TransactMode = Settings.TransactMode.AuthorizeAndCapture;
				settings.AdditionalFee = AdditionalFee;
				settings.AdditionalFeePercentage = AdditionalFeePercentage;

				settings.ClientId = ClientId;
				settings.Secret = Secret;
				settings.ExperienceProfileId = ExperienceProfileId;
				settings.WebhookId = WebhookId;
				settings.ThirdPartyPaymentMethods = ThirdPartyPaymentMethods;
				settings.DisplayPaymentMethodLogo = DisplayPaymentMethodLogo;
				settings.DisplayPaymentMethodDescription = DisplayPaymentMethodDescription;
			}
		}
	}
}