using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation.Attributes;
using SmartStore.Admin.Validators.Customers;
using SmartStore.Core.Domain.Tax;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Customers
{
    [Validator(typeof(CustomerRoleValidator))]
    public class CustomerRoleModel : EntityModelBase
    {
        public CustomerRoleModel()
        {
            TaxDisplayTypes = new List<SelectListItem>();
        }

        [SmartResourceDisplayName("Admin.Customers.CustomerRoles.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [SmartResourceDisplayName("Admin.Customers.CustomerRoles.Fields.FreeShipping")]
        [AllowHtml]
        public bool FreeShipping { get; set; }

        [SmartResourceDisplayName("Admin.Customers.CustomerRoles.Fields.TaxExempt")]
        public bool TaxExempt { get; set; }

        [SmartResourceDisplayName("Admin.Customers.CustomerRoles.Fields.TaxDisplayType")]
        public int? TaxDisplayType { get; set; }
        public List<SelectListItem> TaxDisplayTypes { get; set; }

        [SmartResourceDisplayName("Admin.Customers.CustomerRoles.Fields.Active")]
        public bool Active { get; set; }

        [SmartResourceDisplayName("Admin.Customers.CustomerRoles.Fields.IsSystemRole")]
        public bool IsSystemRole { get; set; }

        [SmartResourceDisplayName("Admin.Customers.CustomerRoles.Fields.SystemName")]
        public string SystemName { get; set; }
    }
}