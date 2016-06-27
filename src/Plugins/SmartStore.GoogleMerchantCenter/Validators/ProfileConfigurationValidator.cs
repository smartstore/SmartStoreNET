using FluentValidation;
using SmartStore.GoogleMerchantCenter.Models;
using SmartStore.Services.Localization;

namespace SmartStore.GoogleMerchantCenter.Validators
{
	public class ProfileConfigurationValidator : AbstractValidator<ProfileConfigurationModel>
	{
		public ProfileConfigurationValidator(ILocalizationService localize)
        {
            RuleFor(x => x.ExpirationDays).InclusiveBetween(0, 29)
				.WithMessage(localize.GetResource("Plugins.Feed.Froogle.ExpirationDays.Validate"));
		}
	}
}
