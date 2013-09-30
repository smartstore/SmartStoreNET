using FluentValidation;
using SmartStore.Plugin.Widgets.TrustedShopsCustomerReviews.Models;
using SmartStore.Services.Localization;
using SmartStore.Web.Framework.Validators;

namespace SmartStore.Plugin.Widgets.TrustedShopsCustomerReviews.Validators
{
    public class TrustedShopsCustomerReviewsValidator : AbstractValidator<ConfigurationModel>
    {
        public TrustedShopsCustomerReviewsValidator(ILocalizationService localize)
        {
			RuleFor(x => x.TrustedShopsId).NotEmpty()
                .WithMessage(localize.GetResource("Plugins.Widgets.TrustedShopsCustomerReviews.MandatoryTrustedShopsId"));
        }
	}
}