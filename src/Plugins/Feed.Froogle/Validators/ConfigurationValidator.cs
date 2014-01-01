using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using SmartStore.Plugin.Feed.Froogle.Models;
using SmartStore.Services.Localization;

namespace SmartStore.Plugin.Feed.Froogle.Validators
{
	public class ConfigurationValidator : AbstractValidator<FeedFroogleModel>
	{
		public ConfigurationValidator(ILocalizationService localize)
        {
            RuleFor(x => x.ExpirationDays).InclusiveBetween(1, 29)
				.WithMessage(localize.GetResource("Plugins.Feed.Froogle.ExpirationDays.Validate"));
		}
	}
}
