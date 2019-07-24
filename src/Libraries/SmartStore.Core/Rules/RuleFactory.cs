using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using SmartStore.Rules.Domain;
using SmartStore.Collections;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;

namespace SmartStore.Rules
{
    public interface IRuleFactory
    {
        IRuleExpressionGroup CreateExpressionGroup(int ruleSetId, IRuleVisitor visitor);
    }

    public partial class RuleFactory : IRuleFactory
    {
        internal const string RULESET_LOOKUP_KEY = "ruleset:lookup";

        private readonly IRuleStorage _storage;
        private readonly ICacheManager _cache;

        public RuleFactory(IRuleStorage storage, ICacheManager cache)
        {
            _storage = storage;
            _cache = cache;
        }

        public virtual IRuleExpressionGroup CreateExpressionGroup(int ruleSetId, IRuleVisitor visitor)
        {
            if (ruleSetId <= 0)
                return null;

            var ruleSet = GetRuleSetById(ruleSetId);
            if (ruleSet == null)
            {
                // TODO: ErrHandling (???)
                return null;
            }

            return CreateExpressionGroup(ruleSet, visitor);
        }

        protected internal IRuleExpressionGroup CreateExpressionGroup(RuleSetEntity ruleSet, IRuleVisitor visitor)
        {
            if (ruleSet.Scope != visitor.Scope)
            {
                // TODO: ErrHandling (ruleSet is for a different scope)
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

        private IRuleExpression CreateExpression(RuleEntity ruleEntity, IRuleVisitor visitor)
        {
            if (!ruleEntity.IsGroup)
            {
                return visitor.VisitRule(ruleEntity);
            }

            // It's a group, do recursive call
            return CreateExpressionGroup(ruleEntity.Value.Convert<int>(), visitor);
        }

        protected RuleSetEntity GetRuleSetById(int id)
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
            return _cache.Get(RULESET_LOOKUP_KEY, () => 
            {
                using (new DbContextScope(forceNoTracking: true, proxyCreation: false, lazyLoading: false))
                {
                    var allRuleSets = _storage.GetAllRuleSets(false, true, includeSubGroups: true).Load();
                    return allRuleSets.ToDictionary(k => k.Id);
                }
            });
        }
    }
}
