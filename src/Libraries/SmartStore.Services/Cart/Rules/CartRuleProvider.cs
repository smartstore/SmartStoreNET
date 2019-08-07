using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using SmartStore.Rules;
using SmartStore.Rules.Domain;
using SmartStore.Services.Cart.Rules.Impl;

namespace SmartStore.Services.Cart.Rules
{
    public interface ICartRuleProvider : IRuleProvider
    {
        IRule GetProcessor(RuleExpression expression);
        RuleExpressionGroup CreateExpressionGroup(int ruleSetId);

        bool RuleMatches(RuleExpression expression);
        bool RuleMatches(int[] ruleSetIds, LogicalRuleOperator logicalOperator);
        bool RuleMatches(RuleExpression[] expressions, LogicalRuleOperator logicalOperator);
    }

    public class CartRuleProvider : RuleProviderBase, ICartRuleProvider
    {
        private readonly IRuleFactory _ruleFactory;
        private readonly IComponentContext _componentContext;
        private readonly ICommonServices _services;

        public CartRuleProvider(IRuleFactory ruleFactory, IComponentContext componentContext, ICommonServices services)
            : base(RuleScope.Cart)
        {
            _ruleFactory = ruleFactory;
            _componentContext = componentContext;
            _services = services;
        }

        public RuleExpressionGroup CreateExpressionGroup(int ruleSetId)
        {
            return _ruleFactory.CreateExpressionGroup(ruleSetId, this) as RuleExpressionGroup;
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
                Id = ruleSet.Id,
                LogicalOperator = ruleSet.LogicalOperator,
                IsSubGroup = ruleSet.IsSubGroup,
                Value = ruleSet.Id,
                RawValue = ruleSet.Id.ToString(),
                Provider = this,
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

            return RuleMatches(new[] { expression }, LogicalRuleOperator.And);
        }

        public bool RuleMatches(int[] ruleSetIds, LogicalRuleOperator logicalOperator)
        {
            Guard.NotNull(ruleSetIds, nameof(ruleSetIds));

            if (ruleSetIds.Length == 0)
                return true;

            var expressions = ruleSetIds
                .Select(id => _ruleFactory.CreateExpressionGroup(id, this))
                .Cast<RuleExpression>()
                .ToArray();

            return RuleMatches(expressions, logicalOperator);
        }

        public bool RuleMatches(RuleExpression[] expressions, LogicalRuleOperator logicalOperator)
        {
            Guard.NotNull(expressions, nameof(expressions));

            if (expressions.Length == 0)
            {
                return true;
            }

            RuleExpressionGroup group = null;

            if (expressions.Length == 1 && expressions[0] is RuleExpressionGroup group2)
            {
                group = group2;
            }
            else
            {
                group = new RuleExpressionGroup() { LogicalOperator = logicalOperator };
                group.AddExpressions(expressions);
            }

            var context = new CartRuleContext
            {
                Customer = _services.WorkContext.CurrentCustomer,
                Store = _services.StoreContext.CurrentStore,
                WorkContext = _services.WorkContext
            };

            var processor = GetProcessor(group);

            return processor.Match(context, group);
        }

        public IRule GetProcessor(RuleExpression expression)
        {
            var descriptor = expression.Descriptor as CartRuleDescriptor;
            if (descriptor == null)
            {
                // TODO: ErrHandling
                throw new InvalidOperationException();
            }

            IRule instance;
            var group = expression as RuleExpressionGroup;

            if (group == null && descriptor.ProcessorType != typeof(CompositeRule))
            {
                instance = _componentContext.ResolveKeyed<IRule>(descriptor.ProcessorType);
            }
            else
            {
                instance = new CompositeRule(group, this);
            }

            return instance;
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
                new CartRuleDescriptor
                {
                    Name = "IPCountry",
                    RuleType = RuleType.StringArray,
                    ProcessorType = typeof(IPCountryRule),
                    Constraints = new IRuleConstraint[0],
                    SelectList = new RemoteRuleValueSelectList("Country") { Multiple = true }
                },
                new CartRuleDescriptor
                {
                    Name = "RuleSet",
                    RuleType = RuleType.Int,
                    ProcessorType = typeof(RuleSetRule),
                    Operators = new[] { RuleOperator.IsEqualTo, RuleOperator.IsNotEqualTo },
                    Constraints = new IRuleConstraint[0],
                    SelectList = new RemoteRuleValueSelectList("CartRule"),
                }
            };
        }
    }
}
