using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;
using SmartStore.Data.Utilities;
using SmartStore.Rules;
using SmartStore.Rules.Filters;
using SmartStore.Services.Security;
using SmartStore.Services.Tasks;

namespace SmartStore.Services.Customers
{
    /// <summary>
    /// Updates the system assignments to customer roles for rules.
    /// </summary>
    public partial class TargetGroupEvaluatorTask : AsyncTask
    {
        protected readonly IRepository<CustomerRole> _customerRoleRepository;
        protected readonly IRepository<CustomerRoleMapping> _customerRoleMappingRepository;
        protected readonly IRuleFactory _ruleFactory;
        protected readonly ITargetGroupService _targetGroupService;
        protected readonly ICacheManager _cacheManager;

        public TargetGroupEvaluatorTask(
            IRepository<CustomerRole> customerRoleRepository,
            IRepository<CustomerRoleMapping> customerRoleMappingRepository,
            IRuleFactory ruleFactory,
            ITargetGroupService targetGroupService,
            ICacheManager cacheManager)
        {
            _customerRoleRepository = customerRoleRepository;
            _customerRoleMappingRepository = customerRoleMappingRepository;
            _ruleFactory = ruleFactory;
            _targetGroupService = targetGroupService;
            _cacheManager = cacheManager;
        }

        public override async Task ExecuteAsync(TaskExecutionContext ctx)
        {
            var count = 0;
            var numDeleted = 0;
            var numAdded = 0;
            var roleQuery = _customerRoleRepository.TableUntracked.Expand(x => x.RuleSets);

            if (ctx.Parameters.ContainsKey("CustomerRoleIds"))
            {
                var roleIds = ctx.Parameters["CustomerRoleIds"].ToIntArray();
                roleQuery = roleQuery.Where(x => roleIds.Contains(x.Id));

                numDeleted = _customerRoleMappingRepository.Context.ExecuteSqlCommand(
                    "Delete From [dbo].[CustomerRoleMapping] Where [CustomerRoleId] In ({0}) And [IsSystemMapping] = 1",
                    false,
                    null,
                    string.Join(",", roleIds));
            }
            else
            {
                numDeleted = _customerRoleMappingRepository.Context.ExecuteSqlCommand("Delete From [dbo].[CustomerRoleMapping] Where [IsSystemMapping] = 1");
            }

            var roles = await roleQuery
                .Where(x => x.Active && x.RuleSets.Any(y => y.IsActive))
                .ToListAsync();

            using (var scope = new DbContextScope(ctx: _customerRoleMappingRepository.Context, autoDetectChanges: false, validateOnSave: false, hooksEnabled: false, autoCommit: false))
            {
                foreach (var role in roles)
                {
                    var ruleSetCustomerIds = new HashSet<int>();

                    ctx.SetProgress(++count, roles.Count, $"Add customer assignments for role \"{role.SystemName.NaIfEmpty()}\".");

                    // Execute active rule sets and collect customer ids.
                    foreach (var ruleSet in role.RuleSets.Where(x => x.IsActive))
                    {
                        if (ctx.CancellationToken.IsCancellationRequested)
                            return;

                        if (_ruleFactory.CreateExpressionGroup(ruleSet, _targetGroupService) is FilterExpression expression)
                        {
                            var filterResult = _targetGroupService.ProcessFilter(expression, 0, 500);
                            var resultPager = new FastPager<Customer>(filterResult.SourceQuery, 500);

                            while (true)
                            {
                                var customerIds = await resultPager.ReadNextPageAsync(x => x.Id, x => x);
                                if (!(customerIds?.Any() ?? false))
                                {
                                    break;
                                }

                                ruleSetCustomerIds.AddRange(customerIds);
                            }
                        }
                    }

                    // Add mappings.
                    if (ruleSetCustomerIds.Any())
                    {
                        foreach (var chunk in ruleSetCustomerIds.Slice(500))
                        {
                            if (ctx.CancellationToken.IsCancellationRequested)
                                return;

                            foreach (var customerId in chunk)
                            {
                                _customerRoleMappingRepository.Insert(new CustomerRoleMapping
                                {
                                    CustomerId = customerId,
                                    CustomerRoleId = role.Id,
                                    IsSystemMapping = true
                                });

                                ++numAdded;
                            }

                            await scope.CommitAsync();
                        }

                        try
                        {
                            _customerRoleMappingRepository.Context.DetachEntities<CustomerRoleMapping>();
                        }
                        catch { }
                    }
                }
            }

            if (numAdded > 0 || numDeleted > 0)
            {
                _cacheManager.RemoveByPattern(AclService.ACL_SEGMENT_PATTERN);
            }

            Debug.WriteLineIf(numDeleted > 0 || numAdded > 0, $"Deleted {numDeleted} and added {numAdded} customer assignments for {roles.Count} roles.");
        }
    }
}
