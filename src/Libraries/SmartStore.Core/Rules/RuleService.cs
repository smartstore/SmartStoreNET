using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using SmartStore.Rules.Domain;
using SmartStore.Rules.Impl;

namespace SmartStore.Rules
{
    public partial class RuleService
    {
        private readonly IComponentContext _componentContext;

        public RuleService(IComponentContext componentContext)
        {
            _componentContext = componentContext;
        }

        public virtual CompositeRule GetRuleSet(int ruleSetId)
        {
            if (ruleSetId <= 0)
                return null;

            // TODO: get rule set from db
            var ruleSet = new RuleSet { LogicalOperator = CompositeRuleOperator.And };

            // TODO: Get all rules for requested set from db
            var ruleEntities = new List<Rule>
            {
                new Rule { RuleSetId = ruleSetId, RuleType = "Store", Operator = RuleOperators.In, Comparand = "1,2,3,4,5" },
                new Rule { RuleSetId = ruleSetId, RuleType = "Language", Operator = RuleOperators.NotIn, Comparand = "3" },
                new Rule { RuleSetId = ruleSetId, RuleType = "Currency", Operator = RuleOperators.In, Comparand = "1,2,3" },
                new Rule { RuleSetId = ruleSetId, RuleType = "Composite", Operator = RuleOperators.Equal, Comparand = "1,2,3" }, // This one is composite and contains other rules
            };

            var compositeRule = new CompositeRule { LogicalOperator = ruleSet.LogicalOperator };

            foreach (var entity in ruleEntities)
            {
                var rule = ActivateRuleInstance(entity);
                rule.Expression = new RuleExpression { Operator = entity.Operator, Comparand = entity.Comparand, TypeCode = rule.Metadata.TypeCode };
                compositeRule.AddRule(rule);
            }

            return compositeRule;
        }

        private IRule ActivateRuleInstance(Rule entity)
        {
            switch (entity.RuleType.ToLowerInvariant())
            {
                case "store":
                    return new StoreRule();
                case "customerrole":
                    return new CustomerRoleRule();
                case "currency":
                    return new CurrencyRule();
                case "language":
                    return new LanguageRule();
                case "rule":
                    return new RuleRule();
                case "composite":
                    return GetRuleSet(entity.Comparand.Convert<int>());
            }

            throw new InvalidOperationException();
        }
    }
}
