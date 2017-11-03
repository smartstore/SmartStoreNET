using FluentValidation;
using SmartStore.Admin.Models.Catalog;
using SmartStore.Services.Localization;

namespace SmartStore.Admin.Validators.Catalog
{
    public partial class CopyProductValidator : AbstractValidator<CopyProductModel>
    {
        public CopyProductValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.NumberOfCopies)
                .NotEmpty().WithMessage(localizationService.GetResource("Admin.Validation.ValueGreaterZero"))
                .GreaterThan(0).WithMessage(localizationService.GetResource("Admin.Validation.ValueGreaterZero"));
        }
    }
}