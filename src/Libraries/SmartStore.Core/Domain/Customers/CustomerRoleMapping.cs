using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace SmartStore.Core.Domain.Customers
{
    /// <summary>
    /// Represents a customer to customer role mapping.
    /// </summary>
    [DataContract]
    [Table("CustomerRoleMapping")]
    public partial class CustomerRoleMapping : BaseEntity
    {
        /// <summary>
        /// Gets or sets the customer identifier.
        /// </summary>
        [DataMember]
        public int CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the customer role identifier.
        /// </summary>
        [DataMember]
        public int CustomerRoleId { get; set; }

        /// <summary>
        /// Indicates whether the mapping is created by the user or by the system.
        /// </summary>
        [DataMember]
        [Index]
        public bool IsSystemMapping { get; set; }

        /// <summary>
        /// Gets or sets the customer.
        /// </summary>
        [DataMember]
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }

        /// <summary>
        /// Gets or sets the customer role.
        /// </summary>
        [DataMember]
        [ForeignKey("CustomerRoleId")]
        public virtual CustomerRole CustomerRole { get; set; }
    }
}
