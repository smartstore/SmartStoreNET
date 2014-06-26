using System;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Plugins;
using SmartStore.Services.Discounts;
using SmartStore.Services.Localization;
using SmartStore.Services.Configuration;

namespace SmartStore.DiscountRules
{
	[SystemName("DiscountRequirement.MustBeAssignedToCustomerRole")]
	[FriendlyName("Must be assigned to customer role")]
	[DisplayOrder(0)]
	public partial class CustomerRoleRule : DiscountRequirementRuleBase
    {
		public override bool CheckRequirement(CheckDiscountRequirementRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            if (request.DiscountRequirement == null)
                throw new SmartException("Discount requirement is not set");

            if (request.Customer == null)
                return false;

            if (!request.DiscountRequirement.RestrictedToCustomerRoleId.HasValue)
                return false;

            foreach (var customerRole in request.Customer.CustomerRoles.Where(cr => cr.Active).ToList())
                if (request.DiscountRequirement.RestrictedToCustomerRoleId == customerRole.Id)
                    return true;

            return false;
        }

		protected override string GetActionName()
		{
			return "CustomerRole";
		}

	}
}