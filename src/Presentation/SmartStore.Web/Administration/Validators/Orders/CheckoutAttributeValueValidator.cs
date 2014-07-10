using FluentValidation;
using SmartStore.Admin.Models.Orders;
using SmartStore.Services.Localization;

namespace SmartStore.Admin.Validators.Orders
{
	public partial class CheckoutAttributeValueValidator : AbstractValidator<CheckoutAttributeValueModel>
    {
        public CheckoutAttributeValueValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotNull().WithMessage(localizationService.GetResource("Admin.Catalog.Attributes.CheckoutAttributes.Values.Fields.Name.Required"));
        }
    }
}