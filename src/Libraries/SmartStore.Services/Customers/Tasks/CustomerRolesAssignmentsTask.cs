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
    public partial class CustomerRolesAssignmentsTask : AsyncTask
    {
        protected readonly IRepository<CustomerRole> _customerRoleRepository;
        protected readonly IRepository<CustomerRoleMapping> _customerRoleMappingRepository;
        protected readonly IRuleFactory _ruleFactory;
        protected readonly ITargetGroupService _targetGroupService;
        protected readonly ICacheManager _cacheManager;

        public CustomerRolesAssignmentsTask(
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
            var clearCache = false;
            var roleQuery = _customerRoleRepository.TableUntracked.Expand(x => x.RuleSets);

            if (ctx.Parameters.ContainsKey("CustomerRoleIds"))
            {
                var rolesIds = ctx.Parameters["CustomerRoleIds"].ToIntArray();
                roleQuery = roleQuery.Where(x => rolesIds.Contains(x.Id));
            }

            var roles = await roleQuery.ToListAsync();
            if (!roles.Any())
            {
                return;
            }

            using var scope = new DbContextScope(ctx: _customerRoleMappingRepository.Context, autoDetectChanges: false, validateOnSave: false, hooksEnabled: false, autoCommit: false);

            foreach (var role in roles)
            {
                try
                {
                    ctx.SetProgress(++count, roles.Count, $"Sync customer assignments for role {role.SystemName.NaIfEmpty()}.");

                    _customerRoleMappingRepository.Context.DetachEntities(x => x is CustomerRoleMapping);
                }
                catch { }

                var ruleSetCustomerIds = new HashSet<int>();
                var existingCustomerIds = new HashSet<int>();
                var numDeleted = 0;
                var numAdded = 0;

                // Execute active rule sets and collect customer ids.
                // Delete old mappings if the role is inactive or has no assigned rule sets.
                if (role.Active)
                {
                    foreach (var ruleSet in role.RuleSets.Where(x => x.IsActive))
                    {
                        if (ctx.CancellationToken.IsCancellationRequested) return;

                        var expression = _ruleFactory.CreateExpressionGroup(ruleSet, _targetGroupService) as FilterExpression;
                        if (expression != null)
                        {
                            var filterResult = _targetGroupService.ProcessFilter(expression, 0, 500);
                            var resultPager = new FastPager<Customer>(filterResult.SourceQuery, 500);

                            for (var i = 0; i < 9999999; ++i)
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
                }

                // Sync mappings.
                var query = _customerRoleMappingRepository.Table.Where(x => x.CustomerRoleId == role.Id && x.IsSystemMapping);
                var pager = new FastPager<CustomerRoleMapping>(query, 500);

                // Mappings to delete.
                for (var i = 0; i < 9999999; ++i)
                {
                    if (ctx.CancellationToken.IsCancellationRequested) return;

                    var mappings = await pager.ReadNextPageAsync<CustomerRoleMapping>();
                    if (!(mappings?.Any() ?? false))
                    {
                        break;
                    }

                    foreach (var mapping in mappings)
                    {
                        if (!role.Active || !ruleSetCustomerIds.Contains(mapping.CustomerId))
                        {
                            _customerRoleMappingRepository.Delete(mapping);
                            
                            ++numDeleted;
                            clearCache = true;
                        }
                        else
                        {
                            existingCustomerIds.Add(mapping.CustomerId);
                        }
                    }

                    await scope.CommitAsync();
                }

                // Mappings to add.
                if (role.Active)
                {
                    var toAdd = ruleSetCustomerIds.Except(existingCustomerIds).ToList();
                    if (toAdd.Any())
                    {
                        foreach (var chunk in toAdd.Slice(500))
                        {
                            if (ctx.CancellationToken.IsCancellationRequested) return;

                            foreach (var customerId in chunk)
                            {
                                _customerRoleMappingRepository.Insert(new CustomerRoleMapping
                                {
                                    CustomerId = customerId,
                                    CustomerRoleId = role.Id,
                                    IsSystemMapping = true
                                });

                                ++numAdded;
                                clearCache = true;
                            }

                            await scope.CommitAsync();
                        }
                    }
                }

                Debug.WriteLineIf(numDeleted > 0 || numAdded > 0, $"Customer assignments for {role.SystemName.NaIfEmpty()}: deleted {numDeleted}, added {numAdded}.");
            }

            if (clearCache)
            {
                _cacheManager.RemoveByPattern(AclService.ACL_SEGMENT_PATTERN);
            }
        }
    }
}
