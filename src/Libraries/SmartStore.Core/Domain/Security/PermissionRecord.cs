using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

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
        [Index]
        public string SystemName { get; set; }

        /// <summary>
        /// Gets or sets permission role mappings.
        /// </summary>
        public virtual ICollection<PermissionRoleMapping> PermissionRoleMappings
        {
            get => _permissionRoleMappings ?? (_permissionRoleMappings = new HashSet<PermissionRoleMapping>());
            protected set => _permissionRoleMappings = value;
        }
    }
}
