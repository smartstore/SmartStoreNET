using FluentValidation;
using SmartStore.Admin.Models.Customers;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Localization;

namespace SmartStore.Admin.Validators.Customers
{
	public partial class CustomerValidator : AbstractValidator<CustomerModel>
    {
        public CustomerValidator(Localizer T, CustomerSettings customerSettings)
        {
			var prefix = "Admin.Customers.Customers.Fields.";

			if (customerSettings.FirstNameRequired)
				RuleFor(x => x.FirstName).NotEmpty().WithMessage(T(prefix + "FirstName.Required"));

			if (customerSettings.LastNameRequired)
				RuleFor(x => x.LastName).NotEmpty().WithMessage(T(prefix + "LastName.Required"));

			if (customerSettings.CompanyRequired && customerSettings.CompanyEnabled)
                RuleFor(x => x.Company).NotEmpty().WithMessage(T(prefix + "Company.Required"));

            if (customerSettings.StreetAddressRequired && customerSettings.StreetAddressEnabled)
                RuleFor(x => x.StreetAddress).NotEmpty().WithMessage(T(prefix + "StreetAddress.Required"));

            if (customerSettings.StreetAddress2Required && customerSettings.StreetAddress2Enabled)
                RuleFor(x => x.StreetAddress2).NotEmpty().WithMessage(T(prefix + "StreetAddress2.Required"));

            if (customerSettings.ZipPostalCodeRequired && customerSettings.ZipPostalCodeEnabled)
                RuleFor(x => x.ZipPostalCode).NotEmpty().WithMessage(T(prefix + "ZipPostalCode.Required"));

            if (customerSettings.CityRequired && customerSettings.CityEnabled)
                RuleFor(x => x.City).NotEmpty().WithMessage(T(prefix + "City.Required"));

            if (customerSettings.PhoneRequired && customerSettings.PhoneEnabled)
                RuleFor(x => x.Phone).NotEmpty().WithMessage(T(prefix + "Phone.Required"));

            if (customerSettings.FaxRequired && customerSettings.FaxEnabled)
                RuleFor(x => x.Fax).NotEmpty().WithMessage(T(prefix + "Fax.Required"));
        }
    }
}