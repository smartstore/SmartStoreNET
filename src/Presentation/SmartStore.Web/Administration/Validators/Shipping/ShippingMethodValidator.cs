using FluentValidation;
using SmartStore.Admin.Models.Shipping;
using SmartStore.Services.Localization;

namespace SmartStore.Admin.Validators.Shipping
{
	public partial class ShippingMethodValidator : AbstractValidator<ShippingMethodModel>
    {
        public ShippingMethodValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotNull().WithMessage(localizationService.GetResource("Admin.Configuration.Shipping.Methods.Fields.Name.Required"));
        }
    }
}