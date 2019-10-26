using System.Collections.Generic;

namespace SmartStore.Core.Domain.Security
{
    /// <summary>
    /// Represents a permission record.
    /// </summary>
    public class PermissionRecord : BaseEntity
    {
        private ICollection<PermissionRoleMapping> _permissionRoleMappings;

        /// <summary>
        /// Gets or sets the permission system name.
        /// </summary>
        public string SystemName { get; set; }
        
        /// <summary>
        /// Gets or sets permission role mappings.
        /// </summary>
        public virtual ICollection<PermissionRoleMapping> PermissionRoleMappings
        {
            get { return _permissionRoleMappings ?? (_permissionRoleMappings = new HashSet<PermissionRoleMapping>()); }
            protected set { _permissionRoleMappings = value; }
        }
    }
}
