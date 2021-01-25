using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Autofac;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.IO;
using SmartStore.Services.Localization;
using SmartStore.Services.Seo;
using SmartStore.Services.Stores;
using SmartStore.Utilities;

namespace SmartStore.Services.DataExchange.Import
{
    public abstract class EntityImporterBase : IEntityImporter
    {
        private const string IMAGE_DOWNLOAD_FOLDER = @"Content\DownloadedImages";

        public DateTime UtcNow
        {
            get;
            private set;
        }

        /// <summary>
        /// URL > file name. To avoid downloading image several times.
        /// </summary>
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
            ImageDownloadFolder = Path.Combine(context.ImportFolder, IMAGE_DOWNLOAD_FOLDER);

            var settings = context.DataExchangeSettings;

            ImageFolder = settings.ImageImportFolder.HasValue()
                ? Path.Combine(context.ImportFolder, settings.ImageImportFolder)
                : context.ImportFolder;

            if (!System.IO.Directory.Exists(ImageDownloadFolder))
            {
                System.IO.Directory.CreateDirectory(ImageDownloadFolder);
            }

            DownloaderContext = new FileDownloadManagerContext
            {
                Timeout = TimeSpan.FromMinutes(settings.ImageDownloadTimeout),
                Logger = context.Log,
                CancellationToken = context.CancellationToken
            };

            context.Result.TotalRecords = context.DataSegmenter.TotalRows;
        }

        public FileDownloadManagerItem CreateDownloadImage(ImportExecuteContext context, string urlOrPath, int displayOrder)
        {
            try
            {
                var item = new FileDownloadManagerItem
                {
                    Id = displayOrder,
                    DisplayOrder = displayOrder
                };

                if (urlOrPath.IsWebUrl())
                {
                    // We append quality to avoid importing of image duplicates.
                    item.Url = context.Services.WebHelper.ModifyQueryString(urlOrPath, "q=100", null);

                    if (DownloadedItems.ContainsKey(urlOrPath))
                    {
                        // URL has already been downloaded.
                        item.Success = true;
                        item.FileName = DownloadedItems[urlOrPath];
                    }
                    else
                    {
                        var localPath = string.Empty;

                        try
                        {
                            // Exclude query string parts!
                            localPath = new Uri(urlOrPath).LocalPath;
                        }
                        catch { }

                        item.FileName = Path.GetFileName(localPath).ToValidFileName().NullEmpty() ?? Path.GetRandomFileName();
                    }

                    item.Path = Path.Combine(ImageDownloadFolder, item.FileName);
                }
                else
                {
                    item.Success = true;
                    item.FileName = Path.GetFileName(urlOrPath).ToValidFileName().NullEmpty() ?? Path.GetRandomFileName();

                    item.Path = Path.IsPathRooted(urlOrPath)
                        ? urlOrPath
                        : Path.Combine(ImageFolder, urlOrPath);
                }

                item.MimeType = MimeTypes.MapNameToMimeType(item.FileName);

                return item;
            }
            catch
            {
                context.Result.AddWarning($"Failed to prepare image download for '{urlOrPath.NaIfEmpty()}'. Skipping file.");

                return null;
            }
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
            var seoSettings = context.Services.Resolve<SeoSettings>();

            UrlRecord slugLookup(string s)
            {
                return slugMap.ContainsKey(s) ? slugMap[s] : null;
            }

            foreach (var row in batch)
            {
                try
                {
                    string localizedName = null;

                    if (row.TryGetDataValue("SeName", out string seName) || row.IsNew || row.NameChanged)
                    {
                        seName = row.Entity.ValidateSeName(seName, row.EntityDisplayName, true, urlRecordService, seoSettings, extraSlugLookup: slugLookup);

                        if (row.IsNew)
                        {
                            // Don't bother validating SeName for new entities.
                            urlRecord = new UrlRecord
                            {
                                EntityId = row.Entity.Id,
                                EntityName = entityName,
                                Slug = seName,
                                LanguageId = 0,
                                IsActive = true,
                            };
                            urlRecordService.InsertUrlRecord(urlRecord);
                        }
                        else
                        {
                            urlRecord = urlRecordService.SaveSlug(row.Entity, seName, 0);
                        }

                        if (urlRecord != null)
                        {
                            // A new record was inserted to the store: keep track of it for this batch.
                            slugMap[seName] = urlRecord;
                        }
                    }

                    // Process localized SeNames.
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
                catch (Exception ex)
                {
                    context.Result.AddWarning(ex.Message, row.GetRowInfo(), "SeName");
                }
            }

            // Commit whole batch at once.
            return context.Services.DbContext.SaveChanges();
        }
    }
}
