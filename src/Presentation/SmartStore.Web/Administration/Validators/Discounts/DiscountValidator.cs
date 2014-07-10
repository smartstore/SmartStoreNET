using FluentValidation;
using SmartStore.Admin.Models.Discounts;
using SmartStore.Services.Localization;

namespace SmartStore.Admin.Validators.Discounts
{
	public partial class DiscountValidator : AbstractValidator<DiscountModel>
    {
        public DiscountValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name)
                .NotNull()
                .WithMessage(localizationService.GetResource("Admin.Promotions.Discounts.Fields.Name.Required"));
        }
    }
}