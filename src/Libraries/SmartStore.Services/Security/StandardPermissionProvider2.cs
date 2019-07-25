using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Security;

namespace SmartStore.Services.Security
{
    public partial class StandardPermissionProvider2 : IPermissionProvider
    {
        public virtual IEnumerable<PermissionRecord> GetPermissions()
        {
            var permissions = new List<PermissionRecord>();
            GetPermissionsFor(typeof(PermissionSystemNames), permissions);

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

        private void GetPermissionsFor(Type type, List<PermissionRecord> result)
        {
            var permissions = type
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
                .Select(x => new PermissionRecord { SystemName = (string)x.GetRawConstantValue() });

            result.AddRange(permissions);

            var nestedTypes = type.GetNestedTypes(BindingFlags.Public | BindingFlags.Static);
            nestedTypes.Each(x => GetPermissionsFor(x, result));
        }
    }
}
