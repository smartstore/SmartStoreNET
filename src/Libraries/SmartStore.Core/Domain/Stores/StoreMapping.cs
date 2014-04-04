using System.Runtime.Serialization;

namespace SmartStore.Core.Domain.Stores
{
	/// <summary>
	/// Represents a store mapping record
	/// </summary>
	[DataContract]
	public partial class StoreMapping : BaseEntity
	{
		/// <summary>
		/// Gets or sets the entity identifier
		/// </summary>
		[DataMember]
		public virtual int EntityId { get; set; }

		/// <summary>
		/// Gets or sets the entity name
		/// </summary>
		[DataMember]
		public virtual string EntityName { get; set; }

		/// <summary>
		/// Gets or sets the store identifier
		/// </summary>
		[DataMember]
		public virtual int StoreId { get; set; }
	}
}
