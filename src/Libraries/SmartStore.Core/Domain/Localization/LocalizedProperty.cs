using System.ComponentModel.DataAnnotations.Schema;
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
		[Index("IX_LocalizedProperty_Compound", Order = 1)]
		public int EntityId { get; set; }

        /// <summary>
        /// Gets or sets the language identifier
        /// </summary>
		[DataMember]
		[Index("IX_LocalizedProperty_Compound", Order = 4)]
		public int LanguageId { get; set; }

        /// <summary>
        /// Gets or sets the locale key group
        /// </summary>
		[DataMember]
		[Index("IX_LocalizedProperty_Compound", Order = 3)]
		public string LocaleKeyGroup { get; set; }

        /// <summary>
        /// Gets or sets the locale key
        /// </summary>
		[DataMember]
		[Index("IX_LocalizedProperty_Compound", Order = 2)]
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
