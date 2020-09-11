using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Security;

namespace SmartStore.Services.Security
{
    public partial class StandardPermissionProvider : IPermissionProvider
    {
        public virtual IEnumerable<PermissionRecord> GetPermissions()
        {
            var permissionSystemNames = PermissionHelper.GetPermissions(typeof(Permissions));
            var permissions = permissionSystemNames.Select(x => new PermissionRecord { SystemName = x });

            return permissions;
        }

        public virtual IEnumerable<DefaultPermissionRecord> GetDefaultPermissions()
        {
            return new[]
            {
                new DefaultPermissionRecord
                {
                    CustomerRoleSystemName = SystemCustomerRoleNames.Administrators,
                    PermissionRecords = new[]
                    {
                        new PermissionRecord { SystemName = Permissions.Catalog.Self },
                        new PermissionRecord { SystemName = Permissions.Customer.Self },
                        new PermissionRecord { SystemName = Permissions.Order.Self },
                        new PermissionRecord { SystemName = Permissions.Promotion.Self },
                        new PermissionRecord { SystemName = Permissions.Cms.Self },
                        new PermissionRecord { SystemName = Permissions.Configuration.Self },
                        new PermissionRecord { SystemName = Permissions.System.Self },
                        new PermissionRecord { SystemName = Permissions.Cart.Self },
                        new PermissionRecord { SystemName = Permissions.Media.Self }
                    }
                },
                new DefaultPermissionRecord
                {
                    CustomerRoleSystemName = SystemCustomerRoleNames.ForumModerators,
                    PermissionRecords = new[]
                    {
                        new PermissionRecord { SystemName = Permissions.Catalog.DisplayPrice },
                        new PermissionRecord { SystemName = Permissions.Cart.AccessShoppingCart },
                        new PermissionRecord { SystemName = Permissions.Cart.AccessWishlist },
                        new PermissionRecord { SystemName = Permissions.System.AccessShop }
                    }
                },
                new DefaultPermissionRecord
                {
                    CustomerRoleSystemName = SystemCustomerRoleNames.Guests,
                    PermissionRecords = new[]
                    {
                        new PermissionRecord { SystemName = Permissions.Catalog.DisplayPrice },
                        new PermissionRecord { SystemName = Permissions.Cart.AccessShoppingCart },
                        new PermissionRecord { SystemName = Permissions.Cart.AccessWishlist },
                        new PermissionRecord { SystemName = Permissions.System.AccessShop }
                    }
                },
                new DefaultPermissionRecord
                {
                    CustomerRoleSystemName = SystemCustomerRoleNames.Registered,
                    PermissionRecords = new[]
                    {
                        new PermissionRecord { SystemName = Permissions.Catalog.DisplayPrice },
                        new PermissionRecord { SystemName = Permissions.Cart.AccessShoppingCart },
                        new PermissionRecord { SystemName = Permissions.Cart.AccessWishlist },
                        new PermissionRecord { SystemName = Permissions.System.AccessShop }
                    }
                }
            };
        }
    }
}
