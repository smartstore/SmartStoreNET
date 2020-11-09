using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.ComponentModel;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Localization;
using SmartStore.Services.Common;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Common
{
    [Validator(typeof(AddressValidator))]
    public partial class AddressModel : EntityModelBase
    {
        public AddressModel()
        {
            AvailableCountries = new List<SelectListItem>();
            AvailableStates = new List<SelectListItem>();
        }

        [SmartResourceDisplayName("Address.Fields.Title")]
        public string Title { get; set; }

        [SmartResourceDisplayName("Admin.Address.Fields.FirstName")]
        [AllowHtml]
        public string FirstName { get; set; }

        [SmartResourceDisplayName("Admin.Address.Fields.LastName")]
        [AllowHtml]
        public string LastName { get; set; }

        [SmartResourceDisplayName("Admin.Address.Fields.Email")]
        [AllowHtml]
        public string Email { get; set; }

        [SmartResourceDisplayName("Admin.Address.Fields.EmailMatch")]
        [AllowHtml]
        public string EmailMatch { get; set; }

        [SmartResourceDisplayName("Admin.Address.Fields.Company")]
        [AllowHtml]
        public string Company { get; set; }

        [SmartResourceDisplayName("Admin.Address.Fields.Country")]
        public int? CountryId { get; set; }

        [SmartResourceDisplayName("Admin.Address.Fields.Country")]
        [AllowHtml]
        public string CountryName { get; set; }

        [SmartResourceDisplayName("Admin.Address.Fields.StateProvince")]
        public int? StateProvinceId { get; set; }

        [SmartResourceDisplayName("Admin.Address.Fields.StateProvince")]
        [AllowHtml]
        public string StateProvinceName { get; set; }

        [SmartResourceDisplayName("Admin.Address.Fields.City")]
        [AllowHtml]
        public string City { get; set; }

        [SmartResourceDisplayName("Admin.Address.Fields.Address1")]
        [AllowHtml]
        public string Address1 { get; set; }

        [SmartResourceDisplayName("Admin.Address.Fields.Address2")]
        [AllowHtml]
        public string Address2 { get; set; }

        [SmartResourceDisplayName("Admin.Address.Fields.ZipPostalCode")]
        [AllowHtml]
        public string ZipPostalCode { get; set; }

        [SmartResourceDisplayName("Admin.Address.Fields.PhoneNumber")]
        [AllowHtml]
        public string PhoneNumber { get; set; }

        [SmartResourceDisplayName("Admin.Address.Fields.FaxNumber")]
        [AllowHtml]
        public string FaxNumber { get; set; }

        public IList<SelectListItem> AvailableCountries { get; set; }
        public IList<SelectListItem> AvailableStates { get; set; }

        public string FormattedAddress { get; set; }

        public bool TitleEnabled { get; set; }
        public bool FirstNameEnabled { get; set; }
        public bool FirstNameRequired { get; set; }
        public bool LastNameEnabled { get; set; }
        public bool LastNameRequired { get; set; }
        public bool EmailEnabled { get; set; }
        public bool EmailRequired { get; set; }
        public bool ValidateEmailAddress { get; set; }
        public bool CompanyEnabled { get; set; }
        public bool CompanyRequired { get; set; }
        public bool CountryEnabled { get; set; }
        public bool StateProvinceEnabled { get; set; }
        public bool CityEnabled { get; set; }
        public bool CityRequired { get; set; }
        public bool StreetAddressEnabled { get; set; }
        public bool StreetAddressRequired { get; set; }
        public bool StreetAddress2Enabled { get; set; }
        public bool StreetAddress2Required { get; set; }
        public bool ZipPostalCodeEnabled { get; set; }
        public bool ZipPostalCodeRequired { get; set; }
        public bool PhoneEnabled { get; set; }
        public bool PhoneRequired { get; set; }
        public bool FaxEnabled { get; set; }
        public bool FaxRequired { get; set; }
    }

    public partial class AddressValidator : AbstractValidator<AddressModel>
    {
        public AddressValidator(Localizer T)
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .When(x => x.FirstNameEnabled && x.FirstNameRequired);

            RuleFor(x => x.LastName)
                .NotEmpty()
                .When(x => x.LastNameEnabled && x.LastNameRequired);

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .When(x => x.EmailEnabled && x.EmailRequired);

            RuleFor(x => x.Company)
                .NotEmpty()
                .When(x => x.CompanyEnabled && x.CompanyRequired);

            RuleFor(x => x.CountryId)
                .NotEmpty()
                .When(x => x.CountryEnabled);

            RuleFor(x => x.CountryId)
                .NotEqual(0)
                .WithMessage(T("Admin.Address.Fields.Country.Required"))
                .When(x => x.CountryEnabled);

            RuleFor(x => x.City)
                .NotEmpty()
                .When(x => x.CityEnabled && x.CityRequired);

            RuleFor(x => x.Address1)
                .NotEmpty()
                .When(x => x.StreetAddressEnabled && x.StreetAddressRequired);

            RuleFor(x => x.Address2)
                .NotEmpty()
                .When(x => x.StreetAddress2Enabled && x.StreetAddress2Required);

            RuleFor(x => x.ZipPostalCode)
                .NotEmpty()
                .When(x => x.ZipPostalCodeEnabled && x.ZipPostalCodeRequired);

            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .When(x => x.PhoneEnabled && x.PhoneRequired);

            RuleFor(x => x.FaxNumber)
                .NotEmpty()
                .When(x => x.FaxEnabled && x.FaxRequired);

            RuleFor(x => x.EmailMatch)
                .NotEmpty()
                .Equal(x => x.Email)
                .WithMessage(T("Admin.Address.Fields.EmailMatch.MustMatchEmail"))
                .When(x => x.ValidateEmailAddress);
        }
    }

    public class AddressMapper :
        IMapper<Address, AddressModel>
    {
        private readonly IAddressService _addressService;

        public AddressMapper(IAddressService addressService)
        {
            _addressService = addressService;
        }

        public void Map(Address from, AddressModel to)
        {
            MiniMapper.Map(from, to);
            to.CountryName = from.Country?.Name;
            to.StateProvinceName = from.StateProvince?.Name;
            to.EmailMatch = from.Email;
            to.FormattedAddress = _addressService.FormatAddress(from, true);
        }
    }
}