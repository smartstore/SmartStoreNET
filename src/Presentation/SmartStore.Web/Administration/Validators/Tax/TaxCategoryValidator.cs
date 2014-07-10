using FluentValidation;
using SmartStore.Admin.Models.Tax;
using SmartStore.Services.Localization;

namespace SmartStore.Admin.Validators.Tax
{
	public partial class TaxCategoryValidator : AbstractValidator<TaxCategoryModel>
    {
        public TaxCategoryValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotNull().WithMessage(localizationService.GetResource("Admin.Configuration.Tax.Categories.Fields.Name.Required"));
        }
    }
}