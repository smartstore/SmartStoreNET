using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Threading.Tasks;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Rules.Domain;

namespace SmartStore.Rules
{
    public interface IRuleStorage
    {
        RuleSetEntity GetRuleSetById(int id, bool withRules, bool forEdit);
        IList<RuleSetEntity> GetRuleSetsByIds(int[] ids, bool withRules);
        IPagedList<RuleSetEntity> GetAllRuleSets(
            bool forEdit,
            bool withRules,
            RuleScope? scope = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            bool includeSubGroups = false,
            bool includeHidden = false);

        void InsertRuleSet(RuleSetEntity ruleSet);
        void UpdateRuleSet(RuleSetEntity ruleSet);
        void DeleteRuleSet(RuleSetEntity ruleSet);

        void InsertRule(RuleEntity rule);
        void UpdateRule(RuleEntity rule);
        void DeleteRule(RuleEntity rule);
    }

    public partial class RuleStorage : IRuleStorage
    {
        private readonly IRepository<RuleSetEntity> _rsRuleSets;
        private readonly IRepository<RuleEntity> _rsRules;

        public RuleStorage(IRepository<RuleSetEntity> rsRuleSets, IRepository<RuleEntity> rsRules)
        {
            _rsRuleSets = rsRuleSets;
            _rsRules = rsRules;
        }

        #region RuleSets

        public RuleSetEntity GetRuleSetById(int id, bool withRules, bool forEdit)
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

            return new PagedList<RuleSetEntity>(query, pageIndex, pageSize);
        }

        public void InsertRuleSet(RuleSetEntity ruleSet)
        {
            Guard.NotNull(ruleSet, nameof(ruleSet));

            _rsRuleSets.Insert(ruleSet);
        }

        public void UpdateRuleSet(RuleSetEntity ruleSet)
        {
            Guard.NotNull(ruleSet, nameof(ruleSet));

            _rsRuleSets.Update(ruleSet);
        }

        public void DeleteRuleSet(RuleSetEntity ruleSet)
        {
            Guard.NotNull(ruleSet, nameof(ruleSet));

            _rsRuleSets.Delete(ruleSet);
        }

        #endregion

        #region Rules

        public void InsertRule(RuleEntity rule)
        {
            Guard.NotNull(rule, nameof(rule));

            // TODO: Update parent set > hooking
            _rsRules.Insert(rule);
        }

        public void UpdateRule(RuleEntity rule)
        {
            Guard.NotNull(rule, nameof(rule));

            // TODO: Update parent set > hooking
            _rsRules.Update(rule);
        }

        public void DeleteRule(RuleEntity rule)
        {
            Guard.NotNull(rule, nameof(rule));

            // TODO: Update parent set > hooking
            _rsRules.Delete(rule);
        }

        #endregion

        private RuleSetEntity CreateTestRuleSet()
        {
            // TODO: remove later
            // TODO: get rule set from db
            var ruleSet = new RuleSetEntity { Id = 1, LogicalOperator = LogicalRuleOperator.And };

            // TODO: Get all rules for requested set from db
            ruleSet.Rules = new List<RuleEntity>
            {
                new RuleEntity { RuleSetId = 1, RuleType = "CartTotal", Operator = RuleOperator.GreaterThanOrEqualTo, Value = "1000" },
                new RuleEntity { RuleSetId = 1, RuleType = "Store", Operator = RuleOperator.In, Value = "1,2,3,4,5" },
                new RuleEntity { RuleSetId = 1, RuleType = "Language", Operator = RuleOperator.NotIn, Value = "3" },
                new RuleEntity { RuleSetId = 1, RuleType = "Currency", Operator = RuleOperator.In, Value = "1,2,3" },
                // This one is composite and contains other rules. "Value" refers to RuleSetEntity.Id in this case.
                new RuleEntity { RuleSetId = 1, RuleType = "Group", Operator = RuleOperator.IsEqualTo, Value = "2" },
            };

            return ruleSet;
        }
    }
}
