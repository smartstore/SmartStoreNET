using System.Runtime.Serialization;

namespace SmartStore.Core.Domain.Tax
{
    /// <summary>
    /// Represents a tax category
    /// </summary>
    [DataContract]
    public partial class TaxCategory : BaseEntity
    {
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        [DataMember]
        public int DisplayOrder { get; set; }
    }

}
