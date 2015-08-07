using System;

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
}
