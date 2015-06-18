using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Core.Domain.Payments;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Admin.Models.Payments
{
	public class PaymentMethodEditModel : ProviderModel
	{
		public List<SelectListItem> AvailableCustomerRoles { get; set; }
		public List<SelectListItem> AvailableShippingMethods { get; set; }
		public List<SelectListItem> AvailableCountries { get; set; }
		public List<SelectListItem> AvailableAmountRestrictionContextTypes { get; set; }

		public CountryExclusionContextType CountryExclusionContext { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Payment.Methods.AmountRestrictionContext")]
		public AmountRestrictionContextType AmountRestrictionContext { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Payment.Methods.MinimumOrderAmount")]
		public decimal? MinimumOrderAmount { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Payment.Methods.MaximumOrderAmount")]
		public decimal? MaximumOrderAmount { get; set; }
	}
}