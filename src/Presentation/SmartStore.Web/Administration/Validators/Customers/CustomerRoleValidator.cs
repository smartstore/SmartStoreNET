using FluentValidation;
using SmartStore.Admin.Models.Customers;
using SmartStore.Services.Localization;

namespace SmartStore.Admin.Validators.Customers
{
	public partial class CustomerRoleValidator : AbstractValidator<CustomerRoleModel>
    {
        public CustomerRoleValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotNull().WithMessage(localizationService.GetResource("Admin.Customers.CustomerRoles.Fields.Name.Required"));
        }
    }
}