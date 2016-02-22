using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using SmartStore.Core.Async;
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
	public class ProductImporter : EntityImporterBase, IEntityImporter
	{
		private readonly IRepository<ProductPicture> _productPictureRepository;
		private readonly IRepository<ProductManufacturer> _productManufacturerRepository;
		private readonly IRepository<ProductCategory> _productCategoryRepository;
		private readonly IRepository<UrlRecord> _urlRecordRepository;
		private readonly IRepository<Product> _productRepository;
		private readonly ICommonServices _services;
		private readonly ILocalizedEntityService _localizedEntityService;
		private readonly IPictureService _pictureService;
		private readonly IManufacturerService _manufacturerService;
		private readonly ICategoryService _categoryService;
		private readonly IProductService _productService;
		private readonly IUrlRecordService _urlRecordService;
		private readonly IProductTemplateService _productTemplateService;
		private readonly IStoreMappingService _storeMappingService;
		private readonly FileDownloadManager _fileDownloadManager;
		private readonly SeoSettings _seoSettings;
		private readonly DataExchangeSettings _dataExchangeSettings;

		public ProductImporter(
			IRepository<ProductPicture> productPictureRepository,
			IRepository<ProductManufacturer> productManufacturerRepository,
			IRepository<ProductCategory> productCategoryRepository,
			IRepository<UrlRecord> urlRecordRepository,
			IRepository<Product> productRepository,
			ICommonServices services,
			ILocalizedEntityService localizedEntityService,
			IPictureService pictureService,
			IManufacturerService manufacturerService,
			ICategoryService categoryService,
			IProductService productService,
			IUrlRecordService urlRecordService,
			IProductTemplateService productTemplateService,
			IStoreMappingService storeMappingService,
			FileDownloadManager fileDownloadManager,
			SeoSettings seoSettings,
			DataExchangeSettings dataExchangeSettings)
		{
			_productPictureRepository = productPictureRepository;
			_productManufacturerRepository = productManufacturerRepository;
			_productCategoryRepository = productCategoryRepository;
			_urlRecordRepository = urlRecordRepository;
			_productRepository = productRepository;
			_services = services;
			_localizedEntityService = localizedEntityService;
			_pictureService = pictureService;
			_manufacturerService = manufacturerService;
			_categoryService = categoryService;
			_productService = productService;
			_urlRecordService = urlRecordService;
			_productTemplateService = productTemplateService;
			_storeMappingService = storeMappingService;
			_fileDownloadManager = fileDownloadManager;
			
			_seoSettings = seoSettings;
			_dataExchangeSettings = dataExchangeSettings;
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

		private int ProcessProductMappings(IImportExecuteContext context,
			ImportRow<Product>[] batch,
			Dictionary<int, ImportProductMapping> srcToDestId)
		{
			foreach (var row in batch)
			{
				var id = row.GetDataValue<int>("Id");
				var parentGroupedProductId = row.GetDataValue<int>("ParentGroupedProductId");

				if (id != 0 && parentGroupedProductId != 0 && srcToDestId.ContainsKey(id) && srcToDestId.ContainsKey(parentGroupedProductId))
				{
					// only touch relationship if child and parent were inserted
					if (srcToDestId[id].Inserted && srcToDestId[parentGroupedProductId].Inserted && srcToDestId[id].DestinationId != 0)
					{
						var product = _productRepository.GetById(srcToDestId[id].DestinationId);
						if (product != null)
						{
							product.ParentGroupedProductId = srcToDestId[parentGroupedProductId].DestinationId;

							_productRepository.Update(product);
						}
					}
				}
			}

			var num = _productRepository.Context.SaveChanges();

			return num;
		}

		private void ProcessProductPictures(IImportExecuteContext context, ImportRow<Product>[] batch)
		{
			// true, cause pictures must be saved and assigned an id prior adding a mapping.
			_productPictureRepository.AutoCommitEnabled = true;

			ProductPicture lastInserted = null;
			var equalPictureId = 0;

			foreach (var row in batch)
			{
				var imageUrls = row.GetDataValue<List<string>>("ImageUrls");
				if (imageUrls == null)
					continue;

				var imageNumber = 0;
				var displayOrder = -1;
				var seoName = _pictureService.GetPictureSeName(row.EntityDisplayName);
				var imageFiles = new List<FileDownloadManagerItem>();

				// collect required image file infos
				foreach (var urlOrPath in imageUrls)
				{
					var image = CreateDownloadImage(urlOrPath, seoName, ++imageNumber);

					if (image != null)
						imageFiles.Add(image);
				}

				// download images
				if (imageFiles.Any(x => x.Url.HasValue()))
				{
					// async downloading in batch processing is inefficient cause only the image processing benefits from async,
					// not the record processing itself. a per record processing may speed up the import.

					AsyncRunner.RunSync(() => _fileDownloadManager.DownloadAsync(DownloaderContext, imageFiles.Where(x => x.Url.HasValue() && !x.Success.HasValue)));
				}

				// import images
				foreach (var image in imageFiles.OrderBy(x => x.DisplayOrder))
				{
					try
					{
						if ((image.Success ?? false) && File.Exists(image.Path))
						{
							Succeeded(image);
							var pictureBinary = File.ReadAllBytes(image.Path);

							if (pictureBinary != null && pictureBinary.Length > 0)
							{
								var currentProductPictures = _productPictureRepository.TableUntracked.Expand(x => x.Picture)
									.Where(x => x.ProductId == row.Entity.Id)
									.ToList();

								var currentPictures = currentProductPictures
									.Select(x => x.Picture)
									.ToList();

								if (displayOrder == -1)
								{
									displayOrder = (currentProductPictures.Any() ? currentProductPictures.Select(x => x.DisplayOrder).Max() : 0);
								}

								pictureBinary = _pictureService.ValidatePicture(pictureBinary);
								pictureBinary = _pictureService.FindEqualPicture(pictureBinary, currentPictures, out equalPictureId);

								if (pictureBinary != null && pictureBinary.Length > 0)
								{
									// no equal picture found in sequence
									var newPicture = _pictureService.InsertPicture(pictureBinary, image.MimeType, seoName, true, false, false);
									if (newPicture != null)
									{
										var mapping = new ProductPicture
										{
											ProductId = row.Entity.Id,
											PictureId = newPicture.Id,
											DisplayOrder = ++displayOrder
										};

										_productPictureRepository.Insert(mapping);
										lastInserted = mapping;
									}
								}
								else
								{
									context.Result.AddInfo("Found equal picture in data store. Skipping field.", row.GetRowInfo(), "ImageUrls" + image.DisplayOrder.ToString());
								}
							}
						}
						else if (image.Url.HasValue())
						{
							context.Result.AddInfo("Download of an image failed.", row.GetRowInfo(), "ImageUrls" + image.DisplayOrder.ToString());
						}
					}
					catch (Exception exception)
					{
						context.Result.AddWarning(exception.ToAllMessages(), row.GetRowInfo(), "ImageUrls" + image.DisplayOrder.ToString());
					}
				}
			}

			// Perf: notify only about LAST insertion and update
			if (lastInserted != null)
			{
				_services.EventPublisher.EntityInserted(lastInserted);
			}
		}

		private void ProcessProductPicturesOld(IImportExecuteContext context, ImportRow<Product>[] batch)
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
			foreach (var row in batch)
			{
				foreach (var lang in context.Languages)
				{
					var name = row.GetDataValue<string>("Name", lang.UniqueSeoCode);
					var shortDescription = row.GetDataValue<string>("ShortDescription", lang.UniqueSeoCode);
					var fullDescription = row.GetDataValue<string>("FullDescription", lang.UniqueSeoCode);
					var metaKeywords = row.GetDataValue<string>("MetaKeywords", lang.UniqueSeoCode);
					var metaDescription  = row.GetDataValue<string>("MetaDescription", lang.UniqueSeoCode);
					var metaTitle = row.GetDataValue<string>("MetaTitle", lang.UniqueSeoCode);
					var bundleTitleText = row.GetDataValue<string>("BundleTitleText", lang.UniqueSeoCode);

					_localizedEntityService.SaveLocalizedValue(row.Entity, x => x.Name, name, lang.Id);
					_localizedEntityService.SaveLocalizedValue(row.Entity, x => x.ShortDescription, shortDescription, lang.Id);
					_localizedEntityService.SaveLocalizedValue(row.Entity, x => x.FullDescription, fullDescription, lang.Id);
					_localizedEntityService.SaveLocalizedValue(row.Entity, x => x.MetaKeywords, metaKeywords, lang.Id);
					_localizedEntityService.SaveLocalizedValue(row.Entity, x => x.MetaDescription, metaDescription, lang.Id);
					_localizedEntityService.SaveLocalizedValue(row.Entity, x => x.MetaTitle, metaTitle, lang.Id);
					_localizedEntityService.SaveLocalizedValue(row.Entity, x => x.BundleTitleText, bundleTitleText, lang.Id);
				}
			}

			// commit whole batch at once
			var num = _productManufacturerRepository.Context.SaveChanges();

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

		private void ProcessStoreMappings(IImportExecuteContext context, ImportRow<Product>[] batch)
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

		private int ProcessProducts(
			IImportExecuteContext context,
			ImportRow<Product>[] batch,
			Dictionary<string, int> templateViewPaths,
			Dictionary<int, ImportProductMapping> srcToDestId)
		{
			_productRepository.AutoCommitEnabled = true;

			Product lastInserted = null;
			Product lastUpdated = null;
			var defaultTemplateId = templateViewPaths["ProductTemplate.Simple"];

			foreach (var row in batch)
			{
				Product product = null;
				var id = row.GetDataValue<int>("Id");
				
				foreach (var keyName in context.KeyFieldNames)
				{
					switch (keyName)
					{
						case "Id":
							product = _productService.GetProductById(id);
							break;
						case "Sku":
							product = _productService.GetProductBySku(row.GetDataValue<string>("Sku"));
							break;
						case "Gtin":
							product = _productService.GetProductByGtin(row.GetDataValue<string>("Gtin"));
							break;
						case "ManufacturerPartNumber":
							product = _productService.GetProductByManufacturerPartNumber(row.GetDataValue<string>("ManufacturerPartNumber"));
							break;
						case "Name":
							product = _productService.GetProductByName(row.GetDataValue<string>("Name"));
							break;
					}

					if (product != null)
						break;
				}

				if (product == null)
				{
					if (context.UpdateOnly)
					{
						++context.Result.SkippedRecords;
						continue;
					}

					// a Name is required with new products.
					if (!row.Segmenter.HasColumn("Name"))
					{
						++context.Result.SkippedRecords;
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

				row.SetProperty(context.Result, product, (x) => x.ProductTypeId, (int)ProductType.SimpleProduct);
				row.SetProperty(context.Result, product, (x) => x.VisibleIndividually, true);
				row.SetProperty(context.Result, product, (x) => x.Name);
				row.SetProperty(context.Result, product, (x) => x.ShortDescription);
				row.SetProperty(context.Result, product, (x) => x.FullDescription);
				row.SetProperty(context.Result, product, (x) => x.AdminComment);
				row.SetProperty(context.Result, product, (x) => x.ShowOnHomePage);
				row.SetProperty(context.Result, product, (x) => x.HomePageDisplayOrder);
				row.SetProperty(context.Result, product, (x) => x.MetaKeywords);
				row.SetProperty(context.Result, product, (x) => x.MetaDescription);
				row.SetProperty(context.Result, product, (x) => x.MetaTitle);
				row.SetProperty(context.Result, product, (x) => x.AllowCustomerReviews, true);
				row.SetProperty(context.Result, product, (x) => x.ApprovedRatingSum);
				row.SetProperty(context.Result, product, (x) => x.NotApprovedRatingSum);
				row.SetProperty(context.Result, product, (x) => x.ApprovedTotalReviews);
				row.SetProperty(context.Result, product, (x) => x.NotApprovedTotalReviews);
				row.SetProperty(context.Result, product, (x) => x.Published, true);
				row.SetProperty(context.Result, product, (x) => x.Sku);
				row.SetProperty(context.Result, product, (x) => x.ManufacturerPartNumber);
				row.SetProperty(context.Result, product, (x) => x.Gtin);
				row.SetProperty(context.Result, product, (x) => x.IsGiftCard);
				row.SetProperty(context.Result, product, (x) => x.GiftCardTypeId);
				row.SetProperty(context.Result, product, (x) => x.RequireOtherProducts);
				row.SetProperty(context.Result, product, (x) => x.RequiredProductIds);	// TODO: global scope
				row.SetProperty(context.Result, product, (x) => x.AutomaticallyAddRequiredProducts);
				row.SetProperty(context.Result, product, (x) => x.IsDownload);
				row.SetProperty(context.Result, product, (x) => x.DownloadId);
				row.SetProperty(context.Result, product, (x) => x.UnlimitedDownloads, true);
				row.SetProperty(context.Result, product, (x) => x.MaxNumberOfDownloads, 10);
				row.SetProperty(context.Result, product, (x) => x.DownloadExpirationDays);
				row.SetProperty(context.Result, product, (x) => x.DownloadActivationTypeId, 1);
				row.SetProperty(context.Result, product, (x) => x.HasSampleDownload);
				row.SetProperty(context.Result, product, (x) => x.SampleDownloadId, (int?)null, ZeroToNull);    // TODO: global scope
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
				row.SetProperty(context.Result, product, (x) => x.TaxCategoryId, 1);    // TODO: global scope
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
				// HasTierPrices... ignore as long as no tier prices are imported
				// LowestAttributeCombinationPrice... ignore as long as no combinations are imported
				row.SetProperty(context.Result, product, (x) => x.Weight);
				row.SetProperty(context.Result, product, (x) => x.Length);
				row.SetProperty(context.Result, product, (x) => x.Width);
				row.SetProperty(context.Result, product, (x) => x.Height);
				row.SetProperty(context.Result, product, (x) => x.DisplayOrder);
				row.SetProperty(context.Result, product, (x) => x.DeliveryTimeId);      // TODO: global scope
				row.SetProperty(context.Result, product, (x) => x.QuantityUnitId);      // TODO: global scope
				row.SetProperty(context.Result, product, (x) => x.BasePriceEnabled);
				row.SetProperty(context.Result, product, (x) => x.BasePriceMeasureUnit);
				row.SetProperty(context.Result, product, (x) => x.BasePriceAmount);
				row.SetProperty(context.Result, product, (x) => x.BasePriceBaseAmount);
				row.SetProperty(context.Result, product, (x) => x.BundleTitleText);
				row.SetProperty(context.Result, product, (x) => x.BundlePerItemShipping);
				row.SetProperty(context.Result, product, (x) => x.BundlePerItemPricing);
				row.SetProperty(context.Result, product, (x) => x.BundlePerItemShoppingCart);
				row.SetProperty(context.Result, product, (x) => x.AvailableStartDateTimeUtc);
				row.SetProperty(context.Result, product, (x) => x.AvailableEndDateTimeUtc);
				row.SetProperty(context.Result, product, (x) => x.LimitedToStores);

				var tvp = row.GetDataValue<string>("ProductTemplateViewPath");
				product.ProductTemplateId = (tvp.HasValue() && templateViewPaths.ContainsKey(tvp) ? templateViewPaths[tvp] : defaultTemplateId);

				row.SetProperty(context.Result, product, (x) => x.CreatedOnUtc, UtcNow);
				product.UpdatedOnUtc = UtcNow;

				if (id != 0 && !srcToDestId.ContainsKey(id))
				{
					srcToDestId.Add(id, new ImportProductMapping { Inserted = row.IsTransient });
				}

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

			// get new product ids
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
				return new string[] { "Id", "Sku", "Gtin", "ManufacturerPartNumber", "Name" };
			}
		}

		public static string[] DefaultKeyFields
		{
			get
			{
				return new string[] { "Id", "Sku", "Gtin", "ManufacturerPartNumber" };
			}
		}

		public void Execute(IImportExecuteContext context)
		{
			var srcToDestId = new Dictionary<int, ImportProductMapping>();

			var templateViewPaths = _productTemplateService.GetAllProductTemplates()
				.GroupBy(x => x.ViewPath, StringComparer.OrdinalIgnoreCase)
				.ToDictionary(x => x.Key, x => x.First().Id);

			using (var scope = new DbContextScope(ctx: _productRepository.Context, autoDetectChanges: false, proxyCreation: false, validateOnSave: false))
			{
				var segmenter = context.GetSegmenter<Product>();

				Init(context, _dataExchangeSettings);

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
						ProcessProducts(context, batch, templateViewPaths, srcToDestId);
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

					if (segmenter.HasColumn("LimitedToStores") && segmenter.HasColumn("StoreIds"))
					{
						try
						{
							_productRepository.Context.AutoDetectChangesEnabled = true;
							ProcessStoreMappings(context, batch);
						}
						catch (Exception exception)
						{
							context.Result.AddError(exception, segmenter.CurrentSegment, "ProcessStoreMappings");
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
					if (segmenter.HasColumn("ImageUrls"))
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

				// ===========================================================================
				// 7.) Map parent id of inserted products
				// ===========================================================================
				if (srcToDestId.Any() && segmenter.HasColumn("Id") && segmenter.HasColumn("ParentGroupedProductId"))
				{
					segmenter.Reset();

					while (context.Abort == DataExchangeAbortion.None && segmenter.ReadNextBatch())
					{
						var batch = segmenter.CurrentBatch;

						_productRepository.Context.DetachAll(false);

						try
						{
							ProcessProductMappings(context, batch, srcToDestId);
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


	internal class ImportProductMapping
	{
		public int DestinationId { get; set; }
		public bool Inserted { get; set; }
	}
}
