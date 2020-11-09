using System.Runtime.Serialization;
using SmartStore.Core.Domain.Common;

namespace SmartStore.Core.Domain.Affiliates
{
    /// <summary>
    /// Represents an affiliate
    /// </summary>
    [DataContract]
    public partial class Affiliate : BaseEntity, ISoftDeletable
    {

        [DataMember]
        public int AddressId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity has been deleted
        /// </summary>
        [DataMember]
        public bool Deleted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is active
        /// </summary>
        [DataMember]
        public bool Active { get; set; }

        public virtual Address Address { get; set; }

    }
}
