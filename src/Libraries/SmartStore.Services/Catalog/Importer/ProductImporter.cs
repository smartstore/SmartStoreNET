using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Events;
using SmartStore.Services.DataExchange.Import;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Seo;
using SmartStore.Services.Stores;
using SmartStore.Utilities;

namespace SmartStore.Services.Catalog.Importer
{
	public class ProductImporter : IEntityImporter
	{
		private readonly IRepository<ProductPicture> _productPictureRepository;
		private readonly IRepository<ProductManufacturer> _productManufacturerRepository;
		private readonly IRepository<ProductCategory> _productCategoryRepository;
		private readonly IRepository<UrlRecord> _urlRecordRepository;
		private readonly IRepository<Product> _productRepository;
		private readonly ICommonServices _services;
		private readonly ILanguageService _languageService;
		private readonly ILocalizedEntityService _localizedEntityService;
		private readonly IPictureService _pictureService;
		private readonly IManufacturerService _manufacturerService;
		private readonly ICategoryService _categoryService;
		private readonly IProductService _productService;
		private readonly IUrlRecordService _urlRecordService;
		private readonly IStoreMappingService _storeMappingService;
		private readonly SeoSettings _seoSettings;

		public ProductImporter(
			IRepository<ProductPicture> productPictureRepository,
			IRepository<ProductManufacturer> productManufacturerRepository,
			IRepository<ProductCategory> productCategoryRepository,
			IRepository<UrlRecord> urlRecordRepository,
			IRepository<Product> productRepository,
			ICommonServices services,
			ILanguageService languageService,
			ILocalizedEntityService localizedEntityService,
			IPictureService pictureService,
			IManufacturerService manufacturerService,
			ICategoryService categoryService,
			IProductService productService,
			IUrlRecordService urlRecordService,
			IStoreMappingService storeMappingService,
			SeoSettings seoSettings)
		{
			_productPictureRepository = productPictureRepository;
			_productManufacturerRepository = productManufacturerRepository;
			_productCategoryRepository = productCategoryRepository;
			_urlRecordRepository = urlRecordRepository;
			_productRepository = productRepository;
			_services = services;
			_languageService = languageService;
			_localizedEntityService = localizedEntityService;
			_pictureService = pictureService;
			_manufacturerService = manufacturerService;
			_categoryService = categoryService;
			_productService = productService;
			_urlRecordService = urlRecordService;
			_storeMappingService = storeMappingService;
			_seoSettings = seoSettings;
		}

		private int? ZeroToNull(object value, CultureInfo culture)
		{
			int result;
			if (CommonHelper.TryConvert<int>(value, culture, out result) && result > 0)
			{
				return result;
			}

			return (int?)null;
		}

		private void ProcessProductPictures(IImportExecuteContext context, ImportRow<Product>[] batch)
		{
			// true, cause pictures must be saved and assigned an id prior adding a mapping.
			_productPictureRepository.AutoCommitEnabled = true;

			ProductPicture lastInserted = null;
			var equalPictureId = 0;

			foreach (var row in batch)
			{
				var count = -1;
				var thumbPaths = row.GetDataValue<List<string>>("PictureThumbPaths");

				if (thumbPaths == null)
					continue;

				foreach (var path in thumbPaths.Where(x => x.HasValue() && File.Exists(x)))
				{
					try
					{
						var currentProductPictures = _productPictureRepository.TableUntracked.Expand(x => x.Picture)
							.Where(x => x.ProductId == row.Entity.Id)
							.ToList();

						var currentPictures = currentProductPictures
							.Select(x => x.Picture)
							.ToList();

						if (count == -1)
						{
							count = (currentProductPictures.Any() ? currentProductPictures.Select(x => x.DisplayOrder).Max() : 0);
						}

						var pictureBinary = _pictureService.FindEqualPicture(path, currentPictures, out equalPictureId);

						if (pictureBinary != null && pictureBinary.Length > 0)
						{
							// no equal picture found in sequence
							var newPicture = _pictureService.InsertPicture(pictureBinary, "image/jpeg", _pictureService.GetPictureSeName(row.EntityDisplayName), true, false, false);
							if (newPicture != null)
							{
								var mapping = new ProductPicture
								{
									ProductId = row.Entity.Id,
									PictureId = newPicture.Id,
									DisplayOrder = ++count
								};

								_productPictureRepository.Insert(mapping);
								lastInserted = mapping;
							}
						}
						else
						{
							var idx = thumbPaths.IndexOf(path) + 1;
							context.Result.AddInfo("Found equal picture in data store. Skipping field.", row.GetRowInfo(), "PictureThumbPaths" + idx.ToString());
						}
					}
					catch (Exception exception)
					{
						var idx = thumbPaths.IndexOf(path) + 1;
						context.Result.AddWarning(exception.Message, row.GetRowInfo(), "PictureThumbPaths" + idx.ToString());
					}
				}
			}

			// Perf: notify only about LAST insertion and update
			if (lastInserted != null)
				_services.EventPublisher.EntityInserted(lastInserted);
		}

		private int ProcessProductManufacturers(IImportExecuteContext context, ImportRow<Product>[] batch)
		{
			_productManufacturerRepository.AutoCommitEnabled = false;

			ProductManufacturer lastInserted = null;

			foreach (var row in batch)
			{
				var manufacturerIds = row.GetDataValue<List<int>>("ManufacturerIds");
				if (manufacturerIds != null && manufacturerIds.Any())
				{
					try
					{
						foreach (var id in manufacturerIds)
						{
							if (_productManufacturerRepository.TableUntracked.Where(x => x.ProductId == row.Entity.Id && x.ManufacturerId == id).FirstOrDefault() == null)
							{
								// ensure that manufacturer exists
								var manufacturer = _manufacturerService.GetManufacturerById(id);
								if (manufacturer != null)
								{
									var productManufacturer = new ProductManufacturer
									{
										ProductId = row.Entity.Id,
										ManufacturerId = manufacturer.Id,
										IsFeaturedProduct = false,
										DisplayOrder = 1
									};
									_productManufacturerRepository.Insert(productManufacturer);
									lastInserted = productManufacturer;
								}
							}
						}
					}
					catch (Exception exception)
					{
						context.Result.AddWarning(exception.Message, row.GetRowInfo(), "ManufacturerIds");
					}
				}
			}

			// commit whole batch at once
			var num = _productManufacturerRepository.Context.SaveChanges();

			// Perf: notify only about LAST insertion and update
			if (lastInserted != null)
				_services.EventPublisher.EntityInserted(lastInserted);

			return num;
		}

		private int ProcessProductCategories(IImportExecuteContext context, ImportRow<Product>[] batch)
		{
			_productCategoryRepository.AutoCommitEnabled = false;

			ProductCategory lastInserted = null;

			foreach (var row in batch)
			{
				var categoryIds = row.GetDataValue<List<int>>("CategoryIds");
				if (categoryIds != null && categoryIds.Any())
				{
					try
					{
						foreach (var id in categoryIds)
						{
							if (_productCategoryRepository.TableUntracked.Where(x => x.ProductId == row.Entity.Id && x.CategoryId == id).FirstOrDefault() == null)
							{
								// ensure that category exists
								var category = _categoryService.GetCategoryById(id);
								if (category != null)
								{
									var productCategory = new ProductCategory
									{
										ProductId = row.Entity.Id,
										CategoryId = category.Id,
										IsFeaturedProduct = false,
										DisplayOrder = 1
									};
									_productCategoryRepository.Insert(productCategory);
									lastInserted = productCategory;
								}
							}
						}
					}
					catch (Exception exception)
					{
						context.Result.AddWarning(exception.Message, row.GetRowInfo(), "CategoryIds");
					}
				}
			}

			// commit whole batch at once
			var num = _productCategoryRepository.Context.SaveChanges();

			// Perf: notify only about LAST insertion and update
			if (lastInserted != null)
				_services.EventPublisher.EntityInserted(lastInserted);

			return num;
		}

		private int ProcessLocalizations(IImportExecuteContext context, ImportRow<Product>[] batch)
		{
			//_rsProductManufacturer.AutoCommitEnabled = false;

			//string lastInserted = null;

			var languages = _languageService.GetAllLanguages(true);

			foreach (var row in batch)
			{

				Product product = null;

				//get product
				try
				{
					product = _productService.GetProductById(row.Entity.Id);
				}
				catch (Exception exception)
				{
					context.Result.AddWarning(exception.Message, row.GetRowInfo(), "ProcessLocalizations Product");
				}

				foreach (var lang in languages)
				{
					string localizedName = row.GetDataValue<string>("Name", lang.UniqueSeoCode);
					string localizedShortDescription = row.GetDataValue<string>("ShortDescription", lang.UniqueSeoCode);
					string localizedFullDescription = row.GetDataValue<string>("FullDescription", lang.UniqueSeoCode);

					if (localizedName.HasValue())
					{
						_localizedEntityService.SaveLocalizedValue(product, x => x.Name, localizedName, lang.Id);
					}
					if (localizedShortDescription.HasValue())
					{
						_localizedEntityService.SaveLocalizedValue(product, x => x.ShortDescription, localizedShortDescription, lang.Id);
					}
					if (localizedFullDescription.HasValue())
					{
						_localizedEntityService.SaveLocalizedValue(product, x => x.FullDescription, localizedFullDescription, lang.Id);
					}
				}
			}

			// commit whole batch at once
			var num = _productManufacturerRepository.Context.SaveChanges();

			// Perf: notify only about LAST insertion and update
			//if (lastInserted != null)
			//    _eventPublisher.EntityInserted(lastInserted);

			return num;
		}

		private int ProcessSlugs(IImportExecuteContext context, ImportRow<Product>[] batch)
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

			var entityName = typeof(Product).Name;

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

		private int ProcessProducts(IImportExecuteContext context, ImportRow<Product>[] batch, DateTime utcNow)
		{
			_productRepository.AutoCommitEnabled = true;

			Product lastInserted = null;
			Product lastUpdated = null;

			foreach (var row in batch)
			{
				var id = row.GetDataValue<int>("Id");
				var product = _productService.GetProductById(id);

				if (product == null)
				{
					product = _productService.GetProductBySku(row.GetDataValue<string>("Sku"));
				}

				if (product == null)
				{
					product = _productService.GetProductByGtin(row.GetDataValue<string>("Gtin"));
				}

				if (product == null)
				{
					// a Name is required with new products.
					if (!row.Segmenter.HasColumn("Name"))
					{
						context.Result.AddError("The 'Name' field is required for new products. Skipping row.", row.GetRowInfo(), "Name");
						continue;
					}
					product = new Product();
				}

				var name = row.GetDataValue<string>("Name");

				row.Initialize(product, name);

				if (!row.IsNew)
				{
					if (!product.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
					{
						// Perf: use this later for SeName updates.
						row.NameChanged = true;
					}
				}

				row.SetProperty(context.Result, product, (x) => x.Sku);
				row.SetProperty(context.Result, product, (x) => x.Gtin);
				row.SetProperty(context.Result, product, (x) => x.ManufacturerPartNumber);
				row.SetProperty(context.Result, product, (x) => x.ProductTypeId, (int)ProductType.SimpleProduct);
				row.SetProperty(context.Result, product, (x) => x.ParentGroupedProductId);
				row.SetProperty(context.Result, product, (x) => x.VisibleIndividually, true);
				row.SetProperty(context.Result, product, (x) => x.Name);
				row.SetProperty(context.Result, product, (x) => x.ShortDescription);
				row.SetProperty(context.Result, product, (x) => x.FullDescription);
				row.SetProperty(context.Result, product, (x) => x.ProductTemplateId);
				row.SetProperty(context.Result, product, (x) => x.ShowOnHomePage);
				row.SetProperty(context.Result, product, (x) => x.HomePageDisplayOrder);
				row.SetProperty(context.Result, product, (x) => x.MetaKeywords);
				row.SetProperty(context.Result, product, (x) => x.MetaDescription);
				row.SetProperty(context.Result, product, (x) => x.MetaTitle);
				row.SetProperty(context.Result, product, (x) => x.AllowCustomerReviews, true);
				row.SetProperty(context.Result, product, (x) => x.Published, true);
				row.SetProperty(context.Result, product, (x) => x.IsGiftCard);
				row.SetProperty(context.Result, product, (x) => x.GiftCardTypeId);
				row.SetProperty(context.Result, product, (x) => x.RequireOtherProducts);
				row.SetProperty(context.Result, product, (x) => x.RequiredProductIds);
				row.SetProperty(context.Result, product, (x) => x.AutomaticallyAddRequiredProducts);
				row.SetProperty(context.Result, product, (x) => x.IsDownload);
				row.SetProperty(context.Result, product, (x) => x.DownloadId);
				row.SetProperty(context.Result, product, (x) => x.UnlimitedDownloads, true);
				row.SetProperty(context.Result, product, (x) => x.MaxNumberOfDownloads, 10);
				row.SetProperty(context.Result, product, (x) => x.DownloadActivationTypeId, 1);
				row.SetProperty(context.Result, product, (x) => x.HasSampleDownload);
				row.SetProperty(context.Result, product, (x) => x.SampleDownloadId, (int?)null, ZeroToNull);
				row.SetProperty(context.Result, product, (x) => x.HasUserAgreement);
				row.SetProperty(context.Result, product, (x) => x.UserAgreementText);
				row.SetProperty(context.Result, product, (x) => x.IsRecurring);
				row.SetProperty(context.Result, product, (x) => x.RecurringCycleLength, 100);
				row.SetProperty(context.Result, product, (x) => x.RecurringCyclePeriodId);
				row.SetProperty(context.Result, product, (x) => x.RecurringTotalCycles, 10);
				row.SetProperty(context.Result, product, (x) => x.IsShipEnabled, true);
				row.SetProperty(context.Result, product, (x) => x.IsFreeShipping);
				row.SetProperty(context.Result, product, (x) => x.AdditionalShippingCharge);
				row.SetProperty(context.Result, product, (x) => x.IsEsd);
				row.SetProperty(context.Result, product, (x) => x.IsTaxExempt);
				row.SetProperty(context.Result, product, (x) => x.TaxCategoryId, 1);
				row.SetProperty(context.Result, product, (x) => x.ManageInventoryMethodId);
				row.SetProperty(context.Result, product, (x) => x.StockQuantity, 10000);
				row.SetProperty(context.Result, product, (x) => x.DisplayStockAvailability);
				row.SetProperty(context.Result, product, (x) => x.DisplayStockQuantity);
				row.SetProperty(context.Result, product, (x) => x.MinStockQuantity);
				row.SetProperty(context.Result, product, (x) => x.LowStockActivityId);
				row.SetProperty(context.Result, product, (x) => x.NotifyAdminForQuantityBelow, 1);
				row.SetProperty(context.Result, product, (x) => x.BackorderModeId);
				row.SetProperty(context.Result, product, (x) => x.AllowBackInStockSubscriptions);
				row.SetProperty(context.Result, product, (x) => x.OrderMinimumQuantity, 1);
				row.SetProperty(context.Result, product, (x) => x.OrderMaximumQuantity, 10000);
				row.SetProperty(context.Result, product, (x) => x.AllowedQuantities);
				row.SetProperty(context.Result, product, (x) => x.DisableBuyButton);
				row.SetProperty(context.Result, product, (x) => x.DisableWishlistButton);
				row.SetProperty(context.Result, product, (x) => x.AvailableForPreOrder);
				row.SetProperty(context.Result, product, (x) => x.CallForPrice);
				row.SetProperty(context.Result, product, (x) => x.Price);
				row.SetProperty(context.Result, product, (x) => x.OldPrice);
				row.SetProperty(context.Result, product, (x) => x.ProductCost);
				row.SetProperty(context.Result, product, (x) => x.SpecialPrice);
				row.SetProperty(context.Result, product, (x) => x.SpecialPriceStartDateTimeUtc);
				row.SetProperty(context.Result, product, (x) => x.SpecialPriceEndDateTimeUtc);
				row.SetProperty(context.Result, product, (x) => x.CustomerEntersPrice);
				row.SetProperty(context.Result, product, (x) => x.MinimumCustomerEnteredPrice);
				row.SetProperty(context.Result, product, (x) => x.MaximumCustomerEnteredPrice, 1000);
				row.SetProperty(context.Result, product, (x) => x.Weight);
				row.SetProperty(context.Result, product, (x) => x.Length);
				row.SetProperty(context.Result, product, (x) => x.Width);
				row.SetProperty(context.Result, product, (x) => x.Height);
				row.SetProperty(context.Result, product, (x) => x.DeliveryTimeId);
				row.SetProperty(context.Result, product, (x) => x.QuantityUnitId);
				row.SetProperty(context.Result, product, (x) => x.BasePriceEnabled);
				row.SetProperty(context.Result, product, (x) => x.BasePriceMeasureUnit);
				row.SetProperty(context.Result, product, (x) => x.BasePriceAmount);
				row.SetProperty(context.Result, product, (x) => x.BasePriceBaseAmount);
				row.SetProperty(context.Result, product, (x) => x.BundlePerItemPricing);
				row.SetProperty(context.Result, product, (x) => x.BundlePerItemShipping);
				row.SetProperty(context.Result, product, (x) => x.BundlePerItemShoppingCart);
				row.SetProperty(context.Result, product, (x) => x.BundleTitleText);
				row.SetProperty(context.Result, product, (x) => x.AvailableStartDateTimeUtc);
				row.SetProperty(context.Result, product, (x) => x.AvailableEndDateTimeUtc);
				row.SetProperty(context.Result, product, (x) => x.LimitedToStores);

				var storeIds = row.GetDataValue<List<int>>("StoreIds");
				if (storeIds != null && storeIds.Any())
				{
					_storeMappingService.SaveStoreMappings(product, storeIds.ToArray());
				}

				row.SetProperty(context.Result, product, (x) => x.CreatedOnUtc, utcNow);
				product.UpdatedOnUtc = utcNow;

				if (row.IsTransient)
				{
					_productRepository.Insert(product);
					lastInserted = product;
				}
				else
				{
					_productRepository.Update(product);
					lastUpdated = product;
				}
			}

			// commit whole batch at once
			var num = _productRepository.Context.SaveChanges();

			// Perf: notify only about LAST insertion and update
			if (lastInserted != null)
				_services.EventPublisher.EntityInserted(lastInserted);
			if (lastUpdated != null)
				_services.EventPublisher.EntityUpdated(lastUpdated);

			return num;
		}

		public void Execute(IImportExecuteContext context)
		{
			using (var scope = new DbContextScope(ctx: _productRepository.Context, autoDetectChanges: false, proxyCreation: false, validateOnSave: false))
			{
				var segmenter = context.GetSegmenter<Product>();
				var utcNow = DateTime.UtcNow;

				context.Result.TotalRecords = segmenter.TotalRows;

				while (context.Abort == DataExchangeAbortion.None && segmenter.ReadNextBatch())
				{
					var batch = segmenter.CurrentBatch;

					// Perf: detach all entities
					_productRepository.Context.DetachAll(false);

					context.SetProgress(segmenter.CurrentSegmentFirstRowIndex - 1, segmenter.TotalRows);

					// ===========================================================================
					// 1.) Import products
					// ===========================================================================
					try
					{
						ProcessProducts(context, batch, utcNow);
					}
					catch (Exception exception)
					{
						context.Result.AddError(exception, segmenter.CurrentSegment, "ProcessProducts");
					}

					// reduce batch to saved (valid) products.
					// No need to perform import operations on errored products.
					batch = batch.Where(x => x.Entity != null && !x.IsTransient).ToArray();

					// update result object
					context.Result.NewRecords += batch.Count(x => x.IsNew && !x.IsTransient);
					context.Result.ModifiedRecords += batch.Count(x => !x.IsNew && !x.IsTransient);

					// ===========================================================================
					// 2.) Import SEO Slugs
					// IMPORTANT: Unlike with Products AutoCommitEnabled must be TRUE,
					//            as Slugs are going to be validated against existing ones in DB.
					// ===========================================================================
					if (segmenter.HasColumn("SeName") || batch.Any(x => x.IsNew || x.NameChanged))
					{
						try
						{
							_productRepository.Context.AutoDetectChangesEnabled = true;
							ProcessSlugs(context, batch);
						}
						catch (Exception exception)
						{
							context.Result.AddError(exception, segmenter.CurrentSegment, "ProcessSlugs");
						}
						finally
						{
							_productRepository.Context.AutoDetectChangesEnabled = false;
						}
					}

					// ===========================================================================
					// 3.) Import Localizations
					// ===========================================================================
					try
					{
						ProcessLocalizations(context, batch);
					}
					catch (Exception exception)
					{
						context.Result.AddError(exception, segmenter.CurrentSegment, "ProcessLocalizations");
					}

					// ===========================================================================
					// 4.) Import product category mappings
					// ===========================================================================
					if (segmenter.HasColumn("CategoryIds"))
					{
						try
						{
							ProcessProductCategories(context, batch);
						}
						catch (Exception exception)
						{
							context.Result.AddError(exception, segmenter.CurrentSegment, "ProcessProductCategories");
						}
					}

					// ===========================================================================
					// 5.) Import product manufacturer mappings
					// ===========================================================================
					if (segmenter.HasColumn("ManufacturerIds"))
					{
						try
						{
							ProcessProductManufacturers(context, batch);
						}
						catch (Exception exception)
						{
							context.Result.AddError(exception, segmenter.CurrentSegment, "ProcessProductManufacturers");
						}
					}

					// ===========================================================================
					// 6.) Import product picture mappings
					// ===========================================================================
					if (segmenter.HasColumn("PictureThumbPaths"))
					{
						try
						{
							ProcessProductPictures(context, batch);
						}
						catch (Exception exception)
						{
							context.Result.AddError(exception, segmenter.CurrentSegment, "ProcessProductPictures");
						}
					}
				}
			}
		}
	}
}
