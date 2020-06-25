using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Security;

namespace SmartStore.DevTools.Security
{
    /// <summary>
    /// All permissions provided by this plugin. Recommended to use singular for names, <see cref="Permissions"/>.
    /// "devtools" is the root permission (by convention, doesn't contain any dot). Localization key is "Plugins.Permissions.DisplayName.DevTools".
    /// "devtools.read" and "devtools.update" do not need localization because they are contained in core, <see cref="PermissionService._displayNameResourceKeys"/>.
    /// </summary>
    public static class DevToolsPermissions
    {
        public const string Self = "devtools";
        public const string Read = "devtools.read";
        public const string Update = "devtools.update";
    }


    public class DevToolsPermissionProvider : IPermissionProvider
    {
        public IEnumerable<PermissionRecord> GetPermissions()
        {
            // Get all permissions from above static class.
            var permissionSystemNames = PermissionHelper.GetPermissions(typeof(DevToolsPermissions));
            var permissions = permissionSystemNames.Select(x => new PermissionRecord { SystemName = x });

            return permissions;
        }

        public IEnumerable<DefaultPermissionRecord> GetDefaultPermissions()
        {
            // Allow root permission for admin by default.
            return new[]
            {
                new DefaultPermissionRecord
                {
                    CustomerRoleSystemName = SystemCustomerRoleNames.Administrators,
                    PermissionRecords = new[]
                    {
                        new PermissionRecord { SystemName = DevToolsPermissions.Self }
                    }
                }
            };
        }
    }
}