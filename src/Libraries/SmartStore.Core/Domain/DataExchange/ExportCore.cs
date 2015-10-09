
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
		NewsLetterSubscription
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
	/// Controls data processing and projection items supported by export provider
	/// </summary>
	public enum ExportSupport
	{
		/// <summary>
		/// Whether to automatically create a file based public deployment when an export profile is created
		/// </summary>
		CreateInitialPublicDeployment = 0,

		/// <summary>
		/// Whether to offer option to include\exclude grouped products
		/// </summary>
		ProjectionNoGroupedProducts,

		/// <summary>
		/// Whether to offer option to export attribute combinations as products
		/// </summary>
		ProjectionAttributeCombinationAsProduct,

		/// <summary>
		/// Whether to offer further options to manipulate the product description
		/// </summary>
		ProjectionDescription,

		/// <summary>
		/// Whether to offer option to enter a brand fallback
		/// </summary>
		ProjectionBrand,

		/// <summary>
		/// Whether to offer option to set a picture size and to get the URL of the main image
		/// </summary>
		ProjectionMainPictureUrl,

		/// <summary>
		/// Whether to use SKU as manufacturer part number if MPN is empty
		/// </summary>
		ProjectionUseOwnProductNo,
		
		/// <summary>
		/// Whether to offer option to enter a shipping time fallback
		/// </summary>
		ProjectionShippingTime,

		/// <summary>
		/// Whether to offer option to enter a shipping costs fallback and a free shipping threshold
		/// </summary>
		ProjectionShippingCosts,

		/// <summary>
		/// Whether to get the calculated old product price
		/// </summary>
		ProjectionOldPrice,

		/// <summary>
		/// Whether to get the calculated special and regular (ignoring special offers) price
		/// </summary>
		ProjectionSpecialPrice
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

	/// <summary>
	/// Export abortion types
	/// </summary>
	public enum ExportAbortion
	{
		/// <summary>
		/// No abortion. Go on with processing.
		/// </summary>
		None = 0,

		/// <summary>
		/// Break item processing but not the rest of the execution. Typically used for demo limitations.
		/// </summary>
		Soft,

		/// <summary>
		/// Break processing immediately
		/// </summary>
		Hard
	}
}
