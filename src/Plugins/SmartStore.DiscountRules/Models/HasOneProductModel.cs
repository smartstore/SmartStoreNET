using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Web.Framework;

namespace SmartStore.DiscountRules.Models
{
	public class HasOneProductModel : DiscountRuleModelBase
    {
		[SmartResourceDisplayName("Plugins.DiscountRules.HasOneProduct.Fields.Products")]
		public string Products { get; set; }
    }
}