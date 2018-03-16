using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using SmartStore.Core.Async;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Events;
using SmartStore.Data.Utilities;
using SmartStore.Services.DataExchange.Import;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Utilities;

namespace SmartStore.Services.Catalog.Importer
{
	public class ProductImporter : EntityImporterBase
	{
		private readonly IRepository<ProductPicture> _productPictureRepository;
		private readonly IRepository<ProductManufacturer> _productManufacturerRepository;
		private readonly IRepository<ProductCategory> _productCategoryRepository;
		private readonly IRepository<Product> _productRepository;
		private readonly ICommonServices _services;
		private readonly ILocalizedEntityService _localizedEntityService;
		private readonly IPictureService _pictureService;
		private readonly IManufacturerService _manufacturerService;
		private readonly ICategoryService _categoryService;
		private readonly IProductService _productService;
		private readonly IProductTemplateService _productTemplateService;
		private readonly FileDownloadManager _fileDownloadManager;

		private static readonly Dictionary<string, Expression<Func<Product, string>>> _localizableProperties = new Dictionary<string, Expression<Func<Product, string>>>
		{
			{ "Name", x => x.Name },
			{ "ShortDescription", x => x.ShortDescription },
			{ "FullDescription", x => x.FullDescription },
			{ "MetaKeywords", x => x.MetaKeywords },
			{ "MetaDescription", x => x.MetaDescription },
			{ "MetaTitle", x => x.MetaTitle },
			{ "BundleTitleText", x => x.BundleTitleText }
		};

		public ProductImporter(
			IRepository<ProductPicture> productPictureRepository,
			IRepository<ProductManufacturer> productManufacturerRepository,
			IRepository<ProductCategory> productCategoryRepository,
			IRepository<Product> productRepository,
			ICommonServices services,
			ILocalizedEntityService localizedEntityService,
			IPictureService pictureService,
			IManufacturerService manufacturerService,
			ICategoryService categoryService,
			IProductService productService,
			IProductTemplateService productTemplateService,
			FileDownloadManager fileDownloadManager)
		{
			_productPictureRepository = productPictureRepository;
			_productManufacturerRepository = productManufacturerRepository;
			_productCategoryRepository = productCategoryRepository;
			_productRepository = productRepository;
			_services = services;
			_localizedEntityService = localizedEntityService;
			_pictureService = pictureService;
			_manufacturerService = manufacturerService;
			_categoryService = categoryService;
			_productService = productService;
			_productTemplateService = productTemplateService;
			_fileDownloadManager = fileDownloadManager;
		}

		protected override void Import(ImportExecuteContext context)
		{
			var srcToDestId = new Dictionary<int, ImportProductMapping>();
			var importStartTime = DateTime.UtcNow;
			var templateViewPaths = _productTemplateService.GetAllProductTemplates().ToDictionarySafe(x => x.ViewPath, x => x.Id);

			using (var scope = new DbContextScope(ctx: _productRepository.Context, hooksEnabled: false, autoDetectChanges: false, proxyCreation: false, validateOnSave: false))
			{
				var segmenter = context.DataSegmenter;

				Initialize(context);

				while (context.Abort == DataExchangeAbortion.None && segmenter.ReadNextBatch())
				{
					var batch = segmenter.GetCurrentBatch<Product>();

					// Perf: detach entities
					_productRepository.Context.DetachEntities(x =>
					{
						return x is Product || x is UrlRecord || x is StoreMapping || x is ProductVariantAttribute || x is LocalizedProperty ||
							   x is ProductBundleItem || x is ProductCategory || x is ProductManufacturer || x is Category || x is Manufacturer ||
							   x is ProductPicture || x is Picture || x is ProductTag || x is TierPrice;
					});
					//_productRepository.Context.DetachAll(true);

					context.SetProgress(segmenter.CurrentSegmentFirstRowIndex - 1, segmenter.TotalRows);

					// ===========================================================================
					// 1.) Import products
					// ===========================================================================
					int savedProducts = 0;
					try
					{
						savedProducts = ProcessProducts(context, batch, templateViewPaths, srcToDestId);
					}
					catch (Exception exception)
					{
						context.Result.AddError(exception, segmenter.CurrentSegment, "ProcessProducts");
					}

					// reduce batch to saved (valid) products.
					// No need to perform import operations on errored products.
					batch = batch.Where(x => x.Entity != null && !x.IsTransient).ToArray();

					// update result object
					context.Result.NewRecords += batch.Count(x => x.IsNew);
					context.Result.ModifiedRecords += Math.Max(0, savedProducts - context.Result.NewRecords);

					// ===========================================================================
					// 2.) Import SEO Slugs
					// IMPORTANT: Unlike with Products AutoCommitEnabled must be TRUE,
					//            as Slugs are going to be validated against existing ones in DB.
					// ===========================================================================
					if (segmenter.HasColumn("SeName", true) || batch.Any(x => x.IsNew || x.NameChanged))
					{
						try
						{
							_productRepository.Context.AutoDetectChangesEnabled = true;
							ProcessSlugs(context, batch, typeof(Product).Name);
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
					// 3.) Import StoreMappings
					// ===========================================================================
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

					// ===========================================================================
					// 4.) Import Localizations
					// ===========================================================================
					try
					{
						ProcessLocalizations(context, batch, _localizableProperties);
					}
					catch (Exception exception)
					{
						context.Result.AddError(exception, segmenter.CurrentSegment, "ProcessLocalizations");
					}

					// ===========================================================================
					// 5.) Import product category mappings
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
					// 6.) Import product manufacturer mappings
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
					// 7.) Import product picture mappings
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
				// 8.) Map parent id of inserted products
				// ===========================================================================
				if (srcToDestId.Any() && segmenter.HasColumn("Id") && segmenter.HasColumn("ParentGroupedProductId") && !segmenter.IsIgnored("ParentGroupedProductId"))
				{
					segmenter.Reset();

					while (context.Abort == DataExchangeAbortion.None && segmenter.ReadNextBatch())
					{
						var batch = segmenter.GetCurrentBatch<Product>();

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

				// ===========================================================================
				// 9.) PostProcess: normalization
				// ===========================================================================
				DataMigrator.FixProductMainPictureIds(_productRepository.Context, importStartTime);
			}
		}

		protected virtual int ProcessProducts(
			ImportExecuteContext context,
			IEnumerable<ImportRow<Product>> batch,
			Dictionary<string, int> templateViewPaths,
			Dictionary<int, ImportProductMapping> srcToDestId)
		{
			_productRepository.AutoCommitEnabled = false;

			var defaultTemplateId = templateViewPaths["Product"];
            
            foreach (var row in batch)
			{
				Product product = null;
				var id = row.GetDataValue<int>("Id");
				
				foreach (var keyName in context.KeyFieldNames)
				{
					var keyValue = row.GetDataValue<string>(keyName);

					if (keyValue.HasValue() || id > 0)
					{
						switch (keyName)
						{
							case "Id":
								product = _productRepository.GetById(id); // get it uncached
								break;
							case "Sku":
								product = _productService.GetProductBySku(keyValue);
								break;
							case "Gtin":
								product = _productService.GetProductByGtin(keyValue);
								break;
							case "ManufacturerPartNumber":
								product = _productService.GetProductByManufacturerPartNumber(keyValue);
								break;
							case "Name":
								product = _productService.GetProductByName(keyValue);
								break;
						}
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

					// a Name is required for new products.
					if (!row.HasDataValue("Name"))
					{
						++context.Result.SkippedRecords;
						context.Result.AddError("The 'Name' field is required for new products. Skipping row.", row.GetRowInfo(), "Name");
						continue;
					}

					product = new Product();
				}

				var name = row.GetDataValue<string>("Name");

				row.Initialize(product, name ?? product.Name);

				if (!row.IsNew)
				{
					if (!product.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
					{
						// Perf: use this later for SeName updates.
						row.NameChanged = true;
					}
				}

				row.SetProperty(context.Result, (x) => x.ProductTypeId, (int)ProductType.SimpleProduct);
				row.SetProperty(context.Result, (x) => x.VisibleIndividually, true);
				row.SetProperty(context.Result, (x) => x.Name);
				row.SetProperty(context.Result, (x) => x.ShortDescription);
				row.SetProperty(context.Result, (x) => x.FullDescription);
				row.SetProperty(context.Result, (x) => x.AdminComment);
				row.SetProperty(context.Result, (x) => x.ShowOnHomePage);
				row.SetProperty(context.Result, (x) => x.HomePageDisplayOrder);
				row.SetProperty(context.Result, (x) => x.MetaKeywords);
				row.SetProperty(context.Result, (x) => x.MetaDescription);
				row.SetProperty(context.Result, (x) => x.MetaTitle);
				row.SetProperty(context.Result, (x) => x.AllowCustomerReviews, true);
				row.SetProperty(context.Result, (x) => x.ApprovedRatingSum);
				row.SetProperty(context.Result, (x) => x.NotApprovedRatingSum);
				row.SetProperty(context.Result, (x) => x.ApprovedTotalReviews);
				row.SetProperty(context.Result, (x) => x.NotApprovedTotalReviews);
				row.SetProperty(context.Result, (x) => x.Published, true);
				row.SetProperty(context.Result, (x) => x.Sku);
				row.SetProperty(context.Result, (x) => x.ManufacturerPartNumber);
				row.SetProperty(context.Result, (x) => x.Gtin);
				row.SetProperty(context.Result, (x) => x.IsGiftCard);
				row.SetProperty(context.Result, (x) => x.GiftCardTypeId);
				row.SetProperty(context.Result, (x) => x.RequireOtherProducts);
				row.SetProperty(context.Result, (x) => x.RequiredProductIds);	// TODO: global scope
				row.SetProperty(context.Result, (x) => x.AutomaticallyAddRequiredProducts);
				row.SetProperty(context.Result, (x) => x.IsDownload);
				row.SetProperty(context.Result, (x) => x.DownloadId);
				row.SetProperty(context.Result, (x) => x.UnlimitedDownloads, true);
				row.SetProperty(context.Result, (x) => x.MaxNumberOfDownloads, 10);
				row.SetProperty(context.Result, (x) => x.DownloadExpirationDays);
				row.SetProperty(context.Result, (x) => x.DownloadActivationTypeId, 1);
				row.SetProperty(context.Result, (x) => x.HasSampleDownload);
				row.SetProperty(context.Result, (x) => x.SampleDownloadId, (int?)null, ZeroToNull);    // TODO: global scope
				row.SetProperty(context.Result, (x) => x.HasUserAgreement);
				row.SetProperty(context.Result, (x) => x.UserAgreementText);
				row.SetProperty(context.Result, (x) => x.IsRecurring);
				row.SetProperty(context.Result, (x) => x.RecurringCycleLength, 100);
				row.SetProperty(context.Result, (x) => x.RecurringCyclePeriodId);
				row.SetProperty(context.Result, (x) => x.RecurringTotalCycles, 10);
				row.SetProperty(context.Result, (x) => x.IsShipEnabled, true);
				row.SetProperty(context.Result, (x) => x.IsFreeShipping);
				row.SetProperty(context.Result, (x) => x.AdditionalShippingCharge);
				row.SetProperty(context.Result, (x) => x.IsEsd);
				row.SetProperty(context.Result, (x) => x.IsTaxExempt);
				row.SetProperty(context.Result, (x) => x.TaxCategoryId, 1);    // TODO: global scope
				row.SetProperty(context.Result, (x) => x.ManageInventoryMethodId);
				row.SetProperty(context.Result, (x) => x.StockQuantity, 10000);
				row.SetProperty(context.Result, (x) => x.DisplayStockAvailability);
				row.SetProperty(context.Result, (x) => x.DisplayStockQuantity);
				row.SetProperty(context.Result, (x) => x.MinStockQuantity);
				row.SetProperty(context.Result, (x) => x.LowStockActivityId);
				row.SetProperty(context.Result, (x) => x.NotifyAdminForQuantityBelow, 1);
				row.SetProperty(context.Result, (x) => x.BackorderModeId);
				row.SetProperty(context.Result, (x) => x.AllowBackInStockSubscriptions);
				row.SetProperty(context.Result, (x) => x.OrderMinimumQuantity, 1);
				row.SetProperty(context.Result, (x) => x.OrderMaximumQuantity, 100);
				row.SetProperty(context.Result, (x) => x.QuantityStep, 1);
				row.SetProperty(context.Result, (x) => x.QuantiyControlType);
				row.SetProperty(context.Result, (x) => x.HideQuantityControl);
                row.SetProperty(context.Result, (x) => x.AllowedQuantities);
				row.SetProperty(context.Result, (x) => x.DisableBuyButton);
				row.SetProperty(context.Result, (x) => x.DisableWishlistButton);
				row.SetProperty(context.Result, (x) => x.AvailableForPreOrder);
				row.SetProperty(context.Result, (x) => x.CallForPrice);
				row.SetProperty(context.Result, (x) => x.Price);
				row.SetProperty(context.Result, (x) => x.OldPrice);
				row.SetProperty(context.Result, (x) => x.ProductCost);
				row.SetProperty(context.Result, (x) => x.SpecialPrice);
				row.SetProperty(context.Result, (x) => x.SpecialPriceStartDateTimeUtc);
				row.SetProperty(context.Result, (x) => x.SpecialPriceEndDateTimeUtc);
				row.SetProperty(context.Result, (x) => x.CustomerEntersPrice);
				row.SetProperty(context.Result, (x) => x.MinimumCustomerEnteredPrice);
				row.SetProperty(context.Result, (x) => x.MaximumCustomerEnteredPrice, 1000);
				// HasTierPrices... ignore as long as no tier prices are imported
				// LowestAttributeCombinationPrice... ignore as long as no combinations are imported
				row.SetProperty(context.Result, (x) => x.Weight);
				row.SetProperty(context.Result, (x) => x.Length);
				row.SetProperty(context.Result, (x) => x.Width);
				row.SetProperty(context.Result, (x) => x.Height);
				row.SetProperty(context.Result, (x) => x.DisplayOrder);
				row.SetProperty(context.Result, (x) => x.DeliveryTimeId);      // TODO: global scope
				row.SetProperty(context.Result, (x) => x.QuantityUnitId);      // TODO: global scope
				row.SetProperty(context.Result, (x) => x.BasePriceEnabled);
				row.SetProperty(context.Result, (x) => x.BasePriceMeasureUnit);
				row.SetProperty(context.Result, (x) => x.BasePriceAmount);
				row.SetProperty(context.Result, (x) => x.BasePriceBaseAmount);
				row.SetProperty(context.Result, (x) => x.BundleTitleText);
				row.SetProperty(context.Result, (x) => x.BundlePerItemShipping);
				row.SetProperty(context.Result, (x) => x.BundlePerItemPricing);
				row.SetProperty(context.Result, (x) => x.BundlePerItemShoppingCart);
				row.SetProperty(context.Result, (x) => x.AvailableStartDateTimeUtc);
				row.SetProperty(context.Result, (x) => x.AvailableEndDateTimeUtc);
				// With new entities, "LimitedToStores" is an implicit field, meaning
				// it has to be set to true by code if it's absent but "StoreIds" exists.
				row.SetProperty(context.Result, (x) => x.LimitedToStores, !row.GetDataValue<List<int>>("StoreIds").IsNullOrEmpty());
				row.SetProperty(context.Result, (x) => x.CustomsTariffNumber);
				row.SetProperty(context.Result, (x) => x.CountryOfOriginId);

				string tvp;
				if (row.TryGetDataValue("ProductTemplateViewPath", out tvp, row.IsTransient))
				{
					product.ProductTemplateId = (tvp.HasValue() && templateViewPaths.ContainsKey(tvp) ? templateViewPaths[tvp] : defaultTemplateId);
				}

				if (id != 0 && !srcToDestId.ContainsKey(id))
				{
					srcToDestId.Add(id, new ImportProductMapping { Inserted = row.IsTransient });
				}

				if (row.IsTransient)
				{
					_productRepository.Insert(product);
				}
				else
				{
					//_productRepository.Update(product); // unnecessary: we use DetectChanges()
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

			return num;
		}

		protected virtual int ProcessProductMappings(
			ImportExecuteContext context,
			IEnumerable<ImportRow<Product>> batch,
			Dictionary<int, ImportProductMapping> srcToDestId)
		{
			_productRepository.AutoCommitEnabled = false;

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
							//_productRepository.Update(product);  // unnecessary: we use DetectChanges()
						}
					}
				}
			}

			var num = _productRepository.Context.SaveChanges();

			return num;
		}

		protected virtual void ProcessProductPictures(ImportExecuteContext context, IEnumerable<ImportRow<Product>> batch)
		{
			// true, cause pictures must be saved and assigned an id prior adding a mapping.
			_productPictureRepository.AutoCommitEnabled = true;

			var equalPictureId = 0;
			var numberOfPictures = (context.ExtraData.NumberOfPictures ?? int.MaxValue);

			foreach (var row in batch)
			{
				var imageUrls = row.GetDataValue<List<string>>("ImageUrls");
				if (imageUrls.IsNullOrEmpty())
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

					if (imageFiles.Count >= numberOfPictures)
						break;
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
								var currentProductPictures = _productPictureRepository.TableUntracked
									.Expand(x => x.Picture)
									.Expand(x => x.Picture.MediaStorage)
									.Where(x => x.ProductId == row.Entity.Id)
									.ToList();

								var currentPictures = currentProductPictures
									.Select(x => x.Picture)
									.ToList();

								if (displayOrder == -1)
								{
									displayOrder = (currentProductPictures.Any() ? currentProductPictures.Select(x => x.DisplayOrder).Max() : 0);
								}

								var size = Size.Empty;
								pictureBinary = _pictureService.ValidatePicture(pictureBinary, image.MimeType, out size);
								pictureBinary = _pictureService.FindEqualPicture(pictureBinary, currentPictures, out equalPictureId);

								if (pictureBinary != null && pictureBinary.Length > 0)
								{
									// no equal picture found in sequence
									var newPicture = _pictureService.InsertPicture(pictureBinary, image.MimeType, seoName, true, size.Width, size.Height, false);
									if (newPicture != null)
									{
										var mapping = new ProductPicture
										{
											ProductId = row.Entity.Id,
											PictureId = newPicture.Id,
											DisplayOrder = ++displayOrder
										};

										_productPictureRepository.Insert(mapping);
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
		}

		protected virtual int ProcessProductManufacturers(ImportExecuteContext context, IEnumerable<ImportRow<Product>> batch)
		{
			_productManufacturerRepository.AutoCommitEnabled = false;

			foreach (var row in batch)
			{
				var manufacturerIds = row.GetDataValue<List<int>>("ManufacturerIds");
				if (!manufacturerIds.IsNullOrEmpty())
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

			return num;
		}

		protected virtual int ProcessProductCategories(ImportExecuteContext context, IEnumerable<ImportRow<Product>> batch)
		{
			_productCategoryRepository.AutoCommitEnabled = false;

			foreach (var row in batch)
			{
				var categoryIds = row.GetDataValue<List<int>>("CategoryIds");
				if (!categoryIds.IsNullOrEmpty())
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
			return num;
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
				return new string[] { "Sku", "Gtin", "ManufacturerPartNumber" };
			}
		}

		public class ImportProductMapping
		{
			public int DestinationId { get; set; }
			public bool Inserted { get; set; }
		}
	}
}
