using System;
using SmartStore.Core;
using SmartStore.Core.Plugins;
using SmartStore.Services.Discounts;
using SmartStore.Services.Localization;

namespace SmartStore.DiscountRules
{
	[SystemName("DiscountRequirement.ShippingCountryIs")]
	[FriendlyName("Shipping country is")]
	[DisplayOrder(10)]
	public partial class ShippingCountryRule : DiscountRequirementRuleBase
    {
		public override bool CheckRequirement(CheckDiscountRequirementRequest request)
        {
			if (request == null)
				throw new ArgumentNullException("request");

			if (request.DiscountRequirement == null)
				throw new SmartException("Discount requirement is not set");

			if (request.Customer == null)
				return false;

			if (request.Customer.ShippingAddress == null)
				return false;

			if (request.DiscountRequirement.ShippingCountryId == 0)
				return false;

			bool result = request.Customer.ShippingAddress.CountryId == request.DiscountRequirement.ShippingCountryId;
			return result;
        }

		protected override string GetActionName()
        {
			return "ShippingCountry";
        }

	}
}