using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.IO;
using SmartStore.Utilities;
using System.Linq.Expressions;
using SmartStore.Core;
using Autofac;
using SmartStore.Services.Localization;
using SmartStore.Core.Domain.Localization;

namespace SmartStore.Services.DataExchange.Import
{
	public abstract class EntityImporterBase<TEntity> : IEntityImporter where TEntity : BaseEntity
	{
		private const string _imageDownloadFolder = @"Content\DownloadedImages";

		public DateTime UtcNow
		{
			get;
			private set;
		}

		public Dictionary<string, string> DownloadedItems
		{
			get;
			private set;
		}

		public string ImageDownloadFolder
		{
			get;
			private set;
		}

		public string ImageFolder
		{
			get;
			private set;
		}

		public FileDownloadManagerContext DownloaderContext
		{
			get;
			private set;
		}

		public void Execute(ImportExecuteContext context)
		{
			Import(context);
		}

		protected abstract void Import(ImportExecuteContext context);

		protected void Initialize(ImportExecuteContext context)
		{
			UtcNow = DateTime.UtcNow;
			DownloadedItems = new Dictionary<string, string>();
			ImageDownloadFolder = Path.Combine(context.ImportFolder, _imageDownloadFolder);

			var settings = context.DataExchangeSettings;

			if (settings.ImageImportFolder.HasValue())
				ImageFolder = Path.Combine(context.ImportFolder, settings.ImageImportFolder);
			else
				ImageFolder = context.ImportFolder;

			if (!System.IO.Directory.Exists(ImageDownloadFolder))
				System.IO.Directory.CreateDirectory(ImageDownloadFolder);

			DownloaderContext = new FileDownloadManagerContext
			{
				Timeout = TimeSpan.FromMinutes(settings.ImageDownloadTimeout),
				Logger = context.Log,
				CancellationToken = context.CancellationToken
			};

			context.Result.TotalRecords = context.DataSegmenter.TotalRows;
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

		protected IEnumerable<string> ResolveLocalizedProperties(ImportDataSegmenter segmenter)
		{
			// Perf: determine whether our localizable properties actually have 
			// counterparts in the source BEFORE import begins. This way we spare ourself
			// to query over and over for values.
			var localizableProperties = GetLocalizableProperties();
			foreach (var kvp in localizableProperties)
			{
				if (segmenter.GetColumnIndexes(kvp.Key).Length > 0)
				{
					yield return kvp.Key;
				}
			}
		}

		protected abstract IDictionary<string, Expression<Func<TEntity, string>>> GetLocalizableProperties();
	}
}
