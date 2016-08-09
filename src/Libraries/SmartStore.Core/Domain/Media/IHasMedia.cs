namespace SmartStore.Core.Domain.Media
{
	public interface IHasMedia
	{
		/// <summary>
		/// Gets or sets the binary data identifier
		/// </summary>
		int? BinaryDataId { get; set; }

		/// <summary>
		/// Gets or sets the binary data
		/// </summary>
		BinaryData BinaryData { get; set; }
	}
}
