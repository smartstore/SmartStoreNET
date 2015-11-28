using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation.Attributes;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;
using SmartStore.Web.Validators.Common;

namespace SmartStore.Web.Models.Common
{
    [Validator(typeof(AddressValidator))]
    public partial class AddressModel : EntityModelBase
    {
        public AddressModel()
        {
            AvailableCountries = new List<SelectListItem>();
            AvailableStates = new List<SelectListItem>();
        }

        [SmartResourceDisplayName("Address.Fields.FirstName")]
        [AllowHtml]
        public string FirstName { get; set; }

        [SmartResourceDisplayName("Address.Fields.LastName")]
        [AllowHtml]
        public string LastName { get; set; }

        [SmartResourceDisplayName("Address.Fields.Email")]
        [AllowHtml]
        public string Email { get; set; }
        [SmartResourceDisplayName("Address.Fields.EmailMatch")]
        public string EmailMatch { get; set; }
        public bool ValidateEmailAddress { get; set; }

        [SmartResourceDisplayName("Address.Fields.Company")]
        [AllowHtml]
        public string Company { get; set; }
        public bool CompanyEnabled { get; set; }
        public bool CompanyRequired { get; set; }

        [SmartResourceDisplayName("Address.Fields.Country")]
        public int? CountryId { get; set; }

        [SmartResourceDisplayName("Address.Fields.Country")]
        [AllowHtml]
        public string CountryName { get; set; }
        public bool CountryEnabled { get; set; }

        [SmartResourceDisplayName("Address.Fields.StateProvince")]
        public int? StateProvinceId { get; set; }
        public bool StateProvinceEnabled { get; set; }

        [SmartResourceDisplayName("Address.Fields.StateProvince")]
        [AllowHtml]
        public string StateProvinceName { get; set; }

        [SmartResourceDisplayName("Address.Fields.City")]
        [AllowHtml]
        public string City { get; set; }
        public bool CityEnabled { get; set; }
        public bool CityRequired { get; set; }

        [SmartResourceDisplayName("Address.Fields.Address1")]
        [AllowHtml]
        public string Address1 { get; set; }
        public bool StreetAddressEnabled { get; set; }
        public bool StreetAddressRequired { get; set; }

        [SmartResourceDisplayName("Address.Fields.Address2")]
        [AllowHtml]
        public string Address2 { get; set; }
        public bool StreetAddress2Enabled { get; set; }
        public bool StreetAddress2Required { get; set; }

        [SmartResourceDisplayName("Address.Fields.ZipPostalCode")]
        [AllowHtml]
        public string ZipPostalCode { get; set; }
        public bool ZipPostalCodeEnabled { get; set; }
        public bool ZipPostalCodeRequired { get; set; }

        [SmartResourceDisplayName("Address.Fields.PhoneNumber")]
        [AllowHtml]
        public string PhoneNumber { get; set; }
        public bool PhoneEnabled { get; set; }
        public bool PhoneRequired { get; set; }

        [SmartResourceDisplayName("Address.Fields.FaxNumber")]
        [AllowHtml]
        public string FaxNumber { get; set; }
        public bool FaxEnabled { get; set; }
        public bool FaxRequired { get; set; }

        public IList<SelectListItem> AvailableCountries { get; set; }
        public IList<SelectListItem> AvailableStates { get; set; }
    }
}
