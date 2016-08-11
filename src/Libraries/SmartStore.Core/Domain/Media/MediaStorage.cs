namespace SmartStore.Core.Domain.Media
{
	public partial class MediaStorage : BaseEntity
	{
		/// <summary>
		/// Binary data
		/// </summary>
		public byte[] Data { get; set; }
	}
}
