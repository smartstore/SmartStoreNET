using System.Collections.Generic;
using SmartStore.Core.Domain.Customers;

namespace SmartStore.Core.Domain.Security
{
    /// <summary>
    /// Represents a permission record.
    /// </summary>
    public class PermissionRecord : BaseEntity
    {
        private ICollection<CustomerRole> _customerRoles;
        private ICollection<PermissionRoleMapping> _permissionRoleMappings;

        /// <summary>
        /// Gets or sets the permission name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the permission system name.
        /// </summary>
        public string SystemName { get; set; }
        
        /// <summary>
        /// Gets or sets the permission category.
        /// </summary>
        public string Category { get; set; }

        //GP: remove.
        /// <summary>
        /// Gets or sets customer roles.
        /// </summary>
        public virtual ICollection<CustomerRole> CustomerRoles
        {
			get { return _customerRoles ?? (_customerRoles = new HashSet<CustomerRole>()); }
            protected set { _customerRoles = value; }
        }

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
