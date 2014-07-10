using FluentValidation;
using SmartStore.Admin.Models.Catalog;
using SmartStore.Services.Localization;

namespace SmartStore.Admin.Validators.Catalog
{
	public partial class ManufacturerValidator : AbstractValidator<ManufacturerModel>
    {
        public ManufacturerValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotNull().WithMessage(localizationService.GetResource("Admin.Catalog.Manufacturers.Fields.Name.Required"));
        }
    }
}