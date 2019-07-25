using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Security;

namespace SmartStore.Services.Security
{
    public partial class StandardPermissionProvider2 : IPermissionProvider
    {
        public virtual IEnumerable<PermissionRecord> GetPermissions()
        {
            var permissionSystemNames = PermissionSystemNames.GetAll();
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
                        new PermissionRecord { SystemName = PermissionSystemNames.Catalog.Self },
                        new PermissionRecord { SystemName = PermissionSystemNames.Customer.Self },
                        new PermissionRecord { SystemName = PermissionSystemNames.Order.Self },
                        new PermissionRecord { SystemName = PermissionSystemNames.Promotion.Self },
                        new PermissionRecord { SystemName = PermissionSystemNames.Cms.Self },
                        new PermissionRecord { SystemName = PermissionSystemNames.Configuration.Self },
                        new PermissionRecord { SystemName = PermissionSystemNames.System.Self },
                        new PermissionRecord { SystemName = PermissionSystemNames.Cart.Self },
                        new PermissionRecord { SystemName = PermissionSystemNames.Media.Self }
                    }
                },
                new DefaultPermissionRecord
                {
                    CustomerRoleSystemName = SystemCustomerRoleNames.ForumModerators,
                    PermissionRecords = new[]
                    {
                        new PermissionRecord { SystemName = PermissionSystemNames.Catalog.DisplayPrice },
                        new PermissionRecord { SystemName = PermissionSystemNames.Cart.AccessShoppingCart },
                        new PermissionRecord { SystemName = PermissionSystemNames.Cart.AccessWishlist },
                        new PermissionRecord { SystemName = PermissionSystemNames.System.AccessShop }
                    }
                },
                new DefaultPermissionRecord
                {
                    CustomerRoleSystemName = SystemCustomerRoleNames.Guests,
                    PermissionRecords = new[]
                    {
                        new PermissionRecord { SystemName = PermissionSystemNames.Catalog.DisplayPrice },
                        new PermissionRecord { SystemName = PermissionSystemNames.Cart.AccessShoppingCart },
                        new PermissionRecord { SystemName = PermissionSystemNames.Cart.AccessWishlist },
                        new PermissionRecord { SystemName = PermissionSystemNames.System.AccessShop }
                    }
                },
                new DefaultPermissionRecord
                {
                    CustomerRoleSystemName = SystemCustomerRoleNames.Registered,
                    PermissionRecords = new[]
                    {
                        new PermissionRecord { SystemName = PermissionSystemNames.Catalog.DisplayPrice },
                        new PermissionRecord { SystemName = PermissionSystemNames.Cart.AccessShoppingCart },
                        new PermissionRecord { SystemName = PermissionSystemNames.Cart.AccessWishlist },
                        new PermissionRecord { SystemName = PermissionSystemNames.System.AccessShop }
                    }
                }
            };
        }
    }
}
