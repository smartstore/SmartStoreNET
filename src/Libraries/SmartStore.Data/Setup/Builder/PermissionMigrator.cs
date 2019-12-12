using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Security;

namespace SmartStore.Data.Setup
{
    internal class PermissionMigrator
	{
		private readonly SmartObjectContext _ctx;
		private readonly DbSet<PermissionRecord> _permissionRecords;
        private readonly DbSet<PermissionRoleMapping> _permissionRoleMappings;

		public PermissionMigrator(SmartObjectContext ctx)
		{
			Guard.NotNull(ctx, nameof(ctx));

			_ctx = ctx;
			_permissionRecords = _ctx.Set<PermissionRecord>();
            _permissionRoleMappings = _ctx.Set<PermissionRoleMapping>();
		}

        /// <summary>
        /// Adds a permission record.
        /// </summary>
        /// <param name="permissionSystemName">Permission systemname.</param>
        /// <param name="roles">Optional. Roles to which the permission is to be granted.</param>
        /// <returns>Added permission. <c>null</c> otherwise.</returns>
        public PermissionRecord AddPermission(string permissionSystemName, params CustomerRole[] roles)
        {
            if (permissionSystemName.IsEmpty())
            {
                return null;
            }

            if (_permissionRecords.Any(x => x.SystemName == permissionSystemName))
            {
                return null;
            }

            var permission = new PermissionRecord { SystemName = permissionSystemName };

            _permissionRecords.Add(permission);
            _ctx.SaveChanges();

            Allow(permission, true, roles);

            return permission;
        }

        /// <summary>
        /// Adds all permissions of a plugin.
        /// </summary>
        /// <param name="pluginTypeName">The type name of static plugin permission names, e.g. "SmartStore.DevTools.Security.DevToolsPermissions, SmartStore.DevTools"</param>
        /// <returns>List of added permissions.</returns>
        public IList<PermissionRecord> AddPluginPermissions(string pluginTypeName)
        {
            var result = new List<PermissionRecord>();

            if (pluginTypeName.IsEmpty())
            {
                return result;
            }

            var assemblyName = Type.GetType(pluginTypeName)?.AssemblyQualifiedName;
            if (assemblyName.IsEmpty())
            {
                throw new SmartException($"Plugin permission type not found ({pluginTypeName}).");
            }

            var type = Type.GetType(assemblyName);
            var systemNames = PermissionHelper.GetPermissions(type);

            foreach (var systemName in systemNames)
            {
                var permission = AddPermission(systemName);
                if (permission != null)
                {
                    result.Add(permission);
                }
            }

            return result;
        }

        /// <summary>
        /// Adds permission role mappings.
        /// </summary>
        /// <param name="permission">Permission record.</param>
        /// <param name="allow">Whether the permission is to be granted or not.</param>
        /// <param name="roles">Mapped customer roles.</param>
        /// <returns>Number of added mappings.</returns>
        public int Allow(PermissionRecord permission, bool allow, params CustomerRole[] roles)
        {
            var num = 0;

            if ((permission?.Id ?? 0) != 0 && (roles?.Any() ?? false))
            {
                foreach (var role in roles)
                {
                    if (!_permissionRoleMappings.Any(x => x.PermissionRecordId == permission.Id && x.CustomerRoleId == role.Id))
                    {
                        _permissionRoleMappings.Add(new PermissionRoleMapping
                        {
                            Allow = allow,
                            PermissionRecordId = permission.Id,
                            CustomerRoleId = role.Id
                        });
                    }
                }

                num = _ctx.SaveChanges();
            }

            return num;
        }
    }
}
