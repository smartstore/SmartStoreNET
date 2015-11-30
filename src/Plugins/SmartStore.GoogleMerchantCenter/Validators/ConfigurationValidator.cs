using FluentValidation;
using SmartStore.GoogleMerchantCenter.Models;
using SmartStore.Services.Localization;

namespace SmartStore.GoogleMerchantCenter.Validators
{
	public class ConfigurationValidator : AbstractValidator<FeedFroogleModel>
	{
		public ConfigurationValidator(ILocalizationService localize)
        {
            RuleFor(x => x.ExpirationDays).InclusiveBetween(0, 29)
				.WithMessage(localize.GetResource("Plugins.Feed.Froogle.ExpirationDays.Validate"));
		}
	}
}
