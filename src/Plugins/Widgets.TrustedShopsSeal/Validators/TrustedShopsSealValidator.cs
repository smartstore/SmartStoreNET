using FluentValidation;
using SmartStore.Plugin.Widgets.TrustedShopsSeal.Models;
using SmartStore.Services.Localization;
using SmartStore.Web.Framework.Validators;

namespace SmartStore.Plugin.Widgets.TrustedShopsSeal.Validators
{
    public class TrustedShopsSealValidator : AbstractValidator<ConfigurationModel>
    {
        public TrustedShopsSealValidator(ILocalizationService localize)
        {
			//RuleFor(x => x.TrustedShopsId).NotEmpty()
			//	.WithMessage(localize.GetResource("Plugins.Widgets.TrustedShopsSeal.MandatoryTrustedShopsId"));
        }
	}
}