using System;
using SmartStore.Core;
using SmartStore.Core.Plugins;
using SmartStore.Services.Discounts;

namespace SmartStore.DiscountRules
{
	public abstract class DiscountRequirementRuleBase : IDiscountRequirementRule
    {
		public abstract bool CheckRequirement(CheckDiscountRequirementRequest request);

        public string GetConfigurationUrl(int discountId, int? discountRequirementId)
        {
			string result = "Plugins/SmartStore.DiscountRules/DiscountRules/{0}?discountId={1}".FormatInvariant(GetActionName(), discountId);
            if (discountRequirementId.HasValue)
			{
                result += string.Format("&discountRequirementId={0}", discountRequirementId.Value);
			}
			return result;
        }

		protected abstract string GetActionName();
    }
}