using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Core.Domain.Common;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Admin.Models.Payments
{
	public class PaymentMethodEditModel : ProviderModel
	{
		[SmartResourceDisplayName("Admin.Configuration.Payment.Methods.ExcludedCustomerRole")]
		public string[] ExcludedCustomerRoleIds { get; set; }
		public List<SelectListItem> AvailableCustomerRoles { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Payment.Methods.ExcludedShippingMethod")]
		public string[] ExcludedShippingMethodIds { get; set; }
		public List<SelectListItem> AvailableShippingMethods { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Payment.Methods.ExcludedCountry")]
		public string[] ExcludedCountryIds { get; set; }
		public List<SelectListItem> AvailableCountries { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Restrictions.CountryExclusionContext")]
		public CountryRestrictionContextType CountryExclusionContext { get; set; }
		public List<SelectListItem> AvailableCountryExclusionContextTypes { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Payment.Methods.MinimumOrderAmount")]
		public decimal? MinimumOrderAmount { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Payment.Methods.MaximumOrderAmount")]
		public decimal? MaximumOrderAmount { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Restrictions.AmountRestrictionContext")]
		public AmountRestrictionContextType AmountRestrictionContext { get; set; }
		public List<SelectListItem> AvailableAmountRestrictionContextTypes { get; set; }
	}
}