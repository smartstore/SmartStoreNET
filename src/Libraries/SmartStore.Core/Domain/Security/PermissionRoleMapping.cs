using System.ComponentModel.DataAnnotations.Schema;
using SmartStore.Core.Domain.Customers;

namespace SmartStore.Core.Domain.Security
{
    [Table("PermissionRoleMapping")]
    public partial class PermissionRoleMapping : BaseEntity
    {
        public bool Allow { get; set; }

        public int PermissionRecordId { get; set; }

        [ForeignKey("PermissionRecordId")]
        public virtual PermissionRecord PermissionRecord { get; set; }

        public int CustomerRoleId { get; set; }
                
        [ForeignKey("CustomerRoleId")]
        public virtual CustomerRole CustomerRole { get; set; }
    }
}
