using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Events;
using SmartStore.Services.DataExchange.Import;
using SmartStore.Services.Media;
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
		private readonly IUrlRecordService _urlRecordService;
		private readonly ICategoryTemplateService _categoryTemplateService;
		private readonly IStoreMappingService _storeMappingService;
		private readonly IPictureService _pictureService;
		private readonly SeoSettings _seoSettings;

		public CategoryImporter(
			IRepository<Category> categoryRepository,
			IRepository<UrlRecord> urlRecordRepository,
			IRepository<Picture> pictureRepository,
			ICommonServices services,
			IUrlRecordService urlRecordService,
			ICategoryTemplateService categoryTemplateService,
			IStoreMappingService storeMappingService,
			IPictureService pictureService,
			SeoSettings seoSettings)
		{
			_categoryRepository = categoryRepository;
			_urlRecordRepository = urlRecordRepository;
			_pictureRepository = pictureRepository;
			_services = services;
			_urlRecordService = urlRecordService;
			_categoryTemplateService = categoryTemplateService;
			_storeMappingService = storeMappingService;
			_pictureService = pictureService;
			_seoSettings = seoSettings;
		}

		private int ProcessSlugs(IImportExecuteContext context, ImportRow<Category>[] batch)
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
				if (row.IsNew || row.NameChanged || row.Segmenter.HasColumn("SeName"))
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

		private int ProcessParentMappings(IImportExecuteContext context,
			ImportRow<Category>[] batch,
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
						}
					}
				}
			}

			var num = _categoryRepository.Context.SaveChanges();

			return num;
		}

		private int ProcessPictures(IImportExecuteContext context,
			ImportRow<Category>[] batch,
			Dictionary<int, ImportCategoryMapping> srcToDestId)
		{
			Picture picture = null;
			var equalPictureId = 0;

			foreach (var row in batch)
			{
				var srcId = row.GetDataValue<int>("Id");
				var thumbPath = row.GetDataValue<string>("PictureThumbPath");

				if (srcId != 0 && srcToDestId.ContainsKey(srcId) && thumbPath.HasValue() && File.Exists(thumbPath))
				{
					var category = _categoryRepository.GetById(srcToDestId[srcId].DestinationId);

					if (category != null)
					{
						var pictures = new List<Picture>();
						if (category.PictureId.HasValue && (picture = _pictureRepository.GetById(category.PictureId.Value)) != null)
							pictures.Add(picture);

						var pictureBinary = _pictureService.FindEqualPicture(thumbPath, pictures, out equalPictureId);

						if (pictureBinary != null && pictureBinary.Length > 0 &&
							(picture = _pictureService.InsertPicture(pictureBinary, "image/jpeg", _pictureService.GetPictureSeName(row.EntityDisplayName), true, false, false)) != null)
						{
							category.PictureId = picture.Id;

							_categoryRepository.Update(category);
						}
					}
				}
			}

			var num = _categoryRepository.Context.SaveChanges();

			return num;
		}

		private void ProcessStoreMappings(IImportExecuteContext context, ImportRow<Category>[] batch)
		{
			foreach (var row in batch)
			{
				var limitedToStore = row.GetDataValue<bool>("LimitedToStores");

				if (limitedToStore)
				{
					var storeIds = row.GetDataValue<List<int>>("StoreIds");

					_storeMappingService.SaveStoreMappings(row.Entity, storeIds == null ? new int[0] : storeIds.ToArray());
				}
			}
		}

		private int ProcessCategories(IImportExecuteContext context,
			ImportRow<Category>[] batch,
			DateTime utcNow,
			List<int> allCategoryTemplateIds,
			Dictionary<int, ImportCategoryMapping> srcToDestId)
		{
			_categoryRepository.AutoCommitEnabled = true;

			Category lastInserted = null;
			Category lastUpdated = null;
			var defaultTemplateId = allCategoryTemplateIds.First();

			foreach (var row in batch)
			{
				Category category = null;
				var id = row.GetDataValue<int>("Id");

				if (id != 0)
				{
					category = _categoryRepository.GetById(id);
				}

				if (category == null)
				{
					// a Name is required with new categories
					if (!row.Segmenter.HasColumn("Name"))
					{
						context.Result.AddError("The 'Name' field is required for new categories. Skipping row.", row.GetRowInfo(), "Name");
						continue;
					}

					if (context.UpdateOnly)
					{
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

				var templateId = row.GetDataValue<int>("CategoryTemplateId");
				if (templateId > 0 && allCategoryTemplateIds.Contains(templateId))
				{
					category.CategoryTemplateId = templateId;
				}

				row.SetProperty(context.Result, category, (x) => x.CreatedOnUtc, utcNow);
				category.UpdatedOnUtc = utcNow;

				if (id != 0 && !srcToDestId.ContainsKey(id))
				{
					srcToDestId.Add(id, new ImportCategoryMapping { Inserted = row.IsTransient });
				}

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

			// get new category ids
			foreach (var row in batch)
			{
				var id = row.GetDataValue<int>("Id");

				if (id != 0 && srcToDestId.ContainsKey(id))
					srcToDestId[id].DestinationId = row.Entity.Id;
			}

			// Perf: notify only about LAST insertion and update
			if (lastInserted != null)
				_services.EventPublisher.EntityInserted(lastInserted);
			if (lastUpdated != null)
				_services.EventPublisher.EntityUpdated(lastUpdated);

			return num;
		}

		public void Execute(IImportExecuteContext context)
		{
			var utcNow = DateTime.UtcNow;
			var srcToDestId = new Dictionary<int, ImportCategoryMapping>();

			var allCategoryTemplateIds = _categoryTemplateService.GetAllCategoryTemplates()
				.Select(x => x.Id)
				.ToList();

			using (var scope = new DbContextScope(ctx: _categoryRepository.Context, autoDetectChanges: false, proxyCreation: false, validateOnSave: false))
			{
				var segmenter = context.GetSegmenter<Category>();

				context.Result.TotalRecords = segmenter.TotalRows;

				while (context.Abort == DataExchangeAbortion.None && segmenter.ReadNextBatch())
				{
					var batch = segmenter.CurrentBatch;

					// Perf: detach all entities
					_categoryRepository.Context.DetachAll(false);

					context.SetProgress(segmenter.CurrentSegmentFirstRowIndex - 1, segmenter.TotalRows);

					try
					{
						ProcessCategories(context, batch, utcNow, allCategoryTemplateIds, srcToDestId);
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

					// process slugs
					if (segmenter.HasColumn("SeName") || batch.Any(x => x.IsNew || x.NameChanged))
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

					// process store mappings
					if (segmenter.HasColumn("LimitedToStores") && segmenter.HasColumn("StoreIds"))
					{
						try
						{
							_categoryRepository.Context.AutoDetectChangesEnabled = true;
							ProcessStoreMappings(context, batch);
						}
						catch (Exception exception)
						{
							context.Result.AddError(exception, segmenter.CurrentSegment, "ProcessStoreMappings");
						}
						finally
						{
							_categoryRepository.Context.AutoDetectChangesEnabled = false;
						}
					}

					// process pictures
					if (srcToDestId.Any() && segmenter.HasColumn("PictureThumbPath"))
					{
						try
						{
							_categoryRepository.Context.AutoDetectChangesEnabled = true;
							ProcessPictures(context, batch, srcToDestId);
						}
						catch (Exception exception)
						{
							context.Result.AddError(exception, segmenter.CurrentSegment, "ProcessPictures");
						}
						finally
						{
							_categoryRepository.Context.AutoDetectChangesEnabled = false;
						}
					}
				}

				// map parent id of inserted categories
				if (srcToDestId.Any() && segmenter.HasColumn("Id") && segmenter.HasColumn("ParentCategoryId"))
				{
					segmenter.Reset();

					while (context.Abort == DataExchangeAbortion.None && segmenter.ReadNextBatch())
					{
						var batch = segmenter.CurrentBatch;

						_categoryRepository.Context.DetachAll(false);

						try
						{
							ProcessParentMappings(context, batch, srcToDestId);
						}
						catch (Exception exception)
						{
							context.Result.AddError(exception, segmenter.CurrentSegment, "ProcessParentMappings");
						}
					}
				}
			}
		}
	}


	internal class ImportCategoryMapping
	{
		public int DestinationId { get; set; }
		public bool Inserted { get; set; }
	}
}
