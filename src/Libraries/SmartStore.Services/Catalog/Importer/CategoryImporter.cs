using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using SmartStore.Core.Async;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.IO;
using SmartStore.Services.DataExchange.Import;
using SmartStore.Services.DataExchange.Import.Events;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Utilities;

namespace SmartStore.Services.Catalog.Importer
{
    public class CategoryImporter : EntityImporterBase
    {
        private readonly IRepository<Category> _categoryRepository;
        private readonly ICategoryTemplateService _categoryTemplateService;
        private readonly IMediaService _mediaService;
        private readonly IFolderService _folderService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly FileDownloadManager _fileDownloadManager;
        private readonly ICacheManager _cache;

        private static readonly Dictionary<string, Expression<Func<Category, string>>> _localizableProperties = new Dictionary<string, Expression<Func<Category, string>>>
        {
            { "Name", x => x.Name },
            { "FullName", x => x.FullName },
            { "Description", x => x.Description },
            { "BottomDescription", x => x.BottomDescription },
            { "MetaKeywords", x => x.MetaKeywords },
            { "MetaDescription", x => x.MetaDescription },
            { "MetaTitle", x => x.MetaTitle }
        };

        public CategoryImporter(
            IRepository<Category> categoryRepository,
            ICategoryTemplateService categoryTemplateService,
            IMediaService mediaService,
            IFolderService folderService,
            ILocalizedEntityService localizedEntityService,
            FileDownloadManager fileDownloadManager,
            ICacheManager cache)
        {
            _categoryRepository = categoryRepository;
            _categoryTemplateService = categoryTemplateService;
            _mediaService = mediaService;
            _folderService = folderService;
            _localizedEntityService = localizedEntityService;
            _fileDownloadManager = fileDownloadManager;
            _cache = cache;
        }

        protected override void Import(ImportExecuteContext context)
        {
            var srcToDestId = new Dictionary<int, ImportCategoryMapping>();

            var templateViewPaths = _categoryTemplateService.GetAllCategoryTemplates().ToDictionarySafe(x => x.ViewPath, x => x.Id);

            using (var scope = new DbContextScope(ctx: context.Services.DbContext, hooksEnabled: false, autoDetectChanges: false, proxyCreation: false, validateOnSave: false))
            {
                var segmenter = context.DataSegmenter;

                Initialize(context);

                while (context.Abort == DataExchangeAbortion.None && segmenter.ReadNextBatch())
                {
                    var batch = segmenter.GetCurrentBatch<Category>();

                    // Perf: detach all entities
                    _categoryRepository.Context.DetachAll(true);

                    context.SetProgress(segmenter.CurrentSegmentFirstRowIndex - 1, segmenter.TotalRows);

                    try
                    {
                        ProcessCategories(context, batch, templateViewPaths, srcToDestId);
                    }
                    catch (Exception ex)
                    {
                        context.Result.AddError(ex, segmenter.CurrentSegment, "ProcessCategories");
                    }

                    // Reduce batch to saved (valid) products.
                    // No need to perform import operations on errored products.
                    batch = batch.Where(x => x.Entity != null && !x.IsTransient).ToArray();

                    // Update result object.
                    context.Result.NewRecords += batch.Count(x => x.IsNew && !x.IsTransient);
                    context.Result.ModifiedRecords += batch.Count(x => !x.IsNew && !x.IsTransient);

                    // Process slugs.
                    if (segmenter.HasColumn("SeName", true) || batch.Any(x => x.IsNew || x.NameChanged))
                    {
                        try
                        {
                            _categoryRepository.Context.AutoDetectChangesEnabled = true;
                            ProcessSlugs(context, batch, typeof(Category).Name);
                        }
                        catch (Exception ex)
                        {
                            context.Result.AddError(ex, segmenter.CurrentSegment, "ProcessSlugs");
                        }
                        finally
                        {
                            _categoryRepository.Context.AutoDetectChangesEnabled = false;
                        }
                    }

                    // Process store mappings.
                    if (segmenter.HasColumn("StoreIds"))
                    {
                        try
                        {
                            ProcessStoreMappings(context, batch);
                        }
                        catch (Exception ex)
                        {
                            context.Result.AddError(ex, segmenter.CurrentSegment, "ProcessStoreMappings");
                        }
                    }

                    // Localizations.
                    try
                    {
                        ProcessLocalizations(context, batch, _localizableProperties);
                    }
                    catch (Exception ex)
                    {
                        context.Result.AddError(ex, segmenter.CurrentSegment, "ProcessLocalizedProperties");
                    }

                    // Process pictures.
                    if (segmenter.HasColumn("ImageUrl") && !segmenter.IsIgnored("PictureId"))
                    {
                        try
                        {
                            _categoryRepository.Context.AutoDetectChangesEnabled = true;
                            ProcessPictures(context, batch);
                        }
                        catch (Exception ex)
                        {
                            context.Result.AddError(ex, segmenter.CurrentSegment, "ProcessPictures");
                        }
                        finally
                        {
                            _categoryRepository.Context.AutoDetectChangesEnabled = false;
                        }
                    }

                    context.Services.EventPublisher.Publish(new ImportBatchExecutedEvent<Category>(context, batch));
                }

                // Map parent id of inserted categories.
                if (srcToDestId.Any() && segmenter.HasColumn("Id") && segmenter.HasColumn("ParentCategoryId") && !segmenter.IsIgnored("ParentCategoryId"))
                {
                    segmenter.Reset();

                    while (context.Abort == DataExchangeAbortion.None && segmenter.ReadNextBatch())
                    {
                        var batch = segmenter.GetCurrentBatch<Category>();
                        _categoryRepository.Context.DetachAll(unchangedEntitiesOnly: false);

                        try
                        {
                            ProcessParentMappings(context, batch, srcToDestId);
                        }
                        catch (Exception ex)
                        {
                            context.Result.AddError(ex, segmenter.CurrentSegment, "ProcessParentMappings");
                        }
                    }
                }
            }

            // Hooks are disabled but category tree may have changed.
            _cache.Clear();
        }

        protected virtual int ProcessPictures(ImportExecuteContext context, IEnumerable<ImportRow<Category>> batch)
        {
            var allFileIds = batch
                .Where(row => row.HasDataValue("ImageUrl") && row.Entity.MediaFileId > 0)
                .Select(row => row.Entity.MediaFileId.Value)
                .Distinct()
                .ToArray();

            var allFiles = _mediaService.GetFilesByIds(allFileIds).ToDictionary(x => x.Id, x => x.File);
            var catalogAlbumId = _folderService.GetNodeByPath(SystemAlbumProvider.Catalog).Value.Id;

            foreach (var row in batch)
            {
                try
                {
                    var imageUrl = row.GetDataValue<string>("ImageUrl");
                    if (imageUrl.IsEmpty())
                    {
                        continue;
                    }

                    var image = CreateDownloadImage(context, imageUrl, 1);
                    if (image == null)
                    {
                        continue;
                    }

                    if (image.Url.HasValue() && !image.Success.HasValue)
                    {
                        AsyncRunner.RunSync(() => _fileDownloadManager.DownloadAsync(DownloaderContext, new FileDownloadManagerItem[] { image }));
                    }

                    if ((image.Success ?? false) && File.Exists(image.Path))
                    {
                        Succeeded(image);
                        using (var stream = File.OpenRead(image.Path))
                        {
                            if ((stream?.Length ?? 0) > 0)
                            {
                                var assignedFile = allFiles.Get(row.Entity.MediaFileId ?? 0);
                                MediaFile sourceFile = null;

                                if (assignedFile != null && _mediaService.FindEqualFile(stream, new[] { assignedFile }, true, out var _))
                                {
                                    context.Result.AddInfo($"Found equal image in data store for '{image.FileName}'. Skipping file.", row.GetRowInfo(), "ImageUrl");
                                }
                                else if (_mediaService.FindEqualFile(stream, image.FileName, catalogAlbumId, true, out sourceFile))
                                {
                                    context.Result.AddInfo($"Found equal image in catalog album for '{image.FileName}'. Assigning existing file instead.", row.GetRowInfo(), "ImageUrl");
                                }
                                else
                                {
                                    var path = _mediaService.CombinePaths(SystemAlbumProvider.Catalog, image.FileName);
                                    sourceFile = _mediaService.SaveFile(path, stream, false, DuplicateFileHandling.Rename)?.File;
                                }

                                if (sourceFile?.Id > 0)
                                {
                                    row.Entity.MediaFileId = sourceFile.Id;
                                    _categoryRepository.Update(row.Entity);
                                }
                            }
                        }
                    }
                    else if (image.Url.HasValue())
                    {
                        context.Result.AddInfo("Download of an image failed.", row.GetRowInfo(), "ImageUrls");
                    }
                }
                catch (Exception ex)
                {
                    context.Result.AddWarning(ex.ToAllMessages(), row.GetRowInfo(), "ImageUrls");
                }
            }

            var num = _categoryRepository.Context.SaveChanges();
            return num;
        }

        protected virtual int ProcessLocalizations(
            ImportExecuteContext context,
            IEnumerable<ImportRow<Category>> batch,
            string[] localizedProperties)
        {
            if (localizedProperties.Length == 0)
            {
                return 0;
            }

            bool shouldSave = false;

            foreach (var row in batch)
            {
                foreach (var prop in localizedProperties)
                {
                    var lambda = _localizableProperties[prop];
                    foreach (var lang in context.Languages)
                    {
                        var code = lang.UniqueSeoCode;
                        string value;

                        if (row.TryGetDataValue(prop /* ColumnName */, code, out value))
                        {
                            _localizedEntityService.SaveLocalizedValue(row.Entity, lambda, value, lang.Id);
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

        protected virtual int ProcessParentMappings(
            ImportExecuteContext context,
            IEnumerable<ImportRow<Category>> batch,
            Dictionary<int, ImportCategoryMapping> srcToDestId)
        {
            foreach (var row in batch)
            {
                var id = row.GetDataValue<int>("Id");
                var rawParentId = row.GetDataValue<string>("ParentCategoryId");
                var parentId = rawParentId.ToInt(-1);

                if (id != 0 && parentId != -1 && srcToDestId.ContainsKey(id) && srcToDestId.ContainsKey(parentId))
                {
                    // only touch hierarchical data if child and parent were inserted
                    if (srcToDestId[id].Inserted && srcToDestId[parentId].Inserted && srcToDestId[id].DestinationId != 0)
                    {
                        var category = _categoryRepository.GetById(srcToDestId[id].DestinationId);
                        if (category != null)
                        {
                            category.ParentCategoryId = srcToDestId[parentId].DestinationId;

                            _categoryRepository.Update(category);
                        }
                    }
                }
            }

            var num = _categoryRepository.Context.SaveChanges();

            return num;
        }

        protected virtual int ProcessCategories(
            ImportExecuteContext context,
            IEnumerable<ImportRow<Category>> batch,
            Dictionary<string, int> templateViewPaths,
            Dictionary<int, ImportCategoryMapping> srcToDestId)
        {
            _categoryRepository.AutoCommitEnabled = true;

            var defaultTemplateId = templateViewPaths["CategoryTemplate.ProductsInGridOrLines"];
            var hasNameColumn = context.DataSegmenter.HasColumn("Name");

            foreach (var row in batch)
            {
                Category category = null;
                var id = row.GetDataValue<int>("Id");
                var name = row.GetDataValue<string>("Name");

                foreach (var keyName in context.KeyFieldNames)
                {
                    switch (keyName)
                    {
                        case "Id":
                            if (id != 0)
                                category = _categoryRepository.GetById(id);
                            break;
                        case "Name":
                            if (name.HasValue())
                                category = _categoryRepository.Table.FirstOrDefault(x => x.Name == name);
                            break;
                    }

                    if (category != null)
                        break;
                }

                if (category == null)
                {
                    if (context.UpdateOnly)
                    {
                        ++context.Result.SkippedRecords;
                        continue;
                    }

                    // A name is required for new categories.
                    if (!row.HasDataValue("Name"))
                    {
                        ++context.Result.SkippedRecords;
                        context.Result.AddError("The 'Name' field is required for new categories. Skipping row.", row.GetRowInfo(), "Name");
                        continue;
                    }

                    category = new Category();
                }

                row.Initialize(category, name ?? category.Name);

                if (!row.IsNew && hasNameColumn && !category.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    // Perf: use this later for SeName updates.
                    row.NameChanged = true;
                }

                row.SetProperty(context.Result, (x) => x.Name);
                row.SetProperty(context.Result, (x) => x.FullName);
                row.SetProperty(context.Result, (x) => x.Description);
                row.SetProperty(context.Result, (x) => x.BottomDescription);
                row.SetProperty(context.Result, (x) => x.MetaKeywords);
                row.SetProperty(context.Result, (x) => x.MetaDescription);
                row.SetProperty(context.Result, (x) => x.MetaTitle);
                row.SetProperty(context.Result, (x) => x.PageSize);
                row.SetProperty(context.Result, (x) => x.AllowCustomersToSelectPageSize);
                row.SetProperty(context.Result, (x) => x.PageSizeOptions);
                row.SetProperty(context.Result, (x) => x.ShowOnHomePage);
                row.SetProperty(context.Result, (x) => x.HasDiscountsApplied);
                row.SetProperty(context.Result, (x) => x.Published, true);
                row.SetProperty(context.Result, (x) => x.DisplayOrder);
                row.SetProperty(context.Result, (x) => x.Alias);
                row.SetProperty(context.Result, (x) => x.DefaultViewMode);
                // With new entities, "LimitedToStores" is an implicit field, meaning
                // it has to be set to true by code if it's absent but "StoreIds" exists.
                row.SetProperty(context.Result, (x) => x.LimitedToStores, !row.GetDataValue<List<int>>("StoreIds").IsNullOrEmpty());

                string tvp;
                if (row.TryGetDataValue("CategoryTemplateViewPath", out tvp, row.IsTransient))
                {
                    category.CategoryTemplateId = (tvp.HasValue() && templateViewPaths.ContainsKey(tvp) ? templateViewPaths[tvp] : defaultTemplateId);
                }

                if (id != 0 && !srcToDestId.ContainsKey(id))
                {
                    srcToDestId.Add(id, new ImportCategoryMapping { Inserted = row.IsTransient });
                }

                if (row.IsTransient)
                {
                    _categoryRepository.Insert(category);
                }
                else
                {
                    _categoryRepository.Update(category);
                }
            }

            // Commit whole batch at once.
            var num = _categoryRepository.Context.SaveChanges();

            // Get new category ids.
            foreach (var row in batch)
            {
                var id = row.GetDataValue<int>("Id");
                if (id != 0 && srcToDestId.ContainsKey(id))
                {
                    srcToDestId[id].DestinationId = row.Entity.Id;
                }
            }

            return num;
        }

        public static string[] SupportedKeyFields => new string[] { "Id", "Name" };

        public static string[] DefaultKeyFields => new string[] { "Name", "Id" };

        public class ImportCategoryMapping
        {
            public int DestinationId { get; set; }
            public bool Inserted { get; set; }
        }
    }

}
