using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartStore.DiscountRules.Settings
{
	public class HadSpentAmountSettings
	{
		public HadSpentAmountSettings()
        {
            BasketSubTotalIncludesDiscounts = true;
        }
		public bool LimitToCurrentBasketSubTotal { get; set; }
		public bool BasketSubTotalIncludesDiscounts { get; set; }
	}
}