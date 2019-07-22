using SmartStore.Collections;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Security;

namespace SmartStore.Services.Security
{
    public partial interface IPermissionService2
    {
        /// <summary>
        /// Gets the permission tree for a customer role.
        /// </summary>
        /// <param name="role">Customer role.</param>
        /// <returns>Permission tree.</returns>
        TreeNode<IPermissionNode> GetPermissionTree(CustomerRole role);
    }
}
