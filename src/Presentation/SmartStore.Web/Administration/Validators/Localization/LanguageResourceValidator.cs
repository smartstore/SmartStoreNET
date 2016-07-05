using FluentValidation;
using SmartStore.Admin.Models.Localization;
using SmartStore.Services.Localization;

namespace SmartStore.Admin.Validators.Localization
{
	public partial class LanguageResourceValidator : AbstractValidator<LanguageResourceModel>
    {
        public LanguageResourceValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.ResourceName).NotNull().WithMessage(localizationService.GetResource("Admin.Configuration.Languages.Resources.Fields.Name.Required"));
            RuleFor(x => x.ResourceValue).NotNull().WithMessage(localizationService.GetResource("Admin.Configuration.Languages.Resources.Fields.Value.Required"));
        }
    }
}