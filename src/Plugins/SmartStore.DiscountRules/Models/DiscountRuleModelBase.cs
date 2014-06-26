using System.Collections.Generic;
using System.Web.Mvc;

namespace SmartStore.DiscountRules.Models
{ 
	public abstract class DiscountRuleModelBase
    {
		public int DiscountId { get; set; }
		public int RequirementId { get; set; }
    }
}