//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace SmartStore.Rules.Cart.Impl
//{
//    public class RuleRule : IRule
//    {
//        protected IRule GetOtherRuleSet(RuleExpression expression)
//        {
//            var ruleId = expression.Value.Convert<int>();

//            // TODO: get other rule from service
//            IRule rule = null;

//            return rule;
//        }

//        public bool Match(CartRuleContext context, RuleExpression expression)
//        {
//            var rule = GetOtherRuleSet(expression);
//            if (rule == null)
//                return false; // TBD: really?!

//            if (expression.Operator == RuleOperator.IsEqualTo)
//            {
//                return rule.Match(context);
//            }
//            if (expression.Operator == RuleOperator.IsNotEqualTo)
//            {
//                return !rule.Match(context);
//            }

//            //throw new InvalidRuleOperatorException(expression); // TODO
//        }
//    }
//}
