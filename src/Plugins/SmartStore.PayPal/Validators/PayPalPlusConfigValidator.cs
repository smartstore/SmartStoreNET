using System;
using FluentValidation;
using SmartStore.PayPal.Models;
using SmartStore.Services.Localization;
using SmartStore.Web.Framework.Validators;

namespace SmartStore.PayPal.Validators
{
	public class PayPalPlusConfigValidator : SmartValidatorBase<PayPalPlusConfigurationModel>
	{
		public PayPalPlusConfigValidator(ILocalizationService localize, Func<string, bool> addRule)
		{
			if (addRule("ClientId"))
			{
				RuleFor(x => x.ClientId).NotEmpty()
					.WithMessage(localize.GetResource("Plugins.SmartStore.PayPal.ValidateClientIdAndSecret"));
			}

			if (addRule("Secret"))
			{
				RuleFor(x => x.Secret).NotEmpty()
					.WithMessage(localize.GetResource("Plugins.SmartStore.PayPal.ValidateClientIdAndSecret"));
			}

			if (addRule("ThirdPartyPaymentMethods"))
			{
				RuleFor(x => x.ThirdPartyPaymentMethods)
					.Must(x => x == null || x.Count <= 5)
					.WithMessage(localize.GetResource("Plugins.Payments.PayPalPlus.ValidateThirdPartyPaymentMethods"));
			}
		}
	}
}