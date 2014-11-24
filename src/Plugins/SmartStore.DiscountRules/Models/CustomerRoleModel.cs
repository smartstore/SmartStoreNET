using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Web.Framework;

namespace SmartStore.DiscountRules.Models
{
	public class CustomerRoleModel : DiscountRuleModelBase
    {
		public CustomerRoleModel()
        {
            AvailableCustomerRoles = new List<SelectListItem>();
        }
        [SmartResourceDisplayName("Plugins.DiscountRequirement.MustBeAssignedToCustomerRole.Fields.CustomerRole")]
        public int CustomerRoleId { get; set; }
        public IList<SelectListItem> AvailableCustomerRoles { get; set; }
    }
}