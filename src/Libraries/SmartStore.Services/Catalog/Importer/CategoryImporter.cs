using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Events;
using SmartStore.Services.DataExchange.Import;
using SmartStore.Services.Seo;
using SmartStore.Services.Stores;

namespace SmartStore.Services.Catalog.Importer
{
	public class CategoryImporter : IEntityImporter
	{
		private readonly IRepository<Category> _categoryRepository;
		private readonly IRepository<UrlRecord> _urlRecordRepository;
		private readonly IRepository<Picture> _pictureRepository;
		private readonly ICommonServices _services;
		private readonly ICategoryService _categoryService;
		private readonly IUrlRecordService _urlRecordService;
		private readonly ICategoryTemplateService _categoryTemplateService;
		private readonly IStoreMappingService _storeMappingService;
		private readonly SeoSettings _seoSettings;

		public CategoryImporter(
			IRepository<Category> categoryRepository,
			IRepository<UrlRecord> urlRecordRepository,
			IRepository<Picture> pictureRepository,
			ICommonServices services,
			ICategoryService categoryService,
			IUrlRecordService urlRecordService,
			ICategoryTemplateService categoryTemplateService,
			IStoreMappingService storeMappingService,
			SeoSettings seoSettings)
		{
			_categoryRepository = categoryRepository;
			_urlRecordRepository = urlRecordRepository;
			_pictureRepository = pictureRepository;
			_services = services;
			_categoryService = categoryService;
			_urlRecordService = urlRecordService;
			_categoryTemplateService = categoryTemplateService;
			_storeMappingService = storeMappingService;
			_seoSettings = seoSettings;
		}

		private int ProcessSlugs(ImportExecuteContext context, ImportRow<Category>[] batch)
		{
			var slugMap = new Dictionary<string, UrlRecord>(100);
			Func<string, UrlRecord> slugLookup = ((s) =>
			{
				if (slugMap.ContainsKey(s))
				{
					return slugMap[s];
				}
				return (UrlRecord)null;
			});

			var entityName = typeof(Category).Name;

			foreach (var row in batch)
			{
				if (row.IsNew || row.NameChanged || row.Segmenter.HasDataColumn("SeName"))
				{
					try
					{
						string seName = row.GetDataValue<string>("SeName");
						seName = row.Entity.ValidateSeName(seName, row.Entity.Name, true, _urlRecordService, _seoSettings, extraSlugLookup: slugLookup);

						UrlRecord urlRecord = null;

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
							_urlRecordRepository.Insert(urlRecord);
						}
						else
						{
							urlRecord = _urlRecordService.SaveSlug(row.Entity, seName, 0);
						}

						if (urlRecord != null)
						{
							// a new record was inserted to the store: keep track of it for this batch.
							slugMap[seName] = urlRecord;
						}
					}
					catch (Exception exception)
					{
						context.Result.AddWarning(exception.Message, row.GetRowInfo(), "SeName");
					}
				}
			}

			// commit whole batch at once
			return _urlRecordRepository.Context.SaveChanges();
		}

		private int ProcessCategories(ImportExecuteContext context,
			ImportRow<Category>[] batch,
			DateTime utcNow,
			List<int> allCategoryTemplateIds)
		{
			_categoryRepository.AutoCommitEnabled = true;

			Category lastInserted = null;
			Category lastUpdated = null;
			var defaultTemplateId = allCategoryTemplateIds.First();

			foreach (var row in batch)
			{
				Category category = null;
				object key;

				// try get by int ID
				if (row.DataRow.TryGetValue("Id", out key) && key.ToString().ToInt() > 0)
				{
					category = _categoryService.GetCategoryById(key.ToString().ToInt());
				}

				if (category == null)
				{
					// a Name is required with new categories
					if (!row.Segmenter.HasDataColumn("Name"))
					{
						context.Result.AddError("The 'Name' field is required for new categories. Skipping row.", row.GetRowInfo(), "Name");
						continue;
					}
					category = new Category
					{
						CategoryTemplateId = defaultTemplateId
					};
				}

				var name = row.GetDataValue<string>("Name");

				row.Initialize(category, name);

				if (!row.IsNew && !category.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
				{
					// Perf: use this later for SeName updates.
					row.NameChanged = true;
				}

				row.SetProperty(context.Result, category, (x) => x.Name);
				row.SetProperty(context.Result, category, (x) => x.FullName);
				row.SetProperty(context.Result, category, (x) => x.Description);
				row.SetProperty(context.Result, category, (x) => x.BottomDescription);
				row.SetProperty(context.Result, category, (x) => x.MetaKeywords);
				row.SetProperty(context.Result, category, (x) => x.MetaDescription);
				row.SetProperty(context.Result, category, (x) => x.MetaTitle);
				row.SetProperty(context.Result, category, (x) => x.ParentCategoryId);   // TODO: hierachical data in batch processing?
				row.SetProperty(context.Result, category, (x) => x.PageSize, 12);
				row.SetProperty(context.Result, category, (x) => x.AllowCustomersToSelectPageSize, true);
				row.SetProperty(context.Result, category, (x) => x.PageSizeOptions);
				row.SetProperty(context.Result, category, (x) => x.PriceRanges);
				row.SetProperty(context.Result, category, (x) => x.ShowOnHomePage);
				row.SetProperty(context.Result, category, (x) => x.HasDiscountsApplied);
				row.SetProperty(context.Result, category, (x) => x.Published, true);
				row.SetProperty(context.Result, category, (x) => x.DisplayOrder);
				row.SetProperty(context.Result, category, (x) => x.LimitedToStores);
				row.SetProperty(context.Result, category, (x) => x.Alias);
				row.SetProperty(context.Result, category, (x) => x.DefaultViewMode);

				if (row.DataRow.TryGetValue("CategoryTemplateId", out key))
				{
					int templateId = key.ToString().ToInt();
					if (templateId > 0 && allCategoryTemplateIds.Contains(templateId))
						category.CategoryTemplateId = templateId;
				}

				if (row.DataRow.TryGetValue("PictureId", out key))
				{
					int pictureId = key.ToString().ToInt();
					if (pictureId > 0 && _pictureRepository.TableUntracked.Any(x => x.Id == pictureId))
						category.PictureId = pictureId;
				}

				var storeIds = row.GetDataValue<List<int>>("StoreIds");
				if (storeIds != null && storeIds.Any())
				{
					_storeMappingService.SaveStoreMappings(category, storeIds.ToArray());
				}

				row.SetProperty(context.Result, category, (x) => x.CreatedOnUtc, utcNow);
				category.UpdatedOnUtc = utcNow;

				if (row.IsTransient)
				{
					_categoryRepository.Insert(category);
					lastInserted = category;
				}
				else
				{
					_categoryRepository.Update(category);
					lastUpdated = category;
				}
			}

			// commit whole batch at once
			var num = _categoryRepository.Context.SaveChanges();

			// Perf: notify only about LAST insertion and update
			if (lastInserted != null)
				_services.EventPublisher.EntityInserted(lastInserted);
			if (lastUpdated != null)
				_services.EventPublisher.EntityUpdated(lastUpdated);

			return num;
		}

		public void Execute(ImportExecuteContext context)
		{
			var allCategoryTemplateIds = _categoryTemplateService.GetAllCategoryTemplates()
				.Select(x => x.Id)
				.ToList();

			using (var scope = new DbContextScope(ctx: _categoryRepository.Context, autoDetectChanges: false, proxyCreation: false, validateOnSave: false))
			{
				var segmenter = new ImportDataSegmenter<Category>(context.DataTable);
				var utcNow = DateTime.UtcNow;

				context.Result.TotalRecords = segmenter.TotalRows;

				while (context.Abort == DataExchangeAbortion.None && segmenter.ReadNextBatch())
				{
					var batch = segmenter.CurrentBatch;

					// Perf: detach all entities
					_categoryRepository.Context.DetachAll(false);

					context.SetProgress(segmenter.CurrentSegmentFirstRowIndex - 1, segmenter.TotalRows);

					try
					{
						ProcessCategories(context, batch, utcNow, allCategoryTemplateIds);
					}
					catch (Exception exception)
					{
						context.Result.AddError(exception, segmenter.CurrentSegment, "ProcessCategories");
					}

					// reduce batch to saved (valid) products.
					// No need to perform import operations on errored products.
					batch = batch.Where(x => x.Entity != null && !x.IsTransient).ToArray();

					// update result object
					context.Result.NewRecords += batch.Count(x => x.IsNew && !x.IsTransient);
					context.Result.ModifiedRecords += batch.Count(x => !x.IsNew && !x.IsTransient);

					if (context.DataTable.HasColumn("SeName") || batch.Any(x => x.IsNew || x.NameChanged))
					{
						try
						{
							_categoryRepository.Context.AutoDetectChangesEnabled = true;
							ProcessSlugs(context, batch);
						}
						catch (Exception exception)
						{
							context.Result.AddError(exception, segmenter.CurrentSegment, "ProcessSlugs");
						}
						finally
						{
							_categoryRepository.Context.AutoDetectChangesEnabled = false;
						}
					}
				}
			}
		}
	}
}
