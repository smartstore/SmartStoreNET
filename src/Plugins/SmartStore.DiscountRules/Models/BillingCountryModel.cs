using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Web.Framework;

namespace SmartStore.DiscountRules.Models
{
	public class BillingCountryModel : DiscountRuleModelBase
    {
        public BillingCountryModel()
        {
			AvailableCountries = new List<SelectListItem>();
        }

		[SmartResourceDisplayName("Plugins.DiscountRules.BillingCountry.Fields.Country")]
		public int CountryId { get; set; }
		public IList<SelectListItem> AvailableCountries { get; set; }
    }
}