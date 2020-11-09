using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Localization;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.Validators;

namespace SmartStore.Web.Models.Customer
{
    [Validator(typeof(RegisterValidator))]
    public partial class RegisterModel : ModelBase
    {
        public RegisterModel()
        {
            AvailableTimeZones = new List<SelectListItem>();
            AvailableCountries = new List<SelectListItem>();
            AvailableStates = new List<SelectListItem>();
        }

        [SmartResourceDisplayName("Account.Fields.Email")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        public bool UsernamesEnabled { get; set; }

        [SmartResourceDisplayName("Account.Fields.Username")]
        public string Username { get; set; }

        public bool CheckUsernameAvailabilityEnabled { get; set; }

        [DataType(DataType.Password)]
        [SmartResourceDisplayName("Account.Fields.Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [SmartResourceDisplayName("Account.Fields.ConfirmPassword")]
        public string ConfirmPassword { get; set; }

        // Form fields & properties.
        public bool GenderEnabled { get; set; }

        [SmartResourceDisplayName("Account.Fields.Gender")]
        public string Gender { get; set; }

        public bool FirstNameRequired { get; set; }
        public bool LastNameRequired { get; set; }

        [SmartResourceDisplayName("Account.Fields.FirstName")]
        public string FirstName { get; set; }

        [SmartResourceDisplayName("Account.Fields.LastName")]
        public string LastName { get; set; }

        public bool DateOfBirthEnabled { get; set; }

        [SmartResourceDisplayName("Account.Fields.DateOfBirth")]
        public int? DateOfBirthDay { get; set; }

        [SmartResourceDisplayName("Account.Fields.DateOfBirth")]
        public int? DateOfBirthMonth { get; set; }

        [SmartResourceDisplayName("Account.Fields.DateOfBirth")]
        public int? DateOfBirthYear { get; set; }

        public bool CompanyEnabled { get; set; }
        public bool CompanyRequired { get; set; }

        [SmartResourceDisplayName("Account.Fields.Company")]
        public string Company { get; set; }

        public bool StreetAddressEnabled { get; set; }
        public bool StreetAddressRequired { get; set; }

        [SmartResourceDisplayName("Account.Fields.StreetAddress")]
        public string StreetAddress { get; set; }

        public bool StreetAddress2Enabled { get; set; }
        public bool StreetAddress2Required { get; set; }

        [SmartResourceDisplayName("Account.Fields.StreetAddress2")]
        public string StreetAddress2 { get; set; }

        public bool ZipPostalCodeEnabled { get; set; }
        public bool ZipPostalCodeRequired { get; set; }

        [SmartResourceDisplayName("Account.Fields.ZipPostalCode")]
        public string ZipPostalCode { get; set; }

        public bool CityEnabled { get; set; }
        public bool CityRequired { get; set; }

        [SmartResourceDisplayName("Account.Fields.City")]
        public string City { get; set; }

        public bool CountryEnabled { get; set; }

        [SmartResourceDisplayName("Account.Fields.Country")]
        public int CountryId { get; set; }
        public IList<SelectListItem> AvailableCountries { get; set; }

        public bool StateProvinceEnabled { get; set; }

        public bool StateProvinceRequired { get; set; }

        [SmartResourceDisplayName("Account.Fields.StateProvince")]
        public int? StateProvinceId { get; set; }
        public IList<SelectListItem> AvailableStates { get; set; }

        public bool PhoneEnabled { get; set; }
        public bool PhoneRequired { get; set; }

        [SmartResourceDisplayName("Account.Fields.Phone")]
        [DataType(DataType.PhoneNumber)]
        public string Phone { get; set; }

        public bool FaxEnabled { get; set; }
        public bool FaxRequired { get; set; }

        [SmartResourceDisplayName("Account.Fields.Fax")]
        [DataType(DataType.PhoneNumber)]
        public string Fax { get; set; }

        public bool NewsletterEnabled { get; set; }

        [SmartResourceDisplayName("Account.Fields.Newsletter")]
        public bool Newsletter { get; set; }

        // Time zone.
        [SmartResourceDisplayName("Account.Fields.TimeZone")]
        public string TimeZoneId { get; set; }
        public bool AllowCustomersToSetTimeZone { get; set; }
        public IList<SelectListItem> AvailableTimeZones { get; set; }

        // EU VAT.
        [SmartResourceDisplayName("Account.Fields.VatNumber")]
        public string VatNumber { get; set; }
        public string VatNumberStatusNote { get; set; }
        public bool DisplayVatNumber { get; set; }
        public bool VatRequired { get; set; }

        public bool DisplayCaptcha { get; set; }
    }

    public class RegisterValidator : AbstractValidator<RegisterModel>
    {
        public RegisterValidator(Localizer T, CustomerSettings customerSettings, TaxSettings taxSettings)
        {
            RuleFor(x => x.Email).NotEmpty();
            RuleFor(x => x.Email).EmailAddress();

            RuleFor(x => x.Password).Password(T, customerSettings);

            RuleFor(x => x.ConfirmPassword).NotEmpty();
            RuleFor(x => x.ConfirmPassword).Equal(x => x.Password).WithMessage(T("Account.Fields.Password.EnteredPasswordsDoNotMatch"));

            // Form fields.
            if (customerSettings.FirstNameRequired)
            {
                RuleFor(x => x.FirstName).NotEmpty();
            }
            if (customerSettings.LastNameRequired)
            {
                RuleFor(x => x.LastName).NotEmpty();
            }
            if (customerSettings.CompanyRequired && customerSettings.CompanyEnabled)
            {
                RuleFor(x => x.Company).NotEmpty();
            }
            if (customerSettings.StreetAddressRequired && customerSettings.StreetAddressEnabled)
            {
                RuleFor(x => x.StreetAddress).NotEmpty();
            }
            if (customerSettings.StreetAddress2Required && customerSettings.StreetAddress2Enabled)
            {
                RuleFor(x => x.StreetAddress2).NotEmpty();
            }
            if (customerSettings.ZipPostalCodeRequired && customerSettings.ZipPostalCodeEnabled)
            {
                RuleFor(x => x.ZipPostalCode).NotEmpty();
            }
            if (customerSettings.CityRequired && customerSettings.CityEnabled)
            {
                RuleFor(x => x.City).NotEmpty();
            }
            if (customerSettings.StateProvinceRequired && customerSettings.StateProvinceEnabled && customerSettings.CountryEnabled)
            {
                RuleFor(x => x.StateProvinceId)
                    .NotNull()
                    .NotEqual(0)
                    .WithMessage(T("Address.Fields.StateProvince.Required"));
            }
            if (customerSettings.PhoneRequired && customerSettings.PhoneEnabled)
            {
                RuleFor(x => x.Phone).NotEmpty();
            }
            if (customerSettings.FaxRequired && customerSettings.FaxEnabled)
            {
                RuleFor(x => x.Fax).NotEmpty();
            }
            if (taxSettings.EuVatEnabled && taxSettings.VatRequired)
            {
                RuleFor(x => x.VatNumber).NotEmpty();
            }
        }
    }
}