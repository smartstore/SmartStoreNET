using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Models.Customers
{
    public class CustomerListModel : ModelBase
    {
		public CustomerListModel()
		{
			AvailableCustomerRoles = new List<SelectListItem>();
		}

		public GridModel<CustomerModel> Customers { get; set; }
		
        [SmartResourceDisplayName("Admin.Customers.Customers.List.CustomerRoles")]
        public string SearchCustomerRoleIds { get; set; }
		public IList<SelectListItem> AvailableCustomerRoles { get; set; }

		[SmartResourceDisplayName("Admin.Customers.Customers.List.SearchEmail")]
        [AllowHtml]
        public string SearchEmail { get; set; }

        [SmartResourceDisplayName("Admin.Customers.Customers.List.SearchUsername")]
        [AllowHtml]
        public string SearchUsername { get; set; }
        public bool UsernamesEnabled { get; set; }

		[SmartResourceDisplayName("Admin.Customers.Customers.List.SearchTerm")]
		[AllowHtml]
		public string SearchTerm { get; set; }


        [SmartResourceDisplayName("Admin.Customers.Customers.List.SearchDateOfBirth")]
        [AllowHtml]
        public string SearchDayOfBirth { get; set; }
        [SmartResourceDisplayName("Admin.Customers.Customers.List.SearchDateOfBirth")]
        [AllowHtml]
        public string SearchMonthOfBirth { get; set; }
        public bool DateOfBirthEnabled { get; set; }

        public bool CompanyEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Customers.Customers.List.SearchPhone")]
        [AllowHtml]
        public string SearchPhone { get; set; }
        public bool PhoneEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Customers.Customers.List.SearchZipCode")]
        [AllowHtml]
        public string SearchZipPostalCode { get; set; }
        public bool ZipPostalCodeEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Customers.Customers.List.SearchActiveOnly")]
        public bool? SearchActiveOnly { get; set; }
    }
}