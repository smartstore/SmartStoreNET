using System.Runtime.Serialization;

namespace SmartStore.Core.Domain.Localization
{
    /// <summary>
    /// Represents a localized property
    /// </summary>
	[DataContract]
    public partial class LocalizedProperty : BaseEntity
    {
        /// <summary>
        /// Gets or sets the entity identifier
        /// </summary>
		[DataMember]
		public int EntityId { get; set; }

        /// <summary>
        /// Gets or sets the language identifier
        /// </summary>
		[DataMember]
		public int LanguageId { get; set; }

        /// <summary>
        /// Gets or sets the locale key group
        /// </summary>
		[DataMember]
		public string LocaleKeyGroup { get; set; }

        /// <summary>
        /// Gets or sets the locale key
        /// </summary>
		[DataMember]
		public string LocaleKey { get; set; }

        /// <summary>
        /// Gets or sets the locale value
        /// </summary>
		[DataMember]
		public string LocaleValue { get; set; }
        
        /// <summary>
        /// Gets the language
        /// </summary>
		[DataMember]
		public virtual Language Language { get; set; }
    }
}
