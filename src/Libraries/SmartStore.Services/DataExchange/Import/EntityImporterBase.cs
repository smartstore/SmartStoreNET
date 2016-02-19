using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.IO;
using SmartStore.Utilities;

namespace SmartStore.Services.DataExchange.Import
{
	public abstract class EntityImporterBase
	{
		private const string _imageDownloadFolder = @"Content\DownloadedImages";

		public DateTime UtcNow { get; private set; }

		public Dictionary<string, string> DownloadedItems { get; private set; }

		public string ImageDownloadFolder { get; private set; }

		public string ImageFolder { get; private set; }

		public FileDownloadManagerContext DownloaderContext { get; private set; }

		public void Init(IImportExecuteContext context, DataExchangeSettings dataExchangeSettings)
		{
			UtcNow = DateTime.UtcNow;
			DownloadedItems = new Dictionary<string, string>();
			ImageDownloadFolder = Path.Combine(context.ImportFolder, _imageDownloadFolder);

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
			var item = new FileDownloadManagerItem
			{
				Id = displayOrder,
				DisplayOrder = displayOrder,
				MimeType = MimeTypes.MapNameToMimeType(urlOrPath)
			};

			if (item.MimeType.IsEmpty())
			{
				item.MimeType = MediaTypeNames.Image.Jpeg;
			}

			var extension = MimeTypes.MapMimeTypeToExtension(item.MimeType);

			if (extension.HasValue())
			{
				if (urlOrPath.IsWebUrl())
				{
					item.Url = urlOrPath;
					item.FileName = "{0}-{1}".FormatInvariant(seoName, item.Id).ToValidFileName();

					if (DownloadedItems.ContainsKey(urlOrPath))
					{
						item.Path = Path.Combine(ImageDownloadFolder, DownloadedItems[urlOrPath]);
						item.Success = true;
					}
					else
					{
						item.Path = Path.Combine(ImageDownloadFolder, item.FileName + extension.EnsureStartsWith("."));
					}
				}
				else if (Path.IsPathRooted(urlOrPath))
				{
					item.Path = urlOrPath;
					item.Success = true;
				}
				else
				{
					item.Path = Path.Combine(ImageFolder, urlOrPath);
					item.Success = true;
				}

				return item;
			}

			return null;
		}

		public void Succeeded(FileDownloadManagerItem item)
		{
			if ((item.Success ?? false) && item.Url.HasValue() && !DownloadedItems.ContainsKey(item.Url))
			{
				DownloadedItems.Add(item.Url, Path.GetFileName(item.Path));
			}
		}
	}
}
