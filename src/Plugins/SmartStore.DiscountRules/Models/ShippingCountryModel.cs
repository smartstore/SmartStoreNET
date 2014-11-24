using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Web.Framework;

namespace SmartStore.DiscountRules.Models
{
	public class ShippingCountryModel : DiscountRuleModelBase
    {
		public ShippingCountryModel()
        {
			AvailableCountries = new List<SelectListItem>();
        }
		[SmartResourceDisplayName("Plugins.DiscountRules.ShippingCountry.Fields.Country")]
		public int CountryId { get; set; }
		public IList<SelectListItem> AvailableCountries { get; set; }
    }
}