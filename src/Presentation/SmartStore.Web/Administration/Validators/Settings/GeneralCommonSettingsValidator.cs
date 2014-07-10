using FluentValidation;
using SmartStore.Admin.Models.Settings;
using SmartStore.Services.Localization;

namespace SmartStore.Admin.Validators.Settings
{
	public partial class GeneralCommonSettingsValidator : AbstractValidator<GeneralCommonSettingsModel>
    {
        public GeneralCommonSettingsValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.ContactDataSettings.CompanyEmailAddress)
                .EmailAddress()
                .WithMessage(localizationService.GetResource("Admin.Common.WrongEmail"));
            RuleFor(x => x.ContactDataSettings.ContactEmailAddress)
                .EmailAddress()
                .WithMessage(localizationService.GetResource("Admin.Common.WrongEmail"));
            RuleFor(x => x.ContactDataSettings.SupportEmailAddress)
                .EmailAddress()
                .WithMessage(localizationService.GetResource("Admin.Common.WrongEmail"));
            RuleFor(x => x.ContactDataSettings.WebmasterEmailAddress)
                .EmailAddress()
                .WithMessage(localizationService.GetResource("Admin.Common.WrongEmail"));
        }
    }
}