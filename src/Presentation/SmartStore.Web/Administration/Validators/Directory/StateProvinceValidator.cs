using FluentValidation;
using SmartStore.Admin.Models.Directory;
using SmartStore.Services.Localization;

namespace SmartStore.Admin.Validators.Directory
{
	public partial class StateProvinceValidator : AbstractValidator<StateProvinceModel>
    {
        public StateProvinceValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name)
                .NotNull()
                .WithMessage(localizationService.GetResource("Admin.Configuration.Countries.States.Fields.Name.Required"));
        }
    }
}