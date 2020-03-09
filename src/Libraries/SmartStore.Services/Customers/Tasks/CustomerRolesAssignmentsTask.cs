using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;
using SmartStore.Data.Utilities;
using SmartStore.Rules;
using SmartStore.Rules.Domain;
using SmartStore.Rules.Filters;
using SmartStore.Services.Tasks;

namespace SmartStore.Services.Customers
{
    /// <summary>
    /// Updates the system assignments to customer roles for rules.
    /// </summary>
    public partial class CustomerRolesAssignmentsTask : AsyncTask
    {
        protected readonly IRepository<CustomerRole> _customerRoleRepository;
        protected readonly IRepository<CustomerRoleMapping> _customerRoleMappingRepository;
        protected readonly IRuleFactory _ruleFactory;
        protected readonly ITargetGroupService _targetGroupService;

        public CustomerRolesAssignmentsTask(
            IRepository<CustomerRole> customerRoleRepository,
            IRepository<CustomerRoleMapping> customerRoleMappingRepository,
            IRuleFactory ruleFactory,
            ITargetGroupService targetGroupService)
        {
            _customerRoleRepository = customerRoleRepository;
            _customerRoleMappingRepository = customerRoleMappingRepository;
            _ruleFactory = ruleFactory;
            _targetGroupService = targetGroupService;
        }

        public override async Task ExecuteAsync(TaskExecutionContext ctx)
        {
            // Get rule sets and organize them in a way that each set is only executed once.
            var ruleSets = new Dictionary<int, RuleSetEntity>();
            var ruleSetIdToRole = new Multimap<int, CustomerRole>();
            var roleQuery = _customerRoleRepository.TableUntracked.Expand(x => x.RuleSets);

            if (ctx.Parameters.ContainsKey("CustomerRoleIds"))
            {
                var rolesIds = ctx.Parameters["CustomerRoleIds"].ToIntArray();
                roleQuery = roleQuery.Where(x => rolesIds.Contains(x.Id));
            }

            var roles = await roleQuery
                .Where(x => x.RuleSets.Any())
                .ToListAsync();

            foreach (var role in roles)
            {
                foreach (var ruleSet in role.RuleSets.Where(x => x.Scope == RuleScope.Customer))
                {
                    ruleSets[ruleSet.Id] = ruleSet;
                    ruleSetIdToRole.Add(ruleSet.Id, role);
                }
            }

            if (!ruleSetIdToRole.Any())
            {
                return;
            }

            using var scope = new DbContextScope(ctx: _customerRoleMappingRepository.Context, validateOnSave: false, hooksEnabled: false, autoCommit: false);

            // Sync customer role mappings created by this task.
            foreach (var pair in ruleSetIdToRole)
            {
                var ruleSet = ruleSets[pair.Key];
                IPagedList<Customer> customers = null;

                foreach (var role in pair.Value)
                {
                    if (!role.Active)
                    {
                        // Delete existing mappings but do not add anything.
                        var query = _customerRoleMappingRepository.Table.Where(x => x.CustomerRoleId == role.Id && x.IsSystemMapping);
                        await DeleteMappingsAsync(scope, query);
                        continue;
                    }

                    // Excecute rule set.
                    if (customers == null)
                    {
                        var expression = _ruleFactory.CreateExpressionGroup(ruleSet, _targetGroupService) as FilterExpression;
                        customers = _targetGroupService.ProcessFilter(new[] { expression }, LogicalRuleOperator.And, 0, 500);
                    }

                    // TODO....
                }

                try
                {
                    _customerRoleMappingRepository.Context.DetachEntities(x => x is CustomerRoleMapping || x is Customer);
                }
                catch { }
            }
        }

        private async Task<int> DeleteMappingsAsync(DbContextScope scope, IQueryable<CustomerRoleMapping> query)
        {
            var num = 0;
            var pager = new FastPager<CustomerRoleMapping>(query, 500);

            for (var i = 0; i < 9999999; ++i)
            {
                var mappings = await pager.ReadNextPageAsync<CustomerRoleMapping>();
                if (!(mappings?.Any() ?? false))
                {
                    break;
                }

                _customerRoleMappingRepository.DeleteRange(mappings);
                num += await scope.CommitAsync();
            }

            return num;
        }
    }
}
