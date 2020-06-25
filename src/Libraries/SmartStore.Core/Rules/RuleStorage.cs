using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Rules.Domain;

namespace SmartStore.Rules
{
    public interface IRuleStorage
    {
        RuleSetEntity GetCachedRuleSet(int id);

        RuleSetEntity GetRuleSetById(int id, bool forEdit, bool withRules);
        IList<RuleSetEntity> GetRuleSetsByIds(int[] ids, bool withRules);
        IPagedList<RuleSetEntity> GetAllRuleSets(
            bool forEdit,
            bool withRules,
            RuleScope? scope = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            bool includeSubGroups = false,
            bool includeHidden = false);

        bool ApplyRuleSetMappings<T>(T entity, int[] selectedRuleSetIds) where T : BaseEntity, IRulesContainer;

        void InsertRuleSet(RuleSetEntity ruleSet);
        void UpdateRuleSet(RuleSetEntity ruleSet);
        void DeleteRuleSet(RuleSetEntity ruleSet);

        RuleEntity GetRuleById(int id, bool forEdit);
        IList<RuleEntity> GetRulesByIds(int[] ids, bool forEdit);

        void InsertRule(RuleEntity rule);
        void UpdateRule(RuleEntity rule);
        void DeleteRule(RuleEntity rule);
    }

    public partial class RuleStorage : IRuleStorage
    {
        internal const string RULESET_BY_ID_KEY = "ruleset:id-{0}";

        private readonly ICacheManager _cache;
        private readonly IRepository<RuleSetEntity> _rsRuleSets;
        private readonly IRepository<RuleEntity> _rsRules;

        public RuleStorage(ICacheManager cache, IRepository<RuleSetEntity> rsRuleSets, IRepository<RuleEntity> rsRules)
        {
            _cache = cache;
            _rsRuleSets = rsRuleSets;
            _rsRules = rsRules;
        }

        #region Read Rule(Set)s

        public RuleSetEntity GetCachedRuleSet(int id)
        {
            if (id <= 0)
                return null;

            //return this.GetRuleSetById(id, false, true);

            var cacheKey = RULESET_BY_ID_KEY.FormatInvariant(id);
            return _cache.Get(cacheKey, () =>
            {
                using (new DbContextScope(forceNoTracking: true, proxyCreation: false, lazyLoading: false))
                {
                    var ruleSet = this.GetRuleSetById(id, false, true);

                    if (ruleSet != null)
                    {
                        _rsRuleSets.Context.DetachEntity(ruleSet, true);
                        //// TODO: check if the above detach call already detached navigation prop instances
                        //// TODO: Sort rules (?)
                        //_rsRuleSets.Context.DetachEntities(ruleSet.Rules);
                    }

                    return ruleSet;
                }
            }, TimeSpan.FromHours(8));
        }

        protected void InvalidateRuleSet(int id)
        {
            if (id > 0)
                _cache.Remove(RULESET_BY_ID_KEY.FormatInvariant(id));
        }

        public RuleSetEntity GetRuleSetById(int id, bool forEdit, bool withRules)
        {
            if (id <= 0)
                return null;

            var table = forEdit
                ? _rsRuleSets.Table
                : _rsRuleSets.TableUntracked;

            if (withRules)
            {
                table = table.Include(x => x.Rules);
            }

            return table.FirstOrDefault(x => x.Id == id);
        }

        public IList<RuleSetEntity> GetRuleSetsByIds(int[] ids, bool withRules)
        {
            if (ids == null || !ids.Any())
            {
                return new List<RuleSetEntity>();
            }

            var query = _rsRuleSets.TableUntracked;
            if (withRules)
            {
                query = query.Include(x => x.Rules);
            }

            var ruleSets = query.ToList();
            return ruleSets.OrderBySequence(ids).ToList();
        }

        public IPagedList<RuleSetEntity> GetAllRuleSets(
            bool forEdit,
            bool withRules,
            RuleScope? scope = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            bool includeSubGroups = false,
            bool includeHidden = false)
        {
            var query = forEdit
                ? _rsRuleSets.Table
                : _rsRuleSets.TableUntracked;

            if (withRules)
            {
                query = query.Include(x => x.Rules);
            }

            if (!includeHidden)
            {
                query = query.Where(x => x.IsActive);
            }

            if (scope != null)
            {
                query = query.Where(x => x.Scope == scope.Value);
            }

            if (!includeSubGroups)
            {
                query = query.Where(x => !x.IsSubGroup);
            }

            query = query
                .OrderByDescending(x => x.IsActive)
                .ThenBy(x => x.Scope);

            return new PagedList<RuleSetEntity>(query, pageIndex, pageSize);
        }

        public RuleEntity GetRuleById(int id, bool forEdit)
        {
            if (id <= 0)
                return null;

            var table = forEdit
                ? _rsRules.Table
                : _rsRules.TableUntracked;

            return table
                .Include(x => x.RuleSet)
                .FirstOrDefault(x => x.Id == id);
        }

        public IList<RuleEntity> GetRulesByIds(int[] ids, bool forEdit)
        {
            if (ids == null || !ids.Any())
            {
                return new List<RuleEntity>();
            }

            var table = forEdit
                ? _rsRules.Table
                : _rsRules.TableUntracked;

            var entities = table.Include(x => x.RuleSet)
                .Where(x => ids.Contains(x.Id))
                .ToList();

            return entities;
        }

        public virtual bool ApplyRuleSetMappings<T>(T entity, int[] selectedRuleSetIds) where T : BaseEntity, IRulesContainer
        {
            Guard.NotNull(entity, nameof(entity));

            var updated = false;
            var allRuleSets = GetAllRuleSets(true, false, includeHidden: true).ToDictionary(x => x.Id);

            foreach (var ruleSetId in allRuleSets.Keys)
            {
                if (selectedRuleSetIds?.Contains(ruleSetId) ?? false)
                {
                    if (!entity.RuleSets.Any(x => x.Id == ruleSetId))
                    {
                        entity.RuleSets.Add(allRuleSets[ruleSetId]);
                        updated = true;
                    }
                }
                else if (entity.RuleSets.Any(x => x.Id == ruleSetId))
                {
                    entity.RuleSets.Remove(allRuleSets[ruleSetId]);
                    updated = false;
                }
            }

            return updated;
        }

        #endregion

        #region Modify RuleSets

        public void InsertRuleSet(RuleSetEntity ruleSet)
        {
            Guard.NotNull(ruleSet, nameof(ruleSet));

            _rsRuleSets.Insert(ruleSet);
        }

        public void UpdateRuleSet(RuleSetEntity ruleSet)
        {
            Guard.NotNull(ruleSet, nameof(ruleSet));

            _rsRuleSets.Update(ruleSet);
            InvalidateRuleSet(ruleSet.Id);
        }

        public void DeleteRuleSet(RuleSetEntity ruleSet)
        {
            Guard.NotNull(ruleSet, nameof(ruleSet));

            _rsRuleSets.Delete(ruleSet);
            InvalidateRuleSet(ruleSet.Id);
        }

        #endregion

        #region Modify Rules

        public void InsertRule(RuleEntity rule)
        {
            Guard.NotNull(rule, nameof(rule));

            // TODO: Update parent set > hooking
            _rsRules.Insert(rule);
            InvalidateRuleSet(rule.RuleSetId);
        }

        public void UpdateRule(RuleEntity rule)
        {
            Guard.NotNull(rule, nameof(rule));

            // TODO: Update parent set > hooking
            _rsRules.Update(rule);
            InvalidateRuleSet(rule.RuleSetId);
        }

        public void DeleteRule(RuleEntity rule)
        {
            Guard.NotNull(rule, nameof(rule));

            // TODO: Update parent set > hooking
            _rsRules.Delete(rule);
            InvalidateRuleSet(rule.RuleSetId);
        }

        #endregion

        //private RuleSetEntity CreateTestRuleSet()
        //{
        //    // TODO: remove later
        //    // TODO: get rule set from db
        //    var ruleSet = new RuleSetEntity { Id = 1, LogicalOperator = LogicalRuleOperator.And };

        //    // TODO: Get all rules for requested set from db
        //    ruleSet.Rules = new List<RuleEntity>
        //    {
        //        new RuleEntity { RuleSetId = 1, RuleType = "CartTotal", Operator = RuleOperator.GreaterThanOrEqualTo, Value = "1000" },
        //        new RuleEntity { RuleSetId = 1, RuleType = "Store", Operator = RuleOperator.In, Value = "1,2,3,4,5" },
        //        new RuleEntity { RuleSetId = 1, RuleType = "Language", Operator = RuleOperator.NotIn, Value = "3" },
        //        new RuleEntity { RuleSetId = 1, RuleType = "Currency", Operator = RuleOperator.In, Value = "1,2,3" },
        //        // This one is composite and contains other rules. "Value" refers to RuleSetEntity.Id in this case.
        //        new RuleEntity { RuleSetId = 1, RuleType = "Group", Operator = RuleOperator.IsEqualTo, Value = "2" },
        //    };

        //    return ruleSet;
        //}
    }
}
