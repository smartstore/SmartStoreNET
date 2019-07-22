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
        private const string PERMISSION_TREE_KEY = "permission:tree-{0}";
        private const string PERMISSION_TREE_PATTERN_KEY = "permission:tree-*";

        private readonly IRepository<PermissionRecord> _permissionRecordRepository;
        private readonly Lazy<ICustomerService> _customerService;
        private readonly IWorkContext _workContext;
        private readonly ICacheManager _cacheManager;

        public PermissionService2(
            IRepository<PermissionRecord> permissionRecordRepository,
            Lazy<ICustomerService> customerService,
            IWorkContext workContext,
            ICacheManager cacheManager)
        {
            _permissionRecordRepository = permissionRecordRepository;
            _customerService = customerService;
            _workContext = workContext;
            _cacheManager = cacheManager;
        }

        public virtual PermissionRecord GetPermissionRecordById(int permissionId)
        {
            if (permissionId == 0)
            {
                return null;
            }

            return _permissionRecordRepository.GetById(permissionId);
        }

        public virtual PermissionRecord GetPermissionRecordBySystemName(string systemName)
        {
            if (systemName.IsEmpty())
            {
                return null;
            }

            var permission = _permissionRecordRepository.Table
                .Where(x => x.SystemName == systemName)
                .OrderBy(x => x.Id)
                .FirstOrDefault();

            return permission;
        }

        public virtual IList<PermissionRecord> GetAllPermissionRecords()
        {
            var permissions = _permissionRecordRepository.Table.ToList();
            return permissions;
        }

        public virtual void InsertPermissionRecord(PermissionRecord permission)
        {
            Guard.NotNull(permission, nameof(permission));

            _permissionRecordRepository.Insert(permission);

            _cacheManager.RemoveByPattern(PERMISSION_TREE_PATTERN_KEY);
        }

        public virtual void UpdatePermissionRecord(PermissionRecord permission)
        {
            Guard.NotNull(permission, nameof(permission));

            _permissionRecordRepository.Update(permission);

            _cacheManager.RemoveByPattern(PERMISSION_TREE_PATTERN_KEY);
        }

        public virtual void DeletePermissionRecord(PermissionRecord permission)
        {
            if (permission != null)
            {
                _permissionRecordRepository.Delete(permission);

                _cacheManager.RemoveByPattern(PERMISSION_TREE_PATTERN_KEY);
            }
        }

        
        public virtual void InstallPermissions(IPermissionProvider permissionProvider)
        {
            Guard.NotNull(permissionProvider, nameof(permissionProvider));

            using (var scope = new DbContextScope(_permissionRecordRepository.Context, autoDetectChanges: false, autoCommit: false))
            {
                var permissions = permissionProvider.GetPermissions();
                foreach (var permission in permissions)
                {
                    var permission1 = GetPermissionRecordBySystemName(permission.SystemName);
                    if (permission1 == null)
                    {
                        permission1 = new PermissionRecord { SystemName = permission.SystemName };

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

                            if (defaultPermission.PermissionRecords.Any(x => x.SystemName == permission1.SystemName))
                            {
                                if (!customerRole.PermissionRoleMappings.Where(x => x.PermissionRecord.SystemName == permission1.SystemName).Select(x => x.PermissionRecord).Any())
                                {
                                    permission1.PermissionRoleMappings.Add(new PermissionRoleMapping
                                    {
                                        Allow = true,
                                        CustomerRoleId = customerRole.Id
                                    });
                                }
                            }
                        }

                        InsertPermissionRecord(permission1);
                    }
                }

                scope.Commit();
            }
        }

        public virtual void UninstallPermissions(IPermissionProvider permissionProvider)
        {
            var permissions = permissionProvider.GetPermissions();
            var systemNames = new HashSet<string>(permissions.Select(x => x.SystemName));

            var toDelete = _permissionRecordRepository.Table
                .Where(x => systemNames.Contains(x.SystemName))
                .ToList();

            toDelete.Each(x => DeletePermissionRecord(x));
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

        public virtual bool Authorize(string permissionRecordSystemName)
        {
            return Authorize(permissionRecordSystemName, _workContext.CurrentCustomer);
        }

        public virtual bool Authorize(string permissionRecordSystemName, Customer customer)
        {
            if (string.IsNullOrEmpty(permissionRecordSystemName))
            {
                return false;
            }

            foreach (var role in customer.CustomerRoles.Where(x => x.Active))
            {
                if (Authorize(permissionRecordSystemName, role))
                {
                    return true;
                }
            }

            return false;
        }

        protected virtual bool Authorize(string permissionRecordSystemName, CustomerRole role)
        {
            throw new NotImplementedException();
        }


        public virtual TreeNode<IPermissionNode> GetPermissionTree(CustomerRole role)
        {
            Guard.NotNull(role, nameof(role));

            // TODO: invalidate cache.
            var result = _cacheManager.Get(PERMISSION_TREE_KEY.FormatInvariant(role.Id), () =>
            {
                var root = new TreeNode<IPermissionNode>(new PermissionNode())
                {
                    Id = role.Name,
                };

                // TODO: caching
                var permissions = _permissionRecordRepository.TableUntracked
                    .Expand(x => x.PermissionRoleMappings)
                    .Where(x => x.Name == null)//GP: TODO, remove clause later
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
                        Allow = mapping?.Allow ?? null,     // null = inherit
                        SystemName = entity.SystemName
                    });

                    AddChildItems(permissions, newNode, entity.SystemName);
                }
            }
        }
    }
}
