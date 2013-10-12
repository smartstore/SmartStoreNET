using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Configuration;

namespace SmartStore.Plugin.DiscountRules.HadSpentAmount
{
    
    public class RequirementSettings
    {
        public RequirementSettings()
        {
            BasketSubTotalIncludesDiscounts = true;
        }
        public bool LimitToCurrentBasketSubTotal { get; set; }
        public bool BasketSubTotalIncludesDiscounts { get; set; }
    }

}
