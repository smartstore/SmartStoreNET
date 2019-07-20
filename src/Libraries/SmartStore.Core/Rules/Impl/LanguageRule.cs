using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Rules.Impl
{
    [Rule("Currency", FriendlyName = "Currency", Scope = RuleScope.Cart, DisplayOrder = 0)]
    public class LanguageRule : ListRuleBase<int>
    {
        protected override int GetValue(RuleContext context)
        {
            return context.WorkContext.WorkingLanguage.Id;
        }

        protected override RuleDescriptor GetRuleDescriptor()
        {
            return new RuleDescriptor
            {
                RuleType = RuleType.IntArray,
                SelectList = new RemoteRuleValueSelectList("Language") { Multiple = true },
                Constraints = new IRuleConstraint[0]
            };
        }
    }
}
