using System.Runtime.Serialization;

namespace SmartStore.Core.Domain.Directory
{
	/// <summary>
	/// Represents a measure dimension
	/// </summary>
	[DataContract]
	public partial class MeasureDimension : BaseEntity
    {
		/// <summary>
		/// Gets or sets the name
		/// </summary>
		[DataMember]
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the system keyword
		/// </summary>
		[DataMember]
		public string SystemKeyword { get; set; }

		/// <summary>
		/// Gets or sets the ratio
		/// </summary>
		[DataMember]
		public decimal Ratio { get; set; }

		/// <summary>
		/// Gets or sets the display order
		/// </summary>
		[DataMember]
		public int DisplayOrder { get; set; }
    }
}
