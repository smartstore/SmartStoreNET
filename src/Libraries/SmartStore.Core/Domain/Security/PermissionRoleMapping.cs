using System.ComponentModel.DataAnnotations.Schema;
using SmartStore.Core.Domain.Customers;

namespace SmartStore.Core.Domain.Security
{
    /// <summary>
    /// Represents a permission to role mapping.
    /// </summary>
    [Table("PermissionRoleMapping")]
    public partial class PermissionRoleMapping : BaseEntity
    {
        /// <summary>
        /// Gets or sets whether the permission is granted.
        /// </summary>
        public bool Allow { get; set; }

        /// <summary>
        /// Gets or sets the permission record id.
        /// </summary>
        public int PermissionRecordId { get; set; }

        /// <summary>
        /// Gets or sets the permission record.
        /// </summary>
        [ForeignKey("PermissionRecordId")]
        public virtual PermissionRecord PermissionRecord { get; set; }

        /// <summary>
        /// Gets or sets the customer role identifier.
        /// </summary>
        public int CustomerRoleId { get; set; }

        /// <summary>
        /// Gets or sets the customer role.
        /// </summary>
        [ForeignKey("CustomerRoleId")]
        public virtual CustomerRole CustomerRole { get; set; }
    }
}
