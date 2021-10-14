using System.Collections.Generic;
using System.Linq;

using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Security;

namespace QTRADO.WMAddOn.Security
{
    public static class WMAddOnPermissions
    {
        public const string Self = "WMAddOn";
        public const string Read = "WMAddOn.read";
        public const string Update = "WMAddOn.update";
        public const string Display = "WMAddOn.display";
        public const string Edit = "WMAddOn.edit";
    }


    public class WMAddOnPermissionProvider : IPermissionProvider
    {
        public IEnumerable<PermissionRecord> GetPermissions()
        {
            var permissionSystemNames = PermissionHelper.GetPermissions(typeof(WMAddOnPermissions));
            var permissions = permissionSystemNames.Select(x => new PermissionRecord { SystemName = x });

            return permissions;
        }

        public IEnumerable<DefaultPermissionRecord> GetDefaultPermissions()
        {
            return new[]
            {
                new DefaultPermissionRecord
                {
                    CustomerRoleSystemName = SystemCustomerRoleNames.Administrators,
                    PermissionRecords = new[]
                    {
                        new PermissionRecord { SystemName = WMAddOnPermissions.Self }
                    }
                },
                new DefaultPermissionRecord
                {
                    CustomerRoleSystemName = SystemCustomerRoleNames.ForumModerators,
                    PermissionRecords = new[]
                    {
                        new PermissionRecord { SystemName = WMAddOnPermissions.Display }
                    }
                },
                new DefaultPermissionRecord
                {
                    CustomerRoleSystemName = SystemCustomerRoleNames.Guests,
                    PermissionRecords = new[]
                    {
                        new PermissionRecord { SystemName = WMAddOnPermissions.Display }
                    }
                },
                new DefaultPermissionRecord
                {
                    CustomerRoleSystemName = SystemCustomerRoleNames.Registered,
                    PermissionRecords = new[]
                    {
                        new PermissionRecord { SystemName = WMAddOnPermissions.Display }
                    }
                }
            };
        }
    }
}