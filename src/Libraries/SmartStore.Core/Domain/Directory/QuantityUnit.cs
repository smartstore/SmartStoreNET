using System.Runtime.Serialization;
using SmartStore.Core.Domain.Localization;

namespace SmartStore.Core.Domain.Directory
{
    /// <summary>
    /// Represents a quantity unit
    /// </summary>
	[DataContract]
    public partial class QuantityUnit : BaseEntity, ILocalizedEntity
    {
        /// <summary>
        /// Gets or sets the name
        /// </summary>
		[DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name plural.
        /// </summary>
		[DataMember]
        public string NamePlural { get; set; }

        /// <summary>
        /// Gets or sets the description
        /// </summary>
		[DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the display locale
        /// </summary>
		[DataMember]
        public string DisplayLocale { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
		[DataMember]
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets the default quantity unit
        /// </summary>
		[DataMember]
        public bool IsDefault { get; set; }
    }
}
