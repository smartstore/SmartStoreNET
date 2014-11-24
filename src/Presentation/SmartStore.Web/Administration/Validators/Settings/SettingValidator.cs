using FluentValidation;
using SmartStore.Admin.Models.Settings;
using SmartStore.Services.Localization;

namespace SmartStore.Admin.Validators.Settings
{
	public partial class SettingValidator : AbstractValidator<SettingModel>
    {
        public SettingValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotNull().WithMessage(localizationService.GetResource("Admin.Configuration.Settings.AllSettings.Fields.Name.Required"));
        }
    }
}