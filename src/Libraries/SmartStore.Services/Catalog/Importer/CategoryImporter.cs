using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using SmartStore.Core.Async;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Events;
using SmartStore.Services.DataExchange.Import;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Seo;
using SmartStore.Services.Stores;
using SmartStore.Utilities;

namespace SmartStore.Services.Catalog.Importer
{
	public class CategoryImporter : EntityImporterBase
	{
		private readonly IRepository<Category> _categoryRepository;
		private readonly IRepository<Picture> _pictureRepository;
		private readonly ICommonServices _services;
		private readonly ICategoryTemplateService _categoryTemplateService;
		private readonly IPictureService _pictureService;
		private readonly ILocalizedEntityService _localizedEntityService;
		private readonly FileDownloadManager _fileDownloadManager;

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
			IRepository<Picture> pictureRepository,
			ICommonServices services,
			ICategoryTemplateService categoryTemplateService,
			IPictureService pictureService,
			ILocalizedEntityService localizedEntityService,
			FileDownloadManager fileDownloadManager)
		{
			_categoryRepository = categoryRepository;
			_pictureRepository = pictureRepository;
			_services = services;
			_categoryTemplateService = categoryTemplateService;
			_pictureService = pictureService;
			_localizedEntityService = localizedEntityService;
			_fileDownloadManager = fileDownloadManager;
		}

		protected override void Import(ImportExecuteContext context)
		{
			var srcToDestId = new Dictionary<int, ImportCategoryMapping>();

			var templateViewPaths = _categoryTemplateService.GetAllCategoryTemplates().ToDictionarySafe(x => x.ViewPath, x => x.Id);

			using (var scope = new DbContextScope(ctx: context.Services.DbContext, autoDetectChanges: false, proxyCreation: false, validateOnSave: false))
			{
				var segmenter = context.DataSegmenter;

				Initialize(context);

				while (context.Abort == DataExchangeAbortion.None && segmenter.ReadNextBatch())
				{
					var batch = segmenter.GetCurrentBatch<Category>();

					// Perf: detach all entities
					_categoryRepository.Context.DetachAll(false);

					context.SetProgress(segmenter.CurrentSegmentFirstRowIndex - 1, segmenter.TotalRows);

					try
					{
						ProcessCategories(context, batch, templateViewPaths, srcToDestId);
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
					if (segmenter.HasColumn("SeName", true) || batch.Any(x => x.IsNew || x.NameChanged))
					{
						try
						{
							_categoryRepository.Context.AutoDetectChangesEnabled = true;
							ProcessSlugs(context, batch, typeof(Category).Name);
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
					if (segmenter.HasColumn("StoreIds"))
					{
						try
						{
							ProcessStoreMappings(context, batch);
						}
						catch (Exception exception)
						{
							context.Result.AddError(exception, segmenter.CurrentSegment, "ProcessStoreMappings");
						}
					}

					// localizations
					try
					{
						ProcessLocalizations(context, batch, _localizableProperties);
					}
					catch (Exception exception)
					{
						context.Result.AddError(exception, segmenter.CurrentSegment, "ProcessLocalizedProperties");
					}

					// process pictures
					if (srcToDestId.Any() && segmenter.HasColumn("ImageUrl") && !segmenter.IsIgnored("PictureId"))
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
				if (srcToDestId.Any() && segmenter.HasColumn("Id") && segmenter.HasColumn("ParentCategoryId") && !segmenter.IsIgnored("ParentCategoryId"))
				{
					segmenter.Reset();

					while (context.Abort == DataExchangeAbortion.None && segmenter.ReadNextBatch())
					{
						var batch = segmenter.GetCurrentBatch<Category>();
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

		protected virtual int ProcessPictures(
			ImportExecuteContext context,
			IEnumerable<ImportRow<Category>> batch,
			Dictionary<int, ImportCategoryMapping> srcToDestId)
		{
			Picture picture = null;
			var equalPictureId = 0;

			foreach (var row in batch)
			{
				try
				{
					var srcId = row.GetDataValue<int>("Id");
					var urlOrPath = row.GetDataValue<string>("ImageUrl");

					if (srcId != 0 && srcToDestId.ContainsKey(srcId) && urlOrPath.HasValue())
					{
						var currentPictures = new List<Picture>();
						var category = _categoryRepository.GetById(srcToDestId[srcId].DestinationId);
						var seoName = _pictureService.GetPictureSeName(row.EntityDisplayName);
						var image = CreateDownloadImage(urlOrPath, seoName, 1);

						if (category != null && image != null)
						{
							if (image.Url.HasValue() && !image.Success.HasValue)
							{
								AsyncRunner.RunSync(() => _fileDownloadManager.DownloadAsync(DownloaderContext, new FileDownloadManagerItem[] { image }));
							}

							if ((image.Success ?? false) && File.Exists(image.Path))
							{
								Succeeded(image);
								var pictureBinary = File.ReadAllBytes(image.Path);

								if (pictureBinary != null && pictureBinary.Length > 0)
								{
									if (category.PictureId.HasValue && (picture = _pictureRepository.GetById(category.PictureId.Value)) != null)
										currentPictures.Add(picture);

									var size = Size.Empty;
									pictureBinary = _pictureService.ValidatePicture(pictureBinary, out size);
									pictureBinary = _pictureService.FindEqualPicture(pictureBinary, currentPictures, out equalPictureId);

									if (pictureBinary != null && pictureBinary.Length > 0)
									{
										if ((picture = _pictureService.InsertPicture(pictureBinary, image.MimeType, seoName, true, size.Width, size.Height, false)) != null)
										{
											category.PictureId = picture.Id;
											_categoryRepository.Update(category);
										}
									}
									else
									{
										context.Result.AddInfo("Found equal picture in data store. Skipping field.", row.GetRowInfo(), "ImageUrls");
									}
								}
							}
							else if (image.Url.HasValue())
							{
								context.Result.AddInfo("Download of an image failed.", row.GetRowInfo(), "ImageUrls");
							}
						}
					}
				}
				catch (Exception exception)
				{
					context.Result.AddWarning(exception.ToAllMessages(), row.GetRowInfo(), "ImageUrls");
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

			Category lastInserted = null;
			Category lastUpdated = null;
			var defaultTemplateId = templateViewPaths["CategoryTemplate.ProductsInGridOrLines"];

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

					// a Name is required with new categories
					if (!row.Segmenter.HasColumn("Name"))
					{
						++context.Result.SkippedRecords;
						context.Result.AddError("The 'Name' field is required for new categories. Skipping row.", row.GetRowInfo(), "Name");
						continue;
					}

					category = new Category();
				}

				row.Initialize(category, name ?? category.Name);

				if (!row.IsNew && !category.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
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
			{
				_services.EventPublisher.EntityInserted(lastInserted);
			}

			if (lastUpdated != null)
			{
				_services.EventPublisher.EntityUpdated(lastUpdated);
			}

			return num;
		}

		public static string[] SupportedKeyFields
		{
			get
			{
				return new string[] { "Id", "Name" };
			}
		}

		public static string[] DefaultKeyFields
		{
			get
			{
				return new string[] { "Name", "Id" };
			}
		}

		public class ImportCategoryMapping
		{
			public int DestinationId { get; set; }
			public bool Inserted { get; set; }
		}
	}

}
