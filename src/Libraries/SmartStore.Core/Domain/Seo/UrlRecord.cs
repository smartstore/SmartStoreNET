using System.Runtime.Serialization;

namespace SmartStore.Core.Domain.Seo
{
    /// <summary>
    /// Represents an URL record
    /// </summary>
	[DataContract]
	public partial class UrlRecord : BaseEntity
    {
        /// <summary>
        /// Gets or sets the entity identifier
        /// </summary>
		[DataMember]
		public int EntityId { get; set; }

        /// <summary>
        /// Gets or sets the entity name
        /// </summary>
		[DataMember]
		public string EntityName { get; set; }

        /// <summary>
        /// Gets or sets the slug
        /// </summary>
		[DataMember]
		public string Slug { get; set; }

        /// <summary>
	    /// Gets or sets the value indicating whether the record is active
	    /// </summary>
		[DataMember]
		public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the language identifier
        /// </summary>
		[DataMember]
		public int LanguageId { get; set; }
    }
}
