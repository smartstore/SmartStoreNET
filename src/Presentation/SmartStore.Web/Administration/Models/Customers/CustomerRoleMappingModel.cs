using System;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Customers
{
    public class CustomerRoleMappingModel : EntityModelBase
    {
        public int CustomerId { get; set; }

        [SmartResourceDisplayName("Admin.Customers.Customers.Fields.Active")]
        public bool Active { get; set; }

        [SmartResourceDisplayName("Admin.Customers.Customers.Fields.Username")]
        public string Username { get; set; }

        [SmartResourceDisplayName("Admin.Customers.Customers.Fields.Email")]
        public string Email { get; set; }

        [SmartResourceDisplayName("Admin.Customers.Customers.Fields.FullName")]
        public string FullName { get; set; }

        [SmartResourceDisplayName("Common.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [SmartResourceDisplayName("Admin.Customers.Customers.Fields.LastActivityDate")]
        public DateTime LastActivityDate { get; set; }

        [SmartResourceDisplayName("Admin.Rules.AddedByRule")]
        public bool IsSystemMapping { get; set; }
    }
}