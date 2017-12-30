using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Web.Mvc;
using FluentValidation.Attributes;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;
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
            AvailableSalutations = new List<SelectListItem>();
        }

        [SmartResourceDisplayName("Address.Fields.Salutation")]
        public string Salutation { get; set; }
        public bool SalutationEnabled { get; set; }

        [SmartResourceDisplayName("Address.Fields.Title")]
        public string Title { get; set; }
        public bool TitleEnabled { get; set; }

        [SmartResourceDisplayName("Address.Fields.FirstName")]
        [AllowHtml]
        public string FirstName { get; set; }

        [SmartResourceDisplayName("Address.Fields.LastName")]
        [AllowHtml]
        public string LastName { get; set; }

        [SmartResourceDisplayName("Address.Fields.Email")]
        [AllowHtml]
		[DataType(DataType.EmailAddress)]
		public string Email { get; set; }

        [SmartResourceDisplayName("Address.Fields.EmailMatch")]
		[DataType(DataType.EmailAddress)]
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
		[DataType(DataType.PhoneNumber)]
		public string PhoneNumber { get; set; }
        public bool PhoneEnabled { get; set; }
        public bool PhoneRequired { get; set; }

        [SmartResourceDisplayName("Address.Fields.FaxNumber")]
        [AllowHtml]
		[DataType(DataType.PhoneNumber)]
		public string FaxNumber { get; set; }
        public bool FaxEnabled { get; set; }
        public bool FaxRequired { get; set; }

        public IList<SelectListItem> AvailableCountries { get; set; }
        public IList<SelectListItem> AvailableStates { get; set; }
        public IList<SelectListItem> AvailableSalutations { get; set; }

		public string FormattedAddress { get; set; }

		public string GetFormattedName()
		{
			var sb = new StringBuilder();

			sb.Append(FirstName);
			if (FirstName.HasValue() && LastName.HasValue())
			{
				sb.Append(" ");
			}
			sb.Append(LastName);

			return sb.ToString();
		}

		public string GetFormattedCityStateZip()
		{
			var sb = new StringBuilder();

			if (CityEnabled && City.HasValue())
			{
				sb.Append(City);
				if ((StateProvinceEnabled && StateProvinceName.HasValue()) || (ZipPostalCodeEnabled && ZipPostalCode.HasValue()))
				{
					sb.Append(", ");
				}
			}

			if (StateProvinceEnabled && StateProvinceName.HasValue())
			{
				sb.Append(StateProvinceName);
				if (ZipPostalCodeEnabled && ZipPostalCode.HasValue())
				{
					sb.Append(" ");
				}
			}

			if (ZipPostalCodeEnabled && ZipPostalCode.HasValue())
			{
				sb.Append(ZipPostalCode);
			}

			return sb.ToString();
		}
	}
}
