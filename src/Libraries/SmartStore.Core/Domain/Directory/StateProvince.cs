
using System.Runtime.Serialization;
using SmartStore.Core.Domain.Localization;

namespace SmartStore.Core.Domain.Directory
{
    /// <summary>
    /// Represents a state/province
    /// </summary>
	[DataContract]
    public partial class StateProvince : BaseEntity, ILocalizedEntity
    {
        /// <summary>
        /// Gets or sets the country identifier
        /// </summary>
		[DataMember]
        public int CountryId { get; set; }

        /// <summary>
        /// Gets or sets the name
        /// </summary>
		[DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the abbreviation
        /// </summary>
		[DataMember]
        public string Abbreviation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is published
        /// </summary>
		[DataMember]
        public bool Published { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
		[DataMember]
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets the country
        /// </summary>
		[DataMember]
        public virtual Country Country { get; set; }
    }

}
