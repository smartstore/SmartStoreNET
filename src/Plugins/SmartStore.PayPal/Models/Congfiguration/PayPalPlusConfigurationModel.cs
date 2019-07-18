using System;
using System.Collections.Generic;
using FluentValidation;
using SmartStore.Core.Localization;
using SmartStore.PayPal.Settings;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Validators;

namespace SmartStore.PayPal.Models
{
    public class PayPalPlusConfigurationModel : ApiConfigurationModel
    {
        public PayPalPlusConfigurationModel()
        {
            TransactMode = TransactMode.AuthorizeAndCapture;
        }

        [SmartResourceDisplayName("Plugins.Payments.PayPalPlus.ThirdPartyPaymentMethods")]
        public List<string> ThirdPartyPaymentMethods { get; set; }
        public IList<ExtendedSelectListItem> AvailableThirdPartyPaymentMethods { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.PayPalPlus.DisplayPaymentMethodLogo")]
        public bool DisplayPaymentMethodLogo { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.PayPalPlus.DisplayPaymentMethodDescription")]
        public bool DisplayPaymentMethodDescription { get; set; }
    }


    public class PayPalApiConfigValidator : SmartValidatorBase<ApiConfigurationModel>
    {
        public PayPalApiConfigValidator(Localizer T, Func<string, bool> addRule)
        {
            if (addRule("ClientId"))
            {
                RuleFor(x => x.ClientId).NotEmpty();
            }

            if (addRule("Secret"))
            {
                RuleFor(x => x.Secret).NotEmpty();
            }
        }
    }
}