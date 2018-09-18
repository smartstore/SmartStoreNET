using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Linq;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.IO;
using SmartStore.Utilities;
using System.Linq.Expressions;
using SmartStore.Core;
using Autofac;
using SmartStore.Services.Localization;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Seo;
using SmartStore.Services.Seo;
using SmartStore.Core.Data;
using SmartStore.Services.Stores;
using SmartStore.Core.Domain.Stores;

namespace SmartStore.Services.DataExchange.Import
{
	public abstract class EntityImporterBase : IEntityImporter
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
					var extension = MimeTypes.MapMimeTypeToExtension(item.MimeType).NullEmpty() ?? ".jpg";

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

		public void Succeeded(FileDownloadManagerItem item)
		{
			if ((item.Success ?? false) && item.Url.HasValue() && !DownloadedItems.ContainsKey(item.Url))
			{
				DownloadedItems.Add(item.Url, Path.GetFileName(item.Path));
			}
		}

		protected virtual int ProcessLocalizations<TEntity>(
			ImportExecuteContext context,
			IEnumerable<ImportRow<TEntity>> batch,
			IDictionary<string, Expression<Func<TEntity, string>>> localizableProperties) where TEntity : BaseEntity, ILocalizedEntity
		{
			Guard.NotNull(context, nameof(context));
			Guard.NotNull(batch, nameof(batch));
			Guard.NotNull(localizableProperties, nameof(localizableProperties));

			// Perf: determine whether our localizable properties actually have 
			// counterparts in the source BEFORE import batch begins. This way we spare ourself
			// to query over and over for values.
			var localizedProps = (from kvp in localizableProperties
								  where context.DataSegmenter.GetColumnIndexes(kvp.Key).Length > 0
								  select kvp.Key).ToArray();

			if (localizedProps.Length == 0)
			{
				return 0;
			}

			var localizedEntityService = context.Services.Resolve<ILocalizedEntityService>();

			bool shouldSave = false;

			foreach (var row in batch)
			{
				foreach (var prop in localizedProps)
				{
					var lambda = localizableProperties[prop];
					foreach (var lang in context.Languages)
					{
						var code = lang.UniqueSeoCode;
						string value;

						if (row.TryGetDataValue(prop /* ColumnName */, code, out value))
						{
							localizedEntityService.SaveLocalizedValue(row.Entity, lambda, value, lang.Id);
							shouldSave = true;
						}
					}
				}
			}

			if (shouldSave)
			{
				// commit whole batch at once
				return context.Services.DbContext.SaveChanges();
			}

			return 0;
		}

		protected virtual int ProcessStoreMappings<TEntity>(
			ImportExecuteContext context,
			IEnumerable<ImportRow<TEntity>> batch) where TEntity : BaseEntity, IStoreMappingSupported
		{
			var storeMappingService = context.Services.Resolve<IStoreMappingService>();
			var storeMappingRepository = context.Services.Resolve<IRepository<StoreMapping>>();

			storeMappingRepository.AutoCommitEnabled = false;

			foreach (var row in batch)
			{
				var storeIds = row.GetDataValue<List<int>>("StoreIds");
				if (!storeIds.IsNullOrEmpty())
				{
					storeMappingService.SaveStoreMappings(row.Entity, storeIds.ToArray());
				}
			}

			// commit whole batch at once
			return context.Services.DbContext.SaveChanges();
		}

		protected virtual int ProcessSlugs<TEntity>(
			ImportExecuteContext context,
			IEnumerable<ImportRow<TEntity>> batch,
			string entityName) where TEntity : BaseEntity, ISlugSupported
		{
			var slugMap = new Dictionary<string, UrlRecord>();
			UrlRecord urlRecord = null;

			var urlRecordService = context.Services.Resolve<IUrlRecordService>();
			var urlRecordRepository = context.Services.Resolve<IRepository<UrlRecord>>();
			var seoSettings = context.Services.Resolve<SeoSettings>();

			Func<string, UrlRecord> slugLookup = ((s) =>
			{
				return (slugMap.ContainsKey(s) ? slugMap[s] : null);
			});

			foreach (var row in batch)
			{
				try
				{
					string seName = null;
					string localizedName = null;

					if (row.TryGetDataValue("SeName", out seName) || row.IsNew || row.NameChanged)
					{
						seName = row.Entity.ValidateSeName(seName, row.EntityDisplayName, true, urlRecordService, seoSettings, extraSlugLookup: slugLookup);

						if (row.IsNew)
						{
							// dont't bother validating SeName for new entities.
							urlRecord = new UrlRecord
							{
								EntityId = row.Entity.Id,
								EntityName = entityName,
								Slug = seName,
								LanguageId = 0,
								IsActive = true,
							};
							urlRecordRepository.Insert(urlRecord);
						}
						else
						{
							urlRecord = urlRecordService.SaveSlug(row.Entity, seName, 0);
						}

						if (urlRecord != null)
						{
							// a new record was inserted to the store: keep track of it for this batch.
							slugMap[seName] = urlRecord;
						}
					}

					// process localized SeNames
					foreach (var lang in context.Languages)
					{
						var hasSeName = row.TryGetDataValue("SeName", lang.UniqueSeoCode, out seName);
						var hasLocalizedName = row.TryGetDataValue("Name", lang.UniqueSeoCode, out localizedName);

						if (hasSeName || hasLocalizedName)
						{
							seName = row.Entity.ValidateSeName(seName, localizedName, false, urlRecordService, seoSettings, lang.Id, slugLookup);
							urlRecord = urlRecordService.SaveSlug(row.Entity, seName, lang.Id);
							if (urlRecord != null)
							{
								slugMap[seName] = urlRecord;
							}
						}
					}
				}
				catch (Exception exception)
				{
					context.Result.AddWarning(exception.Message, row.GetRowInfo(), "SeName");
				}
			}

			// commit whole batch at once
			return context.Services.DbContext.SaveChanges();
		}
	}
}
