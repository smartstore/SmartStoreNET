using System.Collections.Generic;
using System.Linq;
using SmartStore.Collections;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Security;

namespace SmartStore.Services.Security
{
    public partial class PermissionService2 : IPermissionService2
    {
        // {0} = roleId
        private const string PERMISSION_TREE_KEY = "permission:tree-{0}";
        private const string PERMISSION_TREE_PATTERN_KEY = "permission:tree-*";

        private readonly IRepository<PermissionRecord> _permissionRecordRepository;
        private readonly ICacheManager _cacheManager;

        public PermissionService2(
            IRepository<PermissionRecord> permissionRecordRepository,
            ICacheManager cacheManager)
        {
            _permissionRecordRepository = permissionRecordRepository;
            _cacheManager = cacheManager;
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
