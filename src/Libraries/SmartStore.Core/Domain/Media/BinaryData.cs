namespace SmartStore.Core.Domain.Media
{
	public partial class BinaryData : BaseEntity
	{
		/// <summary>
		/// Binary data
		/// </summary>
		public byte[] Data { get; set; }
	}
}
