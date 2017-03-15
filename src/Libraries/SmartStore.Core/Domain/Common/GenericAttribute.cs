using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
namespace SmartStore.Core.Domain.Common
{
    /// <summary>
    /// Represents a generic attribute
    /// </summary>
	[DataContract]
	public partial class GenericAttribute : BaseEntity
    {
        /// <summary>
        /// Gets or sets the entity identifier
        /// </summary>
		[DataMember]
		public int EntityId { get; set; }
        
        /// <summary>
        /// Gets or sets the key group
        /// </summary>
		[DataMember]
		public string KeyGroup { get; set; }

        /// <summary>
        /// Gets or sets the key
        /// </summary>
		[DataMember, Index("IX_GenericAttribute_Key")]
		public string Key { get; set; }

        /// <summary>
        /// Gets or sets the value
        /// </summary>
		[DataMember]
		public string Value { get; set; }

		/// <summary>
		/// Gets or sets the store identifier
		/// </summary>
		[DataMember]
		public int StoreId { get; set; }
    }
}
