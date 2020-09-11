using SmartStore.Core.Domain.Customers;
namespace SmartStore.Core.Domain.Security
{
    /// <summary>
    /// Represents an ACL record
    /// </summary>
    public partial class AclRecord : BaseEntity
    {
        /// <summary>
        /// Gets or sets the entity identifier
        /// </summary>
        public int EntityId { get; set; }

        /// <summary>
        /// Gets or sets the entity name
        /// </summary>
        public string EntityName { get; set; }

        /// <summary>
        /// Gets or sets the customer role identifier
        /// </summary>
        public int CustomerRoleId { get; set; }

        /// <summary>
        /// Gets or sets the customer role
        /// </summary>
        public virtual CustomerRole CustomerRole { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entry is idle
        /// </summary>
        /// <remarks>
        /// An entry is idle when it's related entity has been soft-deleted
        /// </remarks>
        public bool IsIdle { get; set; }
    }
}
