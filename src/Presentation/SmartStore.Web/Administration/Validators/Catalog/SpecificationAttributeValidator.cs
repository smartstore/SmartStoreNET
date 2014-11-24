using FluentValidation;
using SmartStore.Admin.Models.Catalog;
using SmartStore.Services.Localization;

namespace SmartStore.Admin.Validators.Catalog
{
	public partial class SpecificationAttributeValidator : AbstractValidator<SpecificationAttributeModel>
    {
        public SpecificationAttributeValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotNull().WithMessage(localizationService.GetResource("Admin.Catalog.Attributes.SpecificationAttributes.Fields.Name.Required"));
        }
    }
}