using System.Data.Entity;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Security;

namespace SmartStore.Data.Setup
{
	internal class PermissionMigrator
	{
		private readonly SmartObjectContext _ctx;
		private readonly DbSet<PermissionRecord> _permissionRecords;
		private readonly IQueryable<CustomerRole> _customerRoles;

		public PermissionMigrator(SmartObjectContext ctx)
		{
			Guard.NotNull(ctx, nameof(ctx));

			_ctx = ctx;
			_permissionRecords = _ctx.Set<PermissionRecord>();
			_customerRoles = _ctx.Set<CustomerRole>().Expand(x => x.PermissionRecords);
		}

		public void AddPermission(PermissionRecord permission, string[] rolesToMap)
		{
			Guard.NotNull(permission, nameof(permission));
			Guard.NotNull(rolesToMap, nameof(rolesToMap));

			if (permission.SystemName.IsEmpty())
				return;

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
				if (role != null && !role.PermissionRecords.Any(x => x.SystemName == permission.SystemName))
				{
					role.PermissionRecords.Add(permissionRecord);
				}
			}

			_ctx.SaveChanges();
		}
	}
}
