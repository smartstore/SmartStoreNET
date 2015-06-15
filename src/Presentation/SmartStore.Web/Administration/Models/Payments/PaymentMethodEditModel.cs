using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Admin.Models.Payments
{
	public class PaymentMethodEditModel : ProviderModel
	{
		public List<SelectListItem> AvailableCustomerRoles { get; set; }
		public List<SelectListItem> AvailableShippingMethods { get; set; }
		public List<SelectListItem> AvaliableCountries { get; set; }
	}
}