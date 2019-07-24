using System.Collections.Generic;
using SmartStore.Core.Domain.Security;

namespace SmartStore.Services.Security
{
    public partial class StandardPermissionProvider2 : IPermissionProvider
    {
        public virtual IEnumerable<PermissionRecord> GetPermissions()
        {
            return new PermissionRecord[] { };
        }

        public virtual IEnumerable<DefaultPermissionRecord> GetDefaultPermissions()
        {
            return new DefaultPermissionRecord[] { };
        }
    }
}
