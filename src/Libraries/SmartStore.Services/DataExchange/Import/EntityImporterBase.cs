using System;
using System.IO;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.IO;
using SmartStore.Utilities;

namespace SmartStore.Services.DataExchange.Import
{
	public abstract class EntityImporterBase
	{
		public DateTime UtcNow { get; private set; }

		public string ImageDownloadFolder { get; private set; }

		public string ImageFolder { get; private set; }

		public FileDownloadManagerContext DownloaderContext { get; private set; }

		public void Init(IImportExecuteContext context, DataExchangeSettings dataExchangeSettings)
		{
			UtcNow = DateTime.UtcNow;
			ImageDownloadFolder = Path.Combine(context.ImportFolder, "Content\\DownloadedImages");

			if (dataExchangeSettings.ImageImportFolder.HasValue())
				ImageFolder = Path.Combine(context.ImportFolder, dataExchangeSettings.ImageImportFolder);
			else
				ImageFolder = context.ImportFolder;

			if (!System.IO.Directory.Exists(ImageDownloadFolder))
				System.IO.Directory.CreateDirectory(ImageDownloadFolder);

			DownloaderContext = new FileDownloadManagerContext
			{
				Timeout = TimeSpan.FromMinutes(dataExchangeSettings.ImageDownloadTimeout),
				Logger = context.Log,
				CancellationToken = context.CancellationToken
			};
		}

		public FileDownloadManagerItem CreateDownloadImage(string urlOrPath, string seoName, int displayOrder)
		{
			var image = new FileDownloadManagerItem
			{
				Id = displayOrder,
				DisplayOrder = displayOrder,
				MimeType = MimeTypes.MapNameToMimeType(urlOrPath)
			};

			if (image.MimeType.IsEmpty())
				image.MimeType = "image/jpeg";

			var extension = MimeTypes.MapMimeTypeToExtension(image.MimeType);

			if (extension.HasValue())
			{
				if (urlOrPath.IsWebUrl())
				{
					image.Url = urlOrPath;
					image.FileName = "{0}-{1}".FormatInvariant(image.Id, seoName).ToValidFileName();
					image.Path = Path.Combine(ImageDownloadFolder, image.FileName + extension.EnsureStartsWith("."));
				}
				else if (Path.IsPathRooted(urlOrPath))
				{
					image.Path = urlOrPath;
					image.Success = true;
				}
				else
				{
					image.Path = Path.Combine(ImageFolder, urlOrPath);
					image.Success = true;
				}

				return image;
			}

			return null;
		}
	}
}
