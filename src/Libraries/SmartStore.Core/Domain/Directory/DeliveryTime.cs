using System;
using SmartStore.Core.Domain.Localization;
using System.Runtime.Serialization;

namespace SmartStore.Core.Domain.Directory
{
    /// <summary>
    /// Represents a currency
    /// </summary>
	[DataContract]
	public partial class DeliveryTime : BaseEntity, ILocalizedEntity
    {
        /// <summary>
        /// Gets or sets the name
        /// </summary>
		[DataMember]
		public string Name { get; set; }

        /// <summary>
        /// Gets or sets the hex value
        /// </summary>
		[DataMember]
		public string ColorHexValue { get; set; }

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
        /// Gets or sets the display order
        /// </summary>
		[DataMember]
        public bool? IsDefault { get; set; }
    }
}
