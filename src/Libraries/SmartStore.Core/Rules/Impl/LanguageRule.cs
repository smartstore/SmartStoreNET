using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Rules.Impl
{
    [Rule("Currency", FriendlyName = "Currency", Scope = RuleScope.Cart, DisplayOrder = 0)]
    public class LanguageRule : ArrayRuleBase<int>
    {
        protected override int GetValue(RuleContext context)
        {
            return context.WorkContext.WorkingLanguage.Id;
        }

        protected override RuleMetadata GetRuleMetadata()
        {
            return new RuleMetadata
            {
                TypeCode = RuleTypeCode.IntArray,
                Operators = RuleOperators.ArrayOperators,
                Editor = "Language",
                Constraints = new IRuleConstraint[0]
            };
        }
    }
}
