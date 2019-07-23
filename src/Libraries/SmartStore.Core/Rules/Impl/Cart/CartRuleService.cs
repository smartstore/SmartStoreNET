using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using SmartStore.Rules.Cart.Impl;
using SmartStore.Rules.Domain;

namespace SmartStore.Rules.Cart
{
    public class CartRuleService : RuleServiceBase
    {
        private readonly IComponentContext _componentContext;

        public CartRuleService(IComponentContext componentContext)
            : base(RuleScope.Cart)
        {
            _componentContext = componentContext;
        }

        public override IRuleExpression VisitRule(RuleEntity rule)
        {
            var expression = new RuleExpression();
            base.ConvertRule(rule, expression);
            return expression;
        }

        public override IRuleExpressionGroup VisitRuleSet(RuleSetEntity ruleSet)
        {
            var group = new RuleExpressionGroup
            {
                LogicalOperator = ruleSet.LogicalOperator,
                Value = ruleSet.Id,
                RawValue = ruleSet.Id.ToString(),
                Descriptor = new CartRuleDescriptor
                {
                    RuleType = RuleType.Boolean,
                    ProcessorType = typeof(CompositeRule)
                }
            };

            return group;
        }

        public bool RuleMatches(RuleExpression expression)
        {
            Guard.NotNull(expression, nameof(expression));

            var context = new CartRuleContext
            {
                Customer = null, // TODO
                Store = null, // TODO
                WorkContext = null // TODO
            };

            var processor = GetProcessor(expression);

            return processor.Match(context, expression);
        }

        public IRule GetProcessor(RuleExpression expression)
        {
            var descriptor = expression.Descriptor as CartRuleDescriptor;
            if (descriptor == null)
            {
                // TODO: ErrHandling
                throw new InvalidOperationException();
            }

            if (descriptor.ProcessorInstance == null)
            {
                var group = expression as RuleExpressionGroup;

                if (group == null && descriptor.ProcessorType != typeof(CompositeRule))
                {
                    // TODO: Autofac
                    descriptor.ProcessorInstance = (IRule)Activator.CreateInstance(descriptor.ProcessorType);
                }
                else
                {
                    var compositeRule = new CompositeRule(group, this);
                    descriptor.ProcessorInstance = compositeRule;
                }
            }

            return descriptor.ProcessorInstance;
        }

        protected override IEnumerable<RuleDescriptor> LoadDescriptors()
        {
            return new List<CartRuleDescriptor>
            {
                new CartRuleDescriptor
                {
                    Name = "CartTotal",
                    RuleType = RuleType.Money,
                    Constraints = new IRuleConstraint[0],
                    ProcessorType = typeof(CartTotalRule)
                },
                new CartRuleDescriptor
                {
                    Name = "Currency",
                    RuleType = RuleType.IntArray,
                    ProcessorType = typeof(CurrencyRule),
                    Constraints = new IRuleConstraint[0],
                    SelectList = new RemoteRuleValueSelectList("Currency") { Multiple = true }
                },
                new CartRuleDescriptor
                {
                    Name = "CustomerRole",
                    RuleType = RuleType.IntArray,
                    ProcessorType = typeof(CustomerRoleRule),
                    Constraints = new IRuleConstraint[0],
                    SelectList = new RemoteRuleValueSelectList("CustomerRole") { Multiple = true }
                },
                new CartRuleDescriptor
                {
                    Name = "Language",
                    RuleType = RuleType.IntArray,
                    ProcessorType = typeof(LanguageRule),
                    Constraints = new IRuleConstraint[0],
                    SelectList = new RemoteRuleValueSelectList("Language") { Multiple = true }
                },
                new CartRuleDescriptor
                {
                    Name = "Store",
                    RuleType = RuleType.IntArray,
                    ProcessorType = typeof(StoreRule),
                    Constraints = new IRuleConstraint[0],
                    SelectList = new RemoteRuleValueSelectList("Store") { Multiple = true }
                },
                //new CartRuleDescriptor
                //{
                //    Name = "Rule",
                //    RuleType = RuleType.Int,
                //    ProcessorType = typeof(RuleRule),
                //    Operators = new[] { RuleOperator.IsEqualTo, RuleOperator.IsNotEqualTo },
                //    Constraints = new IRuleConstraint[0],
                //    SelectList = new RemoteRuleValueSelectList("CartRule"),
                //}
            };
        }
    }
}
