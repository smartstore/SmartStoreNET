
namespace SmartStore.Core.Domain.DataExchange
{
	/// <summary>
	/// Supported entity types
	/// </summary>
	public enum ExportEntityType
	{
		Product = 0,
		Category,
		Manufacturer,
		Customer,
		Order,
		NewsletterSubscriber
	}

	/// <summary>
	/// Supported deployment types
	/// </summary>
	public enum ExportDeploymentType
	{
		FileSystem = 0,
		Email,
		Http,
		Ftp
	}

	/// <summary>
	/// Supported HTTP transmission types
	/// </summary>
	public enum ExportHttpTransmissionType
	{
		SimplePost = 0,
		MultipartFormDataPost
	}

	/// <summary>
	/// Controls the merging of various data as product description
	/// </summary>
	public enum ExportDescriptionMerging
	{
		None = 0,
		ShortDescriptionOrNameIfEmpty,
		ShortDescription,
		Description,
		NameAndShortDescription,
		NameAndDescription,
		ManufacturerAndNameAndShortDescription,
		ManufacturerAndNameAndDescription
	}

	/// <summary>
	/// Controls the merging of various data while exporting attribute combinations as products
	/// </summary>
	public enum ExportAttributeValueMerging
	{
		None = 0,
		AppendAllValuesToName
	}

	/// <summary>
	/// Projection types supported by export provider
	/// </summary>
	public enum ExportProjectionSupport
	{
		Description = 0,
		Brand,
		MainPictureUrl,
		UseOwnProductNo,
		ShippingTime,
		ShippingCosts,
		AttributeCombinationAsProduct,
		OldPrice,
		SpecialPrice
	}

	/// <summary>
	/// Possible order status change after order exporting
	/// </summary>
	public enum ExportOrderStatusChange
	{
		None = 0,
		Processing,
		Complete
	}
}
