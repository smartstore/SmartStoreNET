
namespace SmartStore.Core.Domain.DataExchange
{
	public enum ExportEntityType
	{
		Product = 0,
		Category,
		Manufacturer,
		Customer,
		Order,
		NewsletterSubscriber
	}

	public enum ExportDeploymentType
	{
		FileSystem = 0,
		Email,
		Http,
		Ftp
	}

	public enum ExportDescriptionMergingType
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

	public enum ExportProjectionFieldType
	{
		Description = 0,
		Brand,
		PictureSize
	}
}
