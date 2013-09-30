using FluentValidation;
using SmartStore.Plugin.Widgets.TrustedShopsCustomerProtection.Models;
using SmartStore.Services.Localization;
using SmartStore.Web.Framework.Validators;

namespace SmartStore.Plugin.Widgets.TrustedShopsCustomerProtection.Validators
{
    public class TrustedShopsCustomerProtectionValidator : AbstractValidator<ConfigurationModel>
    {
        public TrustedShopsCustomerProtectionValidator(ILocalizationService localize)
        {
			RuleFor(x => x.TrustedShopsId).NotEmpty()
                .WithMessage(localize.GetResource("Plugins.Widgets.TrustedShopsCustomerProtection.MandatoryTrustedShopsId"));
        }
	}
}