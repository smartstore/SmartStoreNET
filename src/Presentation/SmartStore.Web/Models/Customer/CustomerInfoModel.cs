using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.Core.Domain.Customers;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.Security;

namespace SmartStore.Web.Models.Customer
{
    [Validator(typeof(CustomerInfoValidator))]
    public partial class CustomerInfoModel : ModelBase
    {
        public CustomerInfoModel()
        {
            this.AvailableTimeZones = new List<SelectListItem>();
            this.AvailableCountries = new List<SelectListItem>();
            this.AvailableStates = new List<SelectListItem>();
            this.AssociatedExternalAuthRecords = new List<AssociatedExternalAuthModel>();
        }

        [SmartResourceDisplayName("Account.Fields.Email")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [SmartResourceDisplayName("Account.Fields.CustomerNumber")]
        public string CustomerNumber { get; set; }
        public bool CustomerNumberEnabled { get; set; }
        public bool DisplayCustomerNumber { get; set; }

        public bool CheckUsernameAvailabilityEnabled { get; set; }
        public bool AllowUsersToChangeUsernames { get; set; }
        public bool UsernamesEnabled { get; set; }

        [SmartResourceDisplayName("Account.Fields.Username")]
        public string Username { get; set; }

        //form fields & properties
        public bool GenderEnabled { get; set; }
        [SmartResourceDisplayName("Account.Fields.Gender")]
        public string Gender { get; set; }

        public bool TitleEnabled { get; set; }
        [SmartResourceDisplayName("Account.Fields.Title")]
        public string Title { get; set; }

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
        [SmartResourceDisplayName("Account.Fields.StateProvince")]
        public int StateProvinceId { get; set; }
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

        //preferences
        public bool SignatureEnabled { get; set; }
        [SmartResourceDisplayName("Account.Fields.Signature")]
        [SanitizeHtml]
        public string Signature { get; set; }

        //time zone
        [SmartResourceDisplayName("Account.Fields.TimeZone")]
        public string TimeZoneId { get; set; }
        public bool AllowCustomersToSetTimeZone { get; set; }
        public IList<SelectListItem> AvailableTimeZones { get; set; }

        //EU VAT
        [SmartResourceDisplayName("Account.Fields.VatNumber")]
        public string VatNumber { get; set; }
        public string VatNumberStatusNote { get; set; }
        public bool DisplayVatNumber { get; set; }

        //external authentication
        [SmartResourceDisplayName("Account.AssociatedExternalAuth")]
        public IList<AssociatedExternalAuthModel> AssociatedExternalAuthRecords { get; set; }

        #region Nested classes

        public partial class AssociatedExternalAuthModel : EntityModelBase
        {
            public string Email { get; set; }
            public string ExternalIdentifier { get; set; }
            public string AuthMethodName { get; set; }
        }

        #endregion
    }

    public class CustomerInfoValidator : AbstractValidator<CustomerInfoModel>
    {
        public CustomerInfoValidator(CustomerSettings customerSettings)
        {
            RuleFor(x => x.Email).NotEmpty();
            RuleFor(x => x.Email).EmailAddress();

            //form fields
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
            if (customerSettings.PhoneRequired && customerSettings.PhoneEnabled)
            {
                RuleFor(x => x.Phone).NotEmpty();
            }
            if (customerSettings.FaxRequired && customerSettings.FaxEnabled)
            {
                RuleFor(x => x.Fax).NotEmpty();
            }
        }
    }
}