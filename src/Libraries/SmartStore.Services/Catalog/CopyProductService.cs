using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using SmartStore.Core.Logging;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Localization;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Search;
using SmartStore.Services.Seo;
using SmartStore.Services.Stores;
using SmartStore.Core.Domain.Seo;

namespace SmartStore.Services.Catalog
{
    public partial class CopyProductService : ICopyProductService
    {
		private readonly IRepository<Product> _productRepository;
		private readonly IRepository<RelatedProduct> _relatedProductRepository;
		private readonly IRepository<CrossSellProduct> _crossSellProductRepository;
		private readonly ICommonServices _services;
		private readonly IProductService _productService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IPictureService _pictureService;
        private readonly IDownloadService _downloadService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IUrlRecordService _urlRecordService;
		private readonly IStoreMappingService _storeMappingService;
		private readonly ICatalogSearchService _catalogSearchService;
		private readonly SeoSettings _seoSettings;

		public CopyProductService(
			IRepository<Product> productRepository,
			IRepository<RelatedProduct> relatedProductRepository,
			IRepository<CrossSellProduct> crossSellProductRepository,
			ICommonServices services,
			IProductService productService,
            IProductAttributeService productAttributeService,
			ILanguageService languageService,
            ILocalizedEntityService localizedEntityService,
			IPictureService pictureService,
			IDownloadService downloadService,
            IProductAttributeParser productAttributeParser,
			IUrlRecordService urlRecordService,
			IStoreMappingService storeMappingService,
			ICatalogSearchService catalogSearchService,
			SeoSettings seoSettings)
        {
			_productRepository = productRepository;
			_relatedProductRepository = relatedProductRepository;
			_crossSellProductRepository = crossSellProductRepository;
			_services = services;
			_productService = productService;
            _productAttributeService = productAttributeService;
            _languageService = languageService;
            _localizedEntityService = localizedEntityService;
            _pictureService = pictureService;
            _downloadService = downloadService;
            _productAttributeParser = productAttributeParser;
			_urlRecordService = urlRecordService;
			_storeMappingService = storeMappingService;
			_catalogSearchService = catalogSearchService;
			_seoSettings = seoSettings;

			T = NullLocalizer.Instance;
        }

		public Localizer T { get; set; }

		public virtual Product CopyProduct(
			Product product, 
			string newName, 
			bool isPublished, 
			bool copyImages, 
			bool copyAssociatedProducts = true)
        {
			Guard.NotNull(product, nameof(product));
			Guard.NotEmpty(newName, nameof(newName));

			using (_services.Chronometer.Step("Copy product " + product.Id))
			{
				Product clone = null;
				var utcNow = DateTime.UtcNow;
				var languages = _languageService.GetAllLanguages(true);

				// Media stuff
				int? downloadId = null;
				int? sampleDownloadId = null;
				var clonedPictures = new Dictionary<int, Picture>(); // Key = former ID, Value = cloned picture

				using (var scope = new DbContextScope(ctx: _productRepository.Context,
					autoCommit: false,
					autoDetectChanges: false,
					proxyCreation: true,
					validateOnSave: false,
					forceNoTracking: true,
					hooksEnabled: false))
				{
					if (product.IsDownload)
					{
						downloadId = CopyDownload(product.DownloadId)?.Id;
					}

					if (product.HasSampleDownload)
					{
						sampleDownloadId = CopyDownload(product.SampleDownloadId.GetValueOrDefault())?.Id;
					}

					if (copyImages)
					{
						clonedPictures = CopyPictures(product, newName);
					}

					// Product
					clone = new Product
					{
						ProductTypeId = product.ProductTypeId,
						ParentGroupedProductId = product.ParentGroupedProductId,
						VisibleIndividually = product.VisibleIndividually,
						Name = newName,
						ShortDescription = product.ShortDescription,
						FullDescription = product.FullDescription,
						ProductTemplateId = product.ProductTemplateId,
						AdminComment = product.AdminComment,
						ShowOnHomePage = product.ShowOnHomePage,
						HomePageDisplayOrder = product.HomePageDisplayOrder,
						MetaKeywords = product.MetaKeywords,
						MetaDescription = product.MetaDescription,
						MetaTitle = product.MetaTitle,
						AllowCustomerReviews = product.AllowCustomerReviews,
						LimitedToStores = product.LimitedToStores,
						Sku = product.Sku,
						ManufacturerPartNumber = product.ManufacturerPartNumber,
						Gtin = product.Gtin,
						IsGiftCard = product.IsGiftCard,
						GiftCardType = product.GiftCardType,
						RequireOtherProducts = product.RequireOtherProducts,
						RequiredProductIds = product.RequiredProductIds,
						AutomaticallyAddRequiredProducts = product.AutomaticallyAddRequiredProducts,
						IsDownload = product.IsDownload,
						DownloadId = downloadId ?? 0,
						UnlimitedDownloads = product.UnlimitedDownloads,
						MaxNumberOfDownloads = product.MaxNumberOfDownloads,
						DownloadExpirationDays = product.DownloadExpirationDays,
						DownloadActivationType = product.DownloadActivationType,
						HasSampleDownload = product.HasSampleDownload,
						SampleDownloadId = sampleDownloadId,
						HasUserAgreement = product.HasUserAgreement,
						UserAgreementText = product.UserAgreementText,
						IsRecurring = product.IsRecurring,
						RecurringCycleLength = product.RecurringCycleLength,
						RecurringCyclePeriod = product.RecurringCyclePeriod,
						RecurringTotalCycles = product.RecurringTotalCycles,
						IsShipEnabled = product.IsShipEnabled,
						IsFreeShipping = product.IsFreeShipping,
						AdditionalShippingCharge = product.AdditionalShippingCharge,
						IsEsd = product.IsEsd,
						IsTaxExempt = product.IsTaxExempt,
						TaxCategoryId = product.TaxCategoryId,
						ManageInventoryMethod = product.ManageInventoryMethod,
						StockQuantity = product.StockQuantity,
						DisplayStockAvailability = product.DisplayStockAvailability,
						DisplayStockQuantity = product.DisplayStockQuantity,
						MinStockQuantity = product.MinStockQuantity,
						LowStockActivityId = product.LowStockActivityId,
						NotifyAdminForQuantityBelow = product.NotifyAdminForQuantityBelow,
						BackorderMode = product.BackorderMode,
						AllowBackInStockSubscriptions = product.AllowBackInStockSubscriptions,
						OrderMinimumQuantity = product.OrderMinimumQuantity,
						OrderMaximumQuantity = product.OrderMaximumQuantity,
						QuantityStep = product.QuantityStep,
						QuantiyControlType = product.QuantiyControlType,
						HideQuantityControl = product.HideQuantityControl,
						AllowedQuantities = product.AllowedQuantities,
						DisableBuyButton = product.DisableBuyButton,
						DisableWishlistButton = product.DisableWishlistButton,
						AvailableForPreOrder = product.AvailableForPreOrder,
						CallForPrice = product.CallForPrice,
						Price = product.Price,
						OldPrice = product.OldPrice,
						ProductCost = product.ProductCost,
						SpecialPrice = product.SpecialPrice,
						SpecialPriceStartDateTimeUtc = product.SpecialPriceStartDateTimeUtc,
						SpecialPriceEndDateTimeUtc = product.SpecialPriceEndDateTimeUtc,
						CustomerEntersPrice = product.CustomerEntersPrice,
						MinimumCustomerEnteredPrice = product.MinimumCustomerEnteredPrice,
						MaximumCustomerEnteredPrice = product.MaximumCustomerEnteredPrice,
						LowestAttributeCombinationPrice = product.LowestAttributeCombinationPrice,
						Weight = product.Weight,
						Length = product.Length,
						Width = product.Width,
						Height = product.Height,
						AvailableStartDateTimeUtc = product.AvailableStartDateTimeUtc,
						AvailableEndDateTimeUtc = product.AvailableEndDateTimeUtc,
						DisplayOrder = product.DisplayOrder,
						Published = isPublished,
						Deleted = product.Deleted,
						IsSystemProduct = product.IsSystemProduct,
						DeliveryTimeId = product.DeliveryTimeId,
						QuantityUnitId = product.QuantityUnitId,
						BasePriceEnabled = product.BasePriceEnabled,
						BasePriceMeasureUnit = product.BasePriceMeasureUnit,
						BasePriceAmount = product.BasePriceAmount,
						BasePriceBaseAmount = product.BasePriceBaseAmount,
						BundleTitleText = product.BundleTitleText,
						BundlePerItemShipping = product.BundlePerItemShipping,
						BundlePerItemPricing = product.BundlePerItemPricing,
						BundlePerItemShoppingCart = product.BundlePerItemShoppingCart,
						CustomsTariffNumber = product.CustomsTariffNumber,
						CountryOfOriginId = product.CountryOfOriginId,
						CreatedOnUtc = utcNow,
						UpdatedOnUtc = utcNow
					};

					// Category mappings
					foreach (var pc in product.ProductCategories)
					{
						clone.ProductCategories.Add(new ProductCategory
						{
							CategoryId = pc.CategoryId,
							IsFeaturedProduct = pc.IsFeaturedProduct,
							DisplayOrder = pc.DisplayOrder
						});
					}

					// Manufacturer mappings
					foreach (var pm in product.ProductManufacturers)
					{
						clone.ProductManufacturers.Add(new ProductManufacturer
						{
							ManufacturerId = pm.ManufacturerId,
							IsFeaturedProduct = pm.IsFeaturedProduct,
							DisplayOrder = pm.DisplayOrder
						});
					}

					// Picture mappings
					if (copyImages)
					{
						foreach (var pp in product.ProductPictures)
						{
							var pictureClone = clonedPictures.Get(pp.PictureId);
							if (pictureClone != null)
							{
								clone.ProductPictures.Add(new ProductPicture
								{
									PictureId = pictureClone.Id,
									DisplayOrder = pp.DisplayOrder
								});
							}
						}
					}

					// Product specifications
					foreach (var psa in product.ProductSpecificationAttributes)
					{
						clone.ProductSpecificationAttributes.Add(new ProductSpecificationAttribute
						{
							SpecificationAttributeOptionId = psa.SpecificationAttributeOptionId,
							AllowFiltering = psa.AllowFiltering,
							ShowOnProductPage = psa.ShowOnProductPage,
							DisplayOrder = psa.DisplayOrder
						});
					}

					// Tier prices
					foreach (var tp in product.TierPrices)
					{
						clone.TierPrices.Add(new TierPrice
						{
							StoreId = tp.StoreId,
							CustomerRoleId = tp.CustomerRoleId,
							Quantity = tp.Quantity,
							Price = tp.Price,
							CalculationMethod = tp.CalculationMethod
						});
					}

					// Discount mapping
					foreach (var discount in product.AppliedDiscounts)
					{
						clone.AppliedDiscounts.Add(discount);
					}

					// Tags
					foreach (var tag in product.ProductTags)
					{
						clone.ProductTags.Add(tag);
					}

					// >>>>>>> Put clone to db (from here on we need the product clone's ID)
					_productRepository.Insert(clone);
					Commit();

					// Related products mapping
					foreach (var rp in _productService.GetRelatedProductsByProductId1(product.Id, true))
					{
						_relatedProductRepository.Insert(new RelatedProduct
						{
							ProductId1 = clone.Id,
							ProductId2 = rp.ProductId2,
							DisplayOrder = rp.DisplayOrder
						});
					}

					// CrossSell products mapping
					foreach (var csp in _productService.GetCrossSellProductsByProductId1(product.Id, true))
					{
						_crossSellProductRepository.Insert(new CrossSellProduct
						{
							ProductId1 = clone.Id,
							ProductId2 = csp.ProductId2
						});
					}

					// Store mapping
					var selectedStoreIds = _storeMappingService.GetStoresIdsWithAccess(product);
					foreach (var id in selectedStoreIds)
					{
						_storeMappingService.InsertStoreMapping(clone, id);
					}

					// SEO
					ProcessSlug(clone);

					// Localization
					ProcessLocalization(product, clone, languages);

					// Attr stuff ...
					ProcessAttributes(product, clone, newName, copyImages, clonedPictures, languages);

					// update computed properties
					clone.HasTierPrices = clone.TierPrices.Count > 0;
					clone.HasDiscountsApplied = clone.AppliedDiscounts.Count > 0;
					clone.LowestAttributeCombinationPrice = _productAttributeService.GetLowestCombinationPrice(clone.Id);
					clone.MainPictureId = clone.ProductPictures.OrderBy(x => x.DisplayOrder).Select(x => x.PictureId).FirstOrDefault();

					// Associated products
					if (copyAssociatedProducts && product.ProductType != ProductType.BundledProduct)
					{
						ProcessAssociatedProducts(product, clone, isPublished, copyImages);
					}

					// Bundle items
					ProcessBundleItems(product, clone);

					// >>>>>>> Our final commit
					Commit();
				}

				return clone;
			}
        }

		private void Commit()
		{
			_services.DbContext.SaveChanges();
		}

		private Download CopyDownload(int downloadId)
		{
			var download = _downloadService.GetDownloadById(downloadId);

			if (download == null)
			{
				return null;
			}

			var clone = new Download
			{
				DownloadGuid = Guid.NewGuid(),
				UseDownloadUrl = download.UseDownloadUrl,
				DownloadUrl = download.DownloadUrl,
				ContentType = download.ContentType,
				Filename = download.Filename,
				Extension = download.Extension,
				IsNew = download.IsNew,
				UpdatedOnUtc = DateTime.UtcNow
			};

			using (var scope = new DbContextScope(ctx: _productRepository.Context, autoCommit: true))
			{
				_downloadService.InsertDownload(clone, download.MediaStorage?.Data);
			}
				
			return clone;
		}

		private Dictionary<int, Picture> CopyPictures(Product product, string newProductName)
		{
			var clonedPictures = new Dictionary<int, Picture>();
			var seoFilename = _pictureService.GetPictureSeName(newProductName);

			foreach (var pp in product.ProductPictures)
			{
				var clone = CopyPicture(pp.Picture, seoFilename);
				clonedPictures[pp.PictureId] = clone;
			}

			return clonedPictures;
		}

		private Picture CopyPicture(Picture picture, string seoFilename)
		{
			using (var scope = new DbContextScope(ctx: _productRepository.Context, autoCommit: true))
			{
				var buffer = _pictureService.LoadPictureBinary(picture);

				var clone = _pictureService.InsertPicture(
					buffer,
					picture.MimeType,
					seoFilename,
					true,
					picture.Width ?? 0,
					picture.Height ?? 0,
					false);

				return clone;
			}	
		}

		private void ProcessSlug(Product clone)
		{
			using (var scope = new DbContextScope(ctx: _productRepository.Context, autoCommit: true))
			{
				var slug = clone.ValidateSeName("", clone.Name, true, _urlRecordService, _seoSettings);
				_urlRecordService.SaveSlug(clone, slug, 0);
			}
		}

		private void ProcessLocalization(Product product, Product clone, IEnumerable<Language> languages)
		{
			using (var scope = new DbContextScope(ctx: _productRepository.Context, autoCommit: true))
			{
				foreach (var lang in languages)
				{
					var name = product.GetLocalized(x => x.Name, lang, false, false);
					if (!String.IsNullOrEmpty(name))
						_localizedEntityService.SaveLocalizedValue(clone, x => x.Name, name, lang.Id);

					var shortDescription = product.GetLocalized(x => x.ShortDescription, lang, false, false);
					if (!String.IsNullOrEmpty(shortDescription))
						_localizedEntityService.SaveLocalizedValue(clone, x => x.ShortDescription, shortDescription, lang.Id);

					var fullDescription = product.GetLocalized(x => x.FullDescription, lang, false, false);
					if (!String.IsNullOrEmpty(fullDescription))
						_localizedEntityService.SaveLocalizedValue(clone, x => x.FullDescription, fullDescription, lang.Id);

					var metaKeywords = product.GetLocalized(x => x.MetaKeywords, lang, false, false);
					if (!String.IsNullOrEmpty(metaKeywords))
						_localizedEntityService.SaveLocalizedValue(clone, x => x.MetaKeywords, metaKeywords, lang.Id);

					var metaDescription = product.GetLocalized(x => x.MetaDescription, lang, false, false);
					if (!String.IsNullOrEmpty(metaDescription))
						_localizedEntityService.SaveLocalizedValue(clone, x => x.MetaDescription, metaDescription, lang.Id);

					var metaTitle = product.GetLocalized(x => x.MetaTitle, lang, false, false);
					if (!String.IsNullOrEmpty(metaTitle))
						_localizedEntityService.SaveLocalizedValue(clone, x => x.MetaTitle, metaTitle, lang.Id);

					var bundleTitleText = product.GetLocalized(x => x.BundleTitleText, lang, false, false);
					if (!String.IsNullOrEmpty(bundleTitleText))
						_localizedEntityService.SaveLocalizedValue(clone, x => x.BundleTitleText, bundleTitleText, lang.Id);

					// Search engine name.
					var slug = clone.ValidateSeName("", name, false, _urlRecordService, _seoSettings, lang.Id);
					_urlRecordService.SaveSlug(clone, slug, lang.Id);
				}
			}
		}

		private void ProcessAssociatedProducts(Product product, Product clone, bool isPublished, bool copyImages)
		{
			var copyOf = T("Admin.Common.CopyOf");
			var searchQuery = new CatalogSearchQuery().HasParentGroupedProduct(product.Id);

			var query = _catalogSearchService.PrepareQuery(searchQuery);
			var associatedProducts = query.OrderBy(p => p.DisplayOrder).ToList();

			foreach (var associatedProduct in associatedProducts)
			{
				var associatedProductCopy = CopyProduct(associatedProduct, $"{copyOf} {associatedProduct.Name}", isPublished, copyImages, false);
				associatedProductCopy.ParentGroupedProductId = clone.Id;
			}
		}

		private void ProcessBundleItems(Product product, Product clone)
		{
			var bundledItems = _productService.GetBundleItems(product.Id, true);

			foreach (var bundleItem in bundledItems)
			{
				var newBundleItem = bundleItem.Item.Clone();
				newBundleItem.BundleProductId = clone.Id;

				_productService.InsertBundleItem(newBundleItem);

				foreach (var itemFilter in bundleItem.Item.AttributeFilters)
				{
					var newItemFilter = itemFilter.Clone();
					newItemFilter.BundleItemId = newBundleItem.Id;

					_productAttributeService.InsertProductBundleItemAttributeFilter(newItemFilter);
				}
			}
		}

		private void ProcessAttributes(
			Product product, 
			Product clone, 
			string newName, 
			bool copyImages,
			Dictionary<int, Picture> clonedPictures,
			IEnumerable<Language> languages)
		{
			// Former attribute id > clone
			var pvaMap = new Dictionary<int, ProductVariantAttribute>();

			// Former attribute value id > clone
			var pvavMap = new Dictionary<int, ProductVariantAttributeValue>();

			var pictureSeName = _pictureService.GetPictureSeName(newName);

			foreach (var pva in product.ProductVariantAttributes)
			{
				var pvaClone = new ProductVariantAttribute
				{
					ProductAttributeId = pva.ProductAttributeId,
					TextPrompt = pva.TextPrompt,
					IsRequired = pva.IsRequired,
					AttributeControlTypeId = pva.AttributeControlTypeId,
					DisplayOrder = pva.DisplayOrder
				};

				clone.ProductVariantAttributes.Add(pvaClone);

				// Save associated value (used for combinations copying)
				pvaMap[pva.Id] = pvaClone;

				// Product variant attribute values
				foreach (var pvav in pva.ProductVariantAttributeValues)
				{
					var pvavClone = new ProductVariantAttributeValue
					{
						Name = pvav.Name,
						Color = pvav.Color,
						PriceAdjustment = pvav.PriceAdjustment,
						WeightAdjustment = pvav.WeightAdjustment,
						IsPreSelected = pvav.IsPreSelected,
						DisplayOrder = pvav.DisplayOrder,
						ValueTypeId = pvav.ValueTypeId,
						LinkedProductId = pvav.LinkedProductId,
						Quantity = pvav.Quantity,
						PictureId = copyImages ? pvav.PictureId : 0 // we'll clone this later
					};

					pvaClone.ProductVariantAttributeValues.Add(pvavClone);

					// Save associated value (used for combinations copying)
					pvavMap.Add(pvav.Id, pvavClone);
				}
			}

			// >>>>>> Commit
			Commit();

			// Attribute value localization
			foreach (var pvav in product.ProductVariantAttributes.SelectMany(x => x.ProductVariantAttributeValues).ToArray())
			{
				foreach (var lang in languages)
				{
					var name = pvav.GetLocalized(x => x.Name, lang, false, false);
					if (!String.IsNullOrEmpty(name))
					{
						var pvavClone = pvavMap.Get(pvav.Id);
						if (pvavClone != null)
						{
							_localizedEntityService.SaveLocalizedValue(pvavClone, x => x.Name, name, lang.Id);
						}
					}					
				}
			}			

			// Clone attribute value images
			if (copyImages)
			{
				// Reduce value set to those with assigned pictures
				var allValueClonesWithPictures = pvavMap.Values.Where(x => x.PictureId > 0).ToArray();
				// Get those pictures for cloning
				var allPictures = _pictureService.GetPicturesByIds(allValueClonesWithPictures.Select(x => x.PictureId).ToArray(), true);

				foreach (var pvavClone in allValueClonesWithPictures)
				{
					var picture = allPictures.FirstOrDefault(x => x.Id == pvavClone.PictureId);
					if (picture != null)
					{
						var pictureClone = CopyPicture(picture, pictureSeName);
						clonedPictures[pvavClone.PictureId] = pictureClone;
						pvavClone.PictureId = pictureClone.Id;
					}
				}
			}

			// >>>>>> Commit attributes & values
			Commit();

			// attribute combinations
			using (var scope = new DbContextScope(lazyLoading: false, forceNoTracking: false))
			{
				scope.LoadCollection(product, (Product p) => p.ProductVariantAttributeCombinations);
			}

			foreach (var combination in product.ProductVariantAttributeCombinations)
			{
				// Generate new AttributesXml according to new value IDs
				string newAttributesXml = "";
				var parsedProductVariantAttributes = _productAttributeParser.ParseProductVariantAttributes(combination.AttributesXml);
				foreach (var oldPva in parsedProductVariantAttributes)
				{
					if (!pvaMap.ContainsKey(oldPva.Id))
						continue;

					var newPva = pvaMap.Get(oldPva.Id);

					if (newPva == null)
						continue;

					var oldPvaValuesStr = _productAttributeParser.ParseValues(combination.AttributesXml, oldPva.Id);
					foreach (var oldPvaValueStr in oldPvaValuesStr)
					{
						if (newPva.ShouldHaveValues())
						{
							// attribute values
							int oldPvaValue = oldPvaValueStr.Convert<int>();
							if (pvavMap.ContainsKey(oldPvaValue))
							{
								var newPvav = pvavMap.Get(oldPvaValue);
								if (newPvav != null)
								{
									newAttributesXml = _productAttributeParser.AddProductAttribute(newAttributesXml, newPva, newPvav.Id.ToString());
								}
							}
						}
						else
						{
							// just a text
							newAttributesXml = _productAttributeParser.AddProductAttribute(newAttributesXml, newPva, oldPvaValueStr);
						}
					}
				}

				var newAssignedPictureIds = new HashSet<string>();
				foreach (var strPicId in combination.AssignedPictureIds.EmptyNull().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
				{
					var newPic = clonedPictures.Get(strPicId.Convert<int>());
					if (newPic != null)
					{
						newAssignedPictureIds.Add(newPic.Id.ToString(CultureInfo.InvariantCulture));
					}
				}

				var combinationClone = new ProductVariantAttributeCombination
				{
					AttributesXml = newAttributesXml,
					StockQuantity = combination.StockQuantity,
					AllowOutOfStockOrders = combination.AllowOutOfStockOrders,
					Sku = combination.Sku,
					Gtin = combination.Gtin,
					ManufacturerPartNumber = combination.ManufacturerPartNumber,
					Price = combination.Price,
					AssignedPictureIds = copyImages ? String.Join(",", newAssignedPictureIds) : null,
					Length = combination.Length,
					Width = combination.Width,
					Height = combination.Height,
					BasePriceAmount = combination.BasePriceAmount,
					BasePriceBaseAmount = combination.BasePriceBaseAmount,
					DeliveryTimeId = combination.DeliveryTimeId,
					QuantityUnitId = combination.QuantityUnitId,
					IsActive = combination.IsActive
					//IsDefaultCombination = combination.IsDefaultCombination
				};

				clone.ProductVariantAttributeCombinations.Add(combinationClone);
			}

			// >>>>>> Commit combinations
			Commit();
		}
	}
}
