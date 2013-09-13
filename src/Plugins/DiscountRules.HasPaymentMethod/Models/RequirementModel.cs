using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Web.Framework;

namespace SmartStore.Plugin.DiscountRules.HasPaymentMethod.Models
{
    public class RequirementModel
    {
		[SmartResourceDisplayName("Plugins.DiscountRules.HasPaymentMethod.Fields.PaymentMethods")]
		public string PaymentMethods { get; set; }
		public IList<SelectListItem> AvailablePaymentMethods { get; set; }

        public int DiscountId { get; set; }
        public int RequirementId { get; set; }
    }
}