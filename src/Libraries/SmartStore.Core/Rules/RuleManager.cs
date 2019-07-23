using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using SmartStore.Rules.Domain;
using SmartStore.Rules.Cart.Impl;
using SmartStore.Collections;

namespace SmartStore.Rules
{
    public partial class RuleManager
    {
        public RuleManager()
        {
        }

        public virtual IRuleExpressionGroup GenerateRuleExpressionGroup(int ruleSetId, IRuleVisitor visitor)
        {
            if (ruleSetId <= 0)
                return null;

            var ruleSet = GetRuleSetById(ruleSetId);
            if (ruleSet == null)
            {
                // TODO: ErrHandling (???)
                return null;
            }

            return GenerateRuleExpressionGroup(ruleSet, visitor);
        }

        protected internal IRuleExpressionGroup GenerateRuleExpressionGroup(RuleSetEntity ruleSet, IRuleVisitor visitor)
        {
            if (ruleSet.Scope != visitor.Scope)
            {
                // TODO: ErrHandling (ruleSet is for a different scope)
                return null;
            }

            var group = visitor.VisitRuleSet(ruleSet);

            group.AddExpressions(ruleSet.Rules
                .Select(x => GenerateRuleExpression(x, visitor))
                .Where(x => x != null)
                .ToArray());

            return group;
        }

        private IRuleExpression GenerateRuleExpression(RuleEntity ruleEntity, IRuleVisitor visitor)
        {
            if (!ruleEntity.IsGroup)
            {
                return visitor.VisitRule(ruleEntity);
            }

            // It's a group, do recursive call
            return GenerateRuleExpressionGroup(ruleEntity.Value.Convert<int>(), visitor);
        }

        public RuleSetEntity GetRuleSetById(int id)
        {
            if (id <= 0)
                return null;

            var ruleSetLookup = GetRuleSetLookup();

            if (!ruleSetLookup.TryGetValue(id, out var ruleSet))
            {
                // TODO: ErrHandling (?)
                return null;
            }

            return ruleSet;
        }

        protected IDictionary<int, RuleSetEntity> GetRuleSetLookup()
        {
            var map = new Dictionary<int, RuleSetEntity>();

            // TODO: get rule set from db
            var ruleSet = new RuleSetEntity { LogicalOperator = LogicalRuleOperator.And };

            // TODO: Get all rules for requested set from db
            var ruleEntities = new List<RuleEntity>
            {
                new RuleEntity { RuleSetId = 1, RuleType = "CartTotal", Operator = RuleOperator.GreaterThanOrEqualTo, Value = "1000" },
                new RuleEntity { RuleSetId = 1, RuleType = "Store", Operator = RuleOperator.In, Value = "1,2,3,4,5" },
                new RuleEntity { RuleSetId = 1, RuleType = "Language", Operator = RuleOperator.NotIn, Value = "3" },
                new RuleEntity { RuleSetId = 1, RuleType = "Currency", Operator = RuleOperator.In, Value = "1,2,3" },
                // This one is composite and contains other rules. "Value" refers to RuleSetEntity.Id in this case.
                new RuleEntity { RuleSetId = 1, RuleType = "Group", Operator = RuleOperator.IsEqualTo, Value = "2" },
            };

            map.Add(1, ruleSet);

            return map;
        }

        //public virtual CompositeRule GetRuleSet(int ruleSetId)
        //{
        //    if (ruleSetId <= 0)
        //        return null;

        //    // TODO: get rule set from db
        //    var ruleSet = new RuleSetEntity { LogicalOperator = LogicalRuleOperator.And };

        //    // TODO: Get all rules for requested set from db
        //    var ruleEntities = new List<RuleEntity>
        //    {
        //        new RuleEntity { RuleSetId = ruleSetId, RuleType = "Store", Operator = RuleOperator.In, Value = "1,2,3,4,5" },
        //        new RuleEntity { RuleSetId = ruleSetId, RuleType = "Language", Operator = RuleOperator.NotIn, Value = "3" },
        //        new RuleEntity { RuleSetId = ruleSetId, RuleType = "Currency", Operator = RuleOperator.In, Value = "1,2,3" },
        //        new RuleEntity { RuleSetId = ruleSetId, RuleType = "Composite", Operator = RuleOperator.IsEqualTo, Value = "1,2,3" }, // This one is composite and contains other rules
        //    };

        //    var compositeRule = new CompositeRule { LogicalOperator = ruleSet.LogicalOperator };

        //    foreach (var entity in ruleEntities)
        //    {
        //        var rule = ActivateRuleInstance(entity);
        //        rule.Expression = new RuleExpression { Operator = entity.Operator, Value = entity.Value, Descriptor = rule.Descriptor };
        //        compositeRule.AddRule(rule);
        //    }

        //    return compositeRule;
        //}

        //private IRule ActivateRuleInstance(RuleEntity entity)
        //{
        //    switch (entity.RuleType.ToLowerInvariant())
        //    {
        //        case "store":
        //            return new StoreRule();
        //        case "customerrole":
        //            return new CustomerRoleRule();
        //        case "currency":
        //            return new CurrencyRule();
        //        case "language":
        //            return new LanguageRule();
        //        //case "rule":
        //        //    return new RuleRule();
        //        case "composite":
        //            return GetRuleSet(entity.Value.Convert<int>());
        //    }

        //    throw new InvalidOperationException();
        //}
    }
}
