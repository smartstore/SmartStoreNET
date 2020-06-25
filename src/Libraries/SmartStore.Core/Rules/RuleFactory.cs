using System.Linq;
using Autofac;
using SmartStore.Rules.Domain;

namespace SmartStore.Rules
{
    public interface IRuleFactory
    {
        IRuleExpressionGroup CreateExpressionGroup(int ruleSetId, IRuleVisitor visitor, bool includeHidden = false);
        IRuleExpressionGroup CreateExpressionGroup(RuleSetEntity ruleSet, IRuleVisitor visitor, bool includeHidden = false);
    }

    public partial class RuleFactory : IRuleFactory
    {
        private readonly IRuleStorage _storage;

        public RuleFactory(IRuleStorage storage)
        {
            _storage = storage;
        }

        public IRuleExpressionGroup CreateExpressionGroup(int ruleSetId, IRuleVisitor visitor, bool includeHidden = false)
        {
            if (ruleSetId <= 0)
                return null;

            // TODO: prevent stack overflow > check if nested groups reference each other.

            var ruleSet = _storage.GetCachedRuleSet(ruleSetId);
            if (ruleSet == null)
            {
                // TODO: ErrHandling (???)
                return null;
            }

            return CreateExpressionGroup(ruleSet, visitor, includeHidden);
        }

        public virtual IRuleExpressionGroup CreateExpressionGroup(RuleSetEntity ruleSet, IRuleVisitor visitor, bool includeHidden = false)
        {
            if (ruleSet.Scope != visitor.Scope)
            {
                throw new SmartException($"Differing rule scope {ruleSet.Scope}. Expected {visitor.Scope}.");
            }

            if (!includeHidden && !ruleSet.IsActive)
            {
                return null;
            }

            var group = visitor.VisitRuleSet(ruleSet);

            var expressions = ruleSet.Rules
                .Select(x => CreateExpression(x, visitor))
                .Where(x => x != null)
                .ToArray();

            group.AddExpressions(expressions);

            return group;
        }

        //private IRuleExpressionGroup CreateExpressionGroup(RuleSetEntity ruleSet, RuleEntity refRule, IRuleVisitor visitor)
        //{
        //    if (ruleSet.Scope != visitor.Scope)
        //    {
        //        // TODO: ErrHandling (ruleSet is for a different scope)
        //        return null;
        //    }

        //    var group = visitor.VisitRuleSet(ruleSet);
        //    if (refRule != null)
        //    {
        //        group.RefRuleId = refRule.Id;
        //    }

        //    var expressions = ruleSet.Rules
        //        .Select(x => CreateExpression(x, visitor))
        //        .Where(x => x != null)
        //        .ToArray();

        //    group.AddExpressions(expressions);

        //    return group;
        //}

        private IRuleExpression CreateExpression(RuleEntity ruleEntity, IRuleVisitor visitor)
        {
            if (!ruleEntity.IsGroup)
            {
                return visitor.VisitRule(ruleEntity);
            }

            // It's a group, do recursive call.
            var group = CreateExpressionGroup(ruleEntity.Value.Convert<int>(), visitor);
            if (group != null)
            {
                group.RefRuleId = ruleEntity.Id;
            }

            return group;
        }
    }
}
