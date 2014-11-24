using FluentValidation;
using SmartStore.Admin.Models.Orders;
using SmartStore.Services.Localization;

namespace SmartStore.Admin.Validators.Orders
{
	public partial class CheckoutAttributeValidator : AbstractValidator<CheckoutAttributeModel>
    {
        public CheckoutAttributeValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotNull().WithMessage(localizationService.GetResource("Admin.Catalog.Attributes.CheckoutAttributes.Fields.Name.Required"));
        }
    }
}