using System;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Stores;
using System.Runtime.Serialization;

namespace SmartStore.Core.Domain.Directory
{
    /// <summary>
    /// Represents a currency
    /// </summary>
	[DataContract]
	public partial class Currency : BaseEntity, ILocalizedEntity, IStoreMappingSupported
    {
        /// <summary>
        /// Gets or sets the name
        /// </summary>
		[DataMember]
		public string Name { get; set; }

        /// <summary>
        /// Gets or sets the currency code
        /// </summary>
		[DataMember]
		public string CurrencyCode { get; set; }

        /// <summary>
        /// Gets or sets the rate
        /// </summary>
		[DataMember]
		public decimal Rate { get; set; }

        /// <summary>
        /// Gets or sets the display locale
        /// </summary>
		[DataMember]
		public string DisplayLocale { get; set; }

        /// <summary>
        /// Gets or sets the custom formatting
        /// </summary>
		[DataMember]
		public string CustomFormatting { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the entity is limited/restricted to certain stores
		/// </summary>
		[DataMember]
		public bool LimitedToStores { get; set; }

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
        /// Gets or sets the date and time of instance creation
        /// </summary>
		[DataMember]
		public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance update
        /// </summary>
		[DataMember]
		public DateTime UpdatedOnUtc { get; set; }

		/// <summary>
		/// Gets or sets the (comma separated) list of domain endings (e.g. country code top-level domains) to which this currency is the default one
		/// </summary>
		[DataMember]
		public string DomainEndings { get; set; }
    }
}
