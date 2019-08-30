using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Security;
using SmartStore.Services.Customers;

namespace SmartStore.Services.Security
{
    public partial class PermissionService2 : IPermissionService2
    {
        // {0} = roleId
        internal const string PERMISSION_TREE_KEY = "permission:tree-{0}";
        private const string PERMISSION_TREE_PATTERN_KEY = "permission:tree-*";

        private readonly IRepository<PermissionRecord> _permissionRepository;
        private readonly IRepository<PermissionRoleMapping> _permissionMappingRepository;
        private readonly Lazy<ICustomerService> _customerService;
        private readonly IWorkContext _workContext;
        private readonly ICacheManager _cacheManager;

        public PermissionService2(
            IRepository<PermissionRecord> permissionRepository,
            IRepository<PermissionRoleMapping> permissionMappingRepository,
            Lazy<ICustomerService> customerService,
            IWorkContext workContext,
            ICacheManager cacheManager)
        {
            _permissionRepository = permissionRepository;
            _permissionMappingRepository = permissionMappingRepository;
            _customerService = customerService;
            _workContext = workContext;
            _cacheManager = cacheManager;
        }

        public virtual PermissionRecord GetPermissionById(int permissionId)
        {
            if (permissionId == 0)
            {
                return null;
            }

            return _permissionRepository.GetById(permissionId);
        }

        public virtual PermissionRecord GetPermissionBySystemName(string systemName)
        {
            if (systemName.IsEmpty())
            {
                return null;
            }

            var permission = _permissionRepository.Table
                .Where(x => x.SystemName == systemName)
                .OrderBy(x => x.Id)
                .FirstOrDefault();

            return permission;
        }

        public virtual IList<PermissionRecord> GetAllPermissions()
        {
            var permissions = _permissionRepository.Table.ToList();
            return permissions;
        }

        public virtual void InsertPermission(PermissionRecord permission)
        {
            Guard.NotNull(permission, nameof(permission));

            _permissionRepository.Insert(permission);

            _cacheManager.RemoveByPattern(PERMISSION_TREE_PATTERN_KEY);
        }

        public virtual void UpdatePermission(PermissionRecord permission)
        {
            Guard.NotNull(permission, nameof(permission));

            _permissionRepository.Update(permission);

            _cacheManager.RemoveByPattern(PERMISSION_TREE_PATTERN_KEY);
        }

        public virtual void DeletePermission(PermissionRecord permission)
        {
            if (permission != null)
            {
                _permissionRepository.Delete(permission);

                _cacheManager.RemoveByPattern(PERMISSION_TREE_PATTERN_KEY);
            }
        }


        public virtual PermissionRoleMapping GetPermissionRoleMappingById(int mappingId)
        {
            if (mappingId == 0)
            {
                return null;
            }

            return _permissionMappingRepository.GetById(mappingId);
        }

        public virtual void InsertPermissionRoleMapping(PermissionRoleMapping mapping)
        {
            Guard.NotNull(mapping, nameof(mapping));

            _permissionMappingRepository.Insert(mapping);

            _cacheManager.RemoveByPattern(PERMISSION_TREE_PATTERN_KEY);
        }

        public virtual void UpdatePermissionRoleMapping(PermissionRoleMapping mapping)
        {
            Guard.NotNull(mapping, nameof(mapping));

            _permissionMappingRepository.Update(mapping);

            _cacheManager.RemoveByPattern(PERMISSION_TREE_PATTERN_KEY);
        }

        public virtual void DeletePermissionRoleMapping(PermissionRoleMapping mapping)
        {
            if (mapping != null)
            {
                _permissionMappingRepository.Delete(mapping);

                _cacheManager.RemoveByPattern(PERMISSION_TREE_PATTERN_KEY);
            }
        }


        public virtual void InstallPermissions(IPermissionProvider2 permissionProvider)
        {
            Guard.NotNull(permissionProvider, nameof(permissionProvider));

            var permissions = permissionProvider.GetPermissions();
            if (!permissions.Any())
            {
                return;
            }

            using (var scope = new DbContextScope(_permissionRepository.Context, autoDetectChanges: false, autoCommit: false))
            {
                foreach (var permission in permissions)
                {
                    var newPermission = GetPermissionBySystemName(permission.SystemName);
                    if (newPermission == null)
                    {
                        newPermission = new PermissionRecord
                        {
                            Name = string.Empty,
                            SystemName = permission.SystemName,
                            Category = string.Empty
                        };

                        // Default customer role mappings.
                        var defaultPermissions = permissionProvider.GetDefaultPermissions();
                        foreach (var defaultPermission in defaultPermissions)
                        {
                            var customerRole = _customerService.Value.GetCustomerRoleBySystemName(defaultPermission.CustomerRoleSystemName);
                            if (customerRole == null)
                            {
                                customerRole = new CustomerRole
                                {
                                    Active = true,
                                    Name = defaultPermission.CustomerRoleSystemName,
                                    SystemName = defaultPermission.CustomerRoleSystemName
                                };
                                _customerService.Value.InsertCustomerRole(customerRole);
                            }

                            if (defaultPermission.PermissionRecords.Any(x => x.SystemName == newPermission.SystemName))
                            {
                                if (!customerRole.PermissionRoleMappings.Where(x => x.PermissionRecord.SystemName == newPermission.SystemName).Select(x => x.PermissionRecord).Any())
                                {
                                    newPermission.PermissionRoleMappings.Add(new PermissionRoleMapping
                                    {
                                        Allow = true,
                                        CustomerRoleId = customerRole.Id
                                    });
                                }
                            }
                        }

                        InsertPermission(newPermission);
                    }
                }

                scope.Commit();
                _cacheManager.RemoveByPattern(PERMISSION_TREE_PATTERN_KEY);
            }
        }

        public virtual void UninstallPermissions(IPermissionProvider2 permissionProvider)
        {
            var permissions = permissionProvider.GetPermissions();
            var systemNames = new HashSet<string>(permissions.Select(x => x.SystemName));

            if (systemNames.Any())
            {
                var toDelete = _permissionRepository.Table
                    .Where(x => systemNames.Contains(x.SystemName))
                    .ToList();

                toDelete.Each(x => DeletePermission(x));

                _cacheManager.RemoveByPattern(PERMISSION_TREE_PATTERN_KEY);
            }
        }


        public virtual bool Authorize(PermissionRecord permission)
        {
            return Authorize(permission, _workContext.CurrentCustomer);
        }

        public virtual bool Authorize(PermissionRecord permission, Customer customer)
        {
            if (permission == null || customer == null)
            {
                return false;
            }

            return Authorize(permission.SystemName, customer);
        }

        public virtual bool Authorize(string permissionSystemName)
        {
            return Authorize(permissionSystemName, _workContext.CurrentCustomer);
        }

        public virtual bool Authorize(string permissionSystemName, Customer customer)
        {
            if (string.IsNullOrEmpty(permissionSystemName))
            {
                return false;
            }

            var result = false;

            foreach (var role in customer.CustomerRoles.Where(x => x.Active))
            {
                var tree = GetPermissionTree(role);
                var node = tree.SelectNodeById(permissionSystemName);
                if (node == null)
                {
                    throw new SmartException($"Unknown permission \"{permissionSystemName}\".");
                }

                // Find explicit allow or deny.
                while (node != null && !node.Value.Allow.HasValue)
                {
                    node = node.Parent;
                }
                if (node == null || !node.Value.Allow.HasValue)
                {
                    continue;
                }

                if (node.Value.Allow.Value)
                {
                    result = true;
                }
                else
                {
                    return false;
                }
            }

            return result;
        }

        public virtual TreeNode<IPermissionNode> GetPermissionTree(CustomerRole role)
        {
            Guard.NotNull(role, nameof(role));

            var result = _cacheManager.Get(PERMISSION_TREE_KEY.FormatInvariant(role.Id), () =>
            {
                var root = new TreeNode<IPermissionNode>(new PermissionNode());

                var permissions = _permissionRepository.TableUntracked
                    .Expand(x => x.PermissionRoleMappings)
                    .Where(x => string.IsNullOrEmpty(x.Name))//GP: TODO, remove clause later
                    .ToList();

                AddChildItems(permissions, root, null);

                return root;
            });

            return result;

            void AddChildItems(List<PermissionRecord> permissions, TreeNode<IPermissionNode> parentNode, string path)
            {
                if (parentNode == null)
                {
                    return;
                }

                IEnumerable<PermissionRecord> entities = null;
                    
                if (path == null)
                {
                    entities = permissions.Where(x => x.SystemName.IndexOf('.') == -1);
                }
                else
                {
                    var tmpPath = path.EnsureEndsWith(".");
                    entities = permissions.Where(x => x.SystemName.StartsWith(tmpPath) && x.SystemName.IndexOf('.', tmpPath.Length) == -1);
                }

                foreach (var entity in entities)
                {
                    var mapping = entity.PermissionRoleMappings.FirstOrDefault(x => x.CustomerRoleId == role.Id);

                    var newNode = parentNode.Append(new PermissionNode
                    {
                        PermissionRecordId = entity.Id,
                        Allow = mapping?.Allow ?? null,     // null = inherit
                        SystemName = entity.SystemName
                    }, entity.SystemName);

                    AddChildItems(permissions, newNode, entity.SystemName);
                }
            }
        }
    }
}
