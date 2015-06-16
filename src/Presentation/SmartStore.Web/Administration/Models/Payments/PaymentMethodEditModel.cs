using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Core.Domain.Payments;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Admin.Models.Payments
{
	public class PaymentMethodEditModel : ProviderModel
	{
		public List<SelectListItem> AvailableCustomerRoles { get; set; }
		public List<SelectListItem> AvailableShippingMethods { get; set; }
		public List<SelectListItem> AvailableCountries { get; set; }

		public CountryExclusionContextType CountryExclusionContext { get; set; }
	}
}