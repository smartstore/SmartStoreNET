using System.Collections.Generic;
using SmartStore.Collections;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Security;

namespace SmartStore.Core.Security
{
    /// <summary>
    /// Permission service interface.
    /// </summary>
    public partial interface IPermissionService
    {
        /// <summary>
        /// Gets a permission.
        /// </summary>
        /// <param name="permissionId">Permission identifier.</param>
        /// <returns>Permission.</returns>
        PermissionRecord GetPermissionById(int permissionId);

        /// <summary>
        /// Gets a permission.
        /// </summary>
        /// <param name="systemName">Permission system name.</param>
        /// <returns>Permission.</returns>
        PermissionRecord GetPermissionBySystemName(string systemName);

        /// <summary>
        /// Gets all permissions.
        /// </summary>
        /// <returns>Permissions.</returns>
        IList<PermissionRecord> GetAllPermissions();

        /// <summary>
        /// Gets system and display name of all permissions.
        /// </summary>
        /// <returns>System and display names.</returns>
        IDictionary<string, string> GetAllSystemNames();

        /// <summary>
        /// Inserts a permission.
        /// </summary>
        /// <param name="permission">Permission.</param>
        void InsertPermission(PermissionRecord permission);

        /// <summary>
        /// Updates a permission.
        /// </summary>
        /// <param name="permission">Permission.</param>
        void UpdatePermission(PermissionRecord permission);

        /// <summary>
        /// Deletes a permission.
        /// </summary>
        /// <param name="permission">Permission.</param>
        void DeletePermission(PermissionRecord permission);


        /// <summary>
        /// Gets a permission role mapping.
        /// </summary>
        /// <param name="mappingId">Permission role mapping identifier.</param>
        /// <returns>Permission role mapping.</returns>
        PermissionRoleMapping GetPermissionRoleMappingById(int mappingId);

        /// <summary>
        /// Inserts a permission role mapping.
        /// </summary>
        /// <param name="mapping">Permission role mapping.</param>
        void InsertPermissionRoleMapping(PermissionRoleMapping mapping);

        /// <summary>
        /// Updates a permission role mapping.
        /// </summary>
        /// <param name="mapping">Permission role mapping.</param>
        void UpdatePermissionRoleMapping(PermissionRoleMapping mapping);

        /// <summary>
        /// Deletes a permission role mapping.
        /// </summary>
        /// <param name="mapping">Permission role mapping.</param>
        void DeletePermissionRoleMapping(PermissionRoleMapping mapping);


        /// <summary>
        /// Installs permissions. Permissions are automatically installed by <see cref="InstallPermissionsStarter"/>.
        /// </summary>
        /// <param name="permissionProviders">Providers whose permissions are to be installed.</param>
        /// <param name="removeUnusedPermissions">Whether to remove permissions no longer supported by the providers.</param>
        void InstallPermissions(IPermissionProvider[] permissionProviders, bool removeUnusedPermissions = false);


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
        /// <param name="permissionSystemName">Permission record system name.</param>
        /// <returns><c>true</c> authorized otherwise <c>false</c>.</returns>
        bool Authorize(string permissionSystemName);

        /// <summary>
        /// Authorize permission.
        /// </summary>
        /// <param name="permissionSystemName">Permission record system name.</param>
        /// <param name="customer">Customer.</param>
        /// <returns><c>true</c> authorized otherwise <c>false</c>.</returns>
        bool Authorize(string permissionSystemName, Customer customer);

        /// <summary>
        /// Authorize permission by alias permission name. Required if granular permission migration has not yet run.
        /// Functional only if the old permission resources still exist in the database.
        /// </summary>
        /// <param name="permissionSystemName">Permission record system name.</param>
        /// <returns><c>true</c> authorized otherwise <c>false</c>.</returns>
        bool AuthorizeByAlias(string permissionSystemName);


        /// <summary>
        /// Search all child permissions for an authorization (initial permission included).
        /// </summary>
        /// <param name="permissionSystemName">Permission record system name.</param>
        /// <returns><c>true</c> authorization found otherwise <c>false</c>.</returns>
        bool FindAuthorization(string permissionSystemName);

        /// <summary>
        /// Search all child permissions for an authorization (initial permission included).
        /// </summary>
        /// <param name="permissionSystemName">Permission record system name.</param>
        /// <param name="customer">Customer.</param>
        /// <returns><c>true</c> authorization found otherwise <c>false</c>.</returns>
        bool FindAuthorization(string permissionSystemName, Customer customer);


        /// <summary>
        /// Gets the permission tree for a customer role.
        /// </summary>
        /// <param name="role">Customer role.</param>
        /// <param name="addDisplayNames">Whether to add the permission display names.</param>
        /// <returns>Permission tree.</returns>
        TreeNode<IPermissionNode> GetPermissionTree(CustomerRole role, bool addDisplayNames = false);

        /// <summary>
        /// Gets the permission tree for a customer.
        /// </summary>
        /// <param name="customer">Customer.</param>
        /// <param name="addDisplayNames">Whether to add the permission display names.</param>
        /// <returns>Permission tree.</returns>
        TreeNode<IPermissionNode> GetPermissionTree(Customer customer, bool addDisplayNames = false);

        /// <summary>
        /// Get display name for a permission system name.
        /// </summary>
        /// <param name="permissionSystemName">Permission record system name.</param>
        /// <returns>Display name.</returns>
        string GetDiplayName(string permissionSystemName);

        /// <summary>
        /// Get detailed unauthorization message.
        /// </summary>
        /// <param name="permissionSystemName">Permission record system name.</param>
        /// <returns>Detailed unauthorization message</returns>
        string GetUnauthorizedMessage(string permissionSystemName);
    }
}
