using System.Data.Entity;
using System.Linq;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Security;

namespace SmartStore.Data.Setup
{
    internal class PermissionMigrator
	{
		private readonly SmartObjectContext _ctx;
		private readonly DbSet<PermissionRecord> _permissionRecords;
        private readonly DbSet<PermissionRoleMapping> _permissionRoleMappings;
		private readonly IQueryable<CustomerRole> _customerRoles;

		public PermissionMigrator(SmartObjectContext ctx)
		{
			Guard.NotNull(ctx, nameof(ctx));

			_ctx = ctx;
			_permissionRecords = _ctx.Set<PermissionRecord>();
            _permissionRoleMappings = _ctx.Set<PermissionRoleMapping>();
			_customerRoles = _ctx.Set<CustomerRole>();
		}

		public void AddPermission(PermissionRecord permission, string[] rolesToMap)
		{
			Guard.NotNull(permission, nameof(permission));
			Guard.NotNull(rolesToMap, nameof(rolesToMap));

            if (permission.SystemName.IsEmpty())
            {
                return;
            }

			var permissionRecord = _permissionRecords.FirstOrDefault(x => x.SystemName == permission.SystemName);
			if (permissionRecord == null)
			{
				_permissionRecords.Add(permission);
				_ctx.SaveChanges();
			}

			permissionRecord = _permissionRecords.FirstOrDefault(x => x.SystemName == permission.SystemName);

			foreach (var roleName in rolesToMap)
			{
                var role = _customerRoles.FirstOrDefault(x => x.SystemName == roleName);
                if (role != null)
                {
                    if (!_permissionRoleMappings.Any(x => x.PermissionRecord.SystemName == permission.SystemName && x.CustomerRole.SystemName == roleName))
                    {
                        _permissionRoleMappings.Add(new PermissionRoleMapping
                        {
                            Allow = true,
                            CustomerRoleId = role.Id,
                            PermissionRecordId = permissionRecord.Id
                        });
                    }
                }
			}

			_ctx.SaveChanges();
		}
	}
}
