using System.Collections.Generic;
using SmartStore.Collections;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Security;

namespace SmartStore.Services.Security
{
    /// <summary>
    /// Permission service interface.
    /// </summary>
    public partial interface IPermissionService2
    {
        /// <summary>
        /// Gets a permission.
        /// </summary>
        /// <param name="permissionId">Permission identifier.</param>
        /// <returns>Permission.</returns>
        PermissionRecord GetPermissionRecordById(int permissionId);

        /// <summary>
        /// Gets a permission.
        /// </summary>
        /// <param name="systemName">Permission system name.</param>
        /// <returns>Permission.</returns>
        PermissionRecord GetPermissionRecordBySystemName(string systemName);

        /// <summary>
        /// Gets all permissions.
        /// </summary>
        /// <returns>Permissions.</returns>
        IList<PermissionRecord> GetAllPermissionRecords();

        /// <summary>
        /// Inserts a permission.
        /// </summary>
        /// <param name="permission">Permission.</param>
        void InsertPermissionRecord(PermissionRecord permission);

        /// <summary>
        /// Updates a permission.
        /// </summary>
        /// <param name="permission">Permission.</param>
        void UpdatePermissionRecord(PermissionRecord permission);

        /// <summary>
        /// Deletes a permission.
        /// </summary>
        /// <param name="permission">Permission.</param>
        void DeletePermissionRecord(PermissionRecord permission);


        /// <summary>
        /// Install permissions.
        /// </summary>
        /// <param name="permissionProvider">Permission provider.</param>
        void InstallPermissions(IPermissionProvider permissionProvider);

        /// <summary>
        /// Uninstall permissions.
        /// </summary>
        /// <param name="permissionProvider">Permission provider.</param>
        void UninstallPermissions(IPermissionProvider permissionProvider);


        /// <summary>
        /// Authorize permission.
        /// </summary>
        /// <param name="permission">Permission.</param>
        /// <returns><c>true</c> authorized otherwise <c>false</c>.</returns>
        bool Authorize(PermissionRecord permission);

        /// <summary>
        /// Authorize permission.
        /// </summary>
        /// <param name="permission">Permission.</param>
        /// <param name="customer">Customer.</param>
        /// <returns><c>true</c> authorized otherwise <c>false</c>.</returns>
        bool Authorize(PermissionRecord permission, Customer customer);

        /// <summary>
        /// Authorize permission.
        /// </summary>
        /// <param name="permissionRecordSystemName">Permission record system name.</param>
        /// <returns><c>true</c> authorized otherwise <c>false</c>.</returns>
        bool Authorize(string permissionRecordSystemName);

        /// <summary>
        /// Authorize permission.
        /// </summary>
        /// <param name="permissionRecordSystemName">Permission record system name.</param>
        /// <param name="customer">Customer.</param>
        /// <returns><c>true</c> authorized otherwise <c>false</c>.</returns>
        bool Authorize(string permissionRecordSystemName, Customer customer);


        /// <summary>
        /// Gets the permission tree for a customer role.
        /// </summary>
        /// <param name="role">Customer role.</param>
        /// <returns>Permission tree.</returns>
        TreeNode<IPermissionNode> GetPermissionTree(CustomerRole role);
    }
}
