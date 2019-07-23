using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Rules.Cart.Impl
{
    public class LanguageRule : ListRuleBase<int>
    {
        protected override int GetValue(CartRuleContext context)
        {
            return context.WorkContext.WorkingLanguage.Id;
        }
    }
}
