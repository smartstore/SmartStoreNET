using FluentValidation;
using SmartStore.Admin.Models.Common;
using SmartStore.Services.Localization;

namespace SmartStore.Admin.Validators.Localization
{
	public partial class GenericAttributeValidator : AbstractValidator<GenericAttributeModel>
    {
        public GenericAttributeValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Key).NotEmpty().WithMessage(localizationService.GetResource("Admin.Common.GenericAttributes.Fields.Name.Required"));
        }
    }
}