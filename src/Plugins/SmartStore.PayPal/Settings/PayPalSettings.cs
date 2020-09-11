using System.Collections.Generic;
using SmartStore.Core.Configuration;

namespace SmartStore.PayPal.Settings
{
    public abstract class PayPalSettingsBase
    {
        public PayPalSettingsBase()
        {
            IpnChangesPaymentStatus = true;
            AddOrderNotes = true;
        }

        public bool UseSandbox { get; set; }

        public bool AddOrderNotes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to "additional fee" is specified as percentage. true - percentage, false - fixed value.
        /// </summary>
        public bool AdditionalFeePercentage { get; set; }

        public decimal AdditionalFee { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an IPN should change the payment status
        /// </summary>
        public bool IpnChangesPaymentStatus { get; set; }
    }

    public class PayPalApiSettingsBase : PayPalSettingsBase
    {
        public TransactMode TransactMode { get; set; }
        public string ApiAccountName { get; set; }
        public string ApiAccountPassword { get; set; }
        public string Signature { get; set; }

        /// <summary>
        /// PayPal client id
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// PayPal secret
        /// </summary>
        public string Secret { get; set; }

        /// <summary>
        /// PayPal experience profile id
        /// </summary>
        public string ExperienceProfileId { get; set; }

        /// <summary>
        /// PayPal webhook id
        /// </summary>
        public string WebhookId { get; set; }
    }


    public class PayPalDirectPaymentSettings : PayPalApiSettingsBase, ISettings
    {
        public PayPalDirectPaymentSettings()
        {
            TransactMode = TransactMode.Authorize;
        }
    }

    public class PayPalExpressPaymentSettings : PayPalApiSettingsBase, ISettings
    {
        public PayPalExpressPaymentSettings()
        {
            TransactMode = TransactMode.Authorize;
        }

        /// <summary>
        /// Determines whether the checkout button is displayed beneath the cart
        /// </summary>
        //public bool DisplayCheckoutButton { get; set; }

        /// <summary>
        /// Specifies whether to display the checkout button in mini shopping cart
        /// </summary>
        public bool ShowButtonInMiniShoppingCart { get; set; }

        /// <summary>
        /// Determines whether the shipment address has  to be confirmed by PayPal 
        /// </summary>
        public bool ConfirmedShipment { get; set; }

        /// <summary>
        /// Determines whether the shipment address is transmitted to PayPal
        /// </summary>
        public bool NoShipmentAddress { get; set; }

        /// <summary>
        /// Callback timeout
        /// </summary>
        public int CallbackTimeout { get; set; }

        /// <summary>
        /// Default shipping price
        /// </summary>
        public decimal DefaultShippingPrice { get; set; }
    }

    public class PayPalPlusPaymentSettings : PayPalApiSettingsBase, ISettings
    {
        public PayPalPlusPaymentSettings()
        {
            TransactMode = TransactMode.AuthorizeAndCapture;
        }

        /// <summary>
        /// Specifies other payment methods to be offered in payment wall
        /// </summary>
        public List<string> ThirdPartyPaymentMethods { get; set; }

        /// <summary>
        /// Specifies whether to display the logo of a third party payment method
        /// </summary>
        public bool DisplayPaymentMethodLogo { get; set; }

        /// <summary>
        /// Specifies whether to display the description of a third party payment method
        /// </summary>
        public bool DisplayPaymentMethodDescription { get; set; }
    }

    public class PayPalStandardPaymentSettings : PayPalSettingsBase, ISettings
    {
        public PayPalStandardPaymentSettings()
        {
            EnableIpn = true;
            IsShippingAddressRequired = true;
        }

        public string BusinessEmail { get; set; }
        public string PdtToken { get; set; }
        public bool PassProductNamesAndTotals { get; set; }
        public bool PdtValidateOrderTotal { get; set; }
        public bool PdtValidateOnlyWarn { get; set; }
        public bool EnableIpn { get; set; }
        public string IpnUrl { get; set; }

        /// <summary>
        /// Specifies whether to use PayPal shipping address. <c>true</c> use PayPal address, <c>false</c> use checkout address.
        /// </summary>
        public bool UsePayPalAddress { get; set; }

        /// <summary>
        /// Specifies whether a shipping address is required.
        /// </summary>
        public bool IsShippingAddressRequired { get; set; }
    }


    /// <summary>
    /// Represents payment processor transaction mode
    /// </summary>
    public enum TransactMode
    {
        Authorize = 1,
        AuthorizeAndCapture = 2
    }
}
