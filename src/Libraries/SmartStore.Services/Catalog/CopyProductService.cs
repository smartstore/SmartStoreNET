using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Media;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Search;
using SmartStore.Services.Seo;
using SmartStore.Services.Stores;

namespace SmartStore.Services.Catalog
{
    /// <summary>
    /// Copy Product service
    /// </summary>
    public partial class CopyProductService : ICopyProductService
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IPictureService _pictureService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IDownloadService _downloadService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IUrlRecordService _urlRecordService;
		private readonly IStoreMappingService _storeMappingService;
		private readonly ILocalizationService _localizationService;
		private readonly ICatalogSearchService _catalogSearchService;

		#endregion

		#region Ctor

		public CopyProductService(
			IProductService productService,
            IProductAttributeService productAttributeService,
			ILanguageService languageService,
            ILocalizedEntityService localizedEntityService,
			IPictureService pictureService,
            ICategoryService categoryService,
			IManufacturerService manufacturerService,
            ISpecificationAttributeService specificationAttributeService,
			IDownloadService downloadService,
            IProductAttributeParser productAttributeParser,
			IUrlRecordService urlRecordService,
			IStoreMappingService storeMappingService,
			ILocalizationService localizationService,
			ICatalogSearchService catalogSearchService)
        {
            _productService = productService;
            _productAttributeService = productAttributeService;
            _languageService = languageService;
            _localizedEntityService = localizedEntityService;
            _pictureService = pictureService;
            _categoryService = categoryService;
            _manufacturerService = manufacturerService;
            _specificationAttributeService = specificationAttributeService;
            _downloadService = downloadService;
            _productAttributeParser = productAttributeParser;
			_urlRecordService = urlRecordService;
			_storeMappingService = storeMappingService;
			_localizationService = localizationService;
			_catalogSearchService = catalogSearchService;
        }

        #endregion

        #region Methods

        /// <summary>
		/// Create a copy of product with all depended data
        /// </summary>
        /// <param name="product">The product</param>
        /// <param name="newName">The name of product duplicate</param>
        /// <param name="isPublished">A value indicating whether the product duplicate should be published</param>
        /// <param name="copyImages">A value indicating whether the product images should be copied</param>
		/// <param name="copyAssociatedProducts">A value indicating whether the copy associated products</param>
        /// <returns>Product entity</returns>
		public virtual Product CopyProduct(Product product, string newName, bool isPublished, bool copyImages, bool copyAssociatedProducts = true)
        {
			if (product == null)
				throw new ArgumentNullException("product");

			if (String.IsNullOrEmpty(newName))
				throw new ArgumentException("Product name is required");

            Product productCopy = null;
			var utcNow = DateTime.UtcNow;

			// product download & sample download
			int downloadId = 0;
			int? sampleDownloadId = null;

			if (product.IsDownload)
			{
				var download = _downloadService.GetDownloadById(product.DownloadId);
				if (download != null)
				{
					var downloadCopy = new Download
					{
						DownloadGuid = Guid.NewGuid(),
						UseDownloadUrl = download.UseDownloadUrl,
						DownloadUrl = download.DownloadUrl,
						ContentType = download.ContentType,
						Filename = download.Filename,
						Extension = download.Extension,
						IsNew = download.IsNew,
						UpdatedOnUtc = utcNow
					};

					if ((download.MediaStorageId ?? 0) != 0 && download.MediaStorage != null)
						_downloadService.InsertDownload(downloadCopy, download.MediaStorage.Data);
					else
						_downloadService.InsertDownload(downloadCopy, null);

					downloadId = downloadCopy.Id;
				}

				if (product.HasSampleDownload)
				{
					var sampleDownload = _downloadService.GetDownloadById(product.SampleDownloadId.GetValueOrDefault());
					if (sampleDownload != null)
					{
						var sampleDownloadCopy = new Download
						{
							DownloadGuid = Guid.NewGuid(),
							UseDownloadUrl = sampleDownload.UseDownloadUrl,
							DownloadUrl = sampleDownload.DownloadUrl,
							ContentType = sampleDownload.ContentType,
							Filename = sampleDownload.Filename,
							Extension = sampleDownload.Extension,
							IsNew = sampleDownload.IsNew,
							UpdatedOnUtc = utcNow
						};

						if ((sampleDownload.MediaStorageId ?? 0) != 0 && sampleDownload.MediaStorage != null)
							_downloadService.InsertDownload(sampleDownloadCopy, sampleDownload.MediaStorage.Data);
						else
							_downloadService.InsertDownload(sampleDownloadCopy, null);

						sampleDownloadId = sampleDownloadCopy.Id;
					}
				}
			}

            // product
            productCopy = new Product
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
				DownloadId = downloadId,
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
				CountryOfOriginId = product.CountryOfOriginId
            };

            _productService.InsertProduct(productCopy);

            //search engine name
            _urlRecordService.SaveSlug(productCopy, productCopy.ValidateSeName("", productCopy.Name, true), 0);

            var languages = _languageService.GetAllLanguages(true);

            //localization
            foreach (var lang in languages)
            {
                var name = product.GetLocalized(x => x.Name, lang.Id, false, false);
                if (!String.IsNullOrEmpty(name))
                    _localizedEntityService.SaveLocalizedValue(productCopy, x => x.Name, name, lang.Id);

                var shortDescription = product.GetLocalized(x => x.ShortDescription, lang.Id, false, false);
                if (!String.IsNullOrEmpty(shortDescription))
                    _localizedEntityService.SaveLocalizedValue(productCopy, x => x.ShortDescription, shortDescription, lang.Id);

                var fullDescription = product.GetLocalized(x => x.FullDescription, lang.Id, false, false);
                if (!String.IsNullOrEmpty(fullDescription))
                    _localizedEntityService.SaveLocalizedValue(productCopy, x => x.FullDescription, fullDescription, lang.Id);

                var metaKeywords = product.GetLocalized(x => x.MetaKeywords, lang.Id, false, false);
                if (!String.IsNullOrEmpty(metaKeywords))
                    _localizedEntityService.SaveLocalizedValue(productCopy, x => x.MetaKeywords, metaKeywords, lang.Id);

                var metaDescription = product.GetLocalized(x => x.MetaDescription, lang.Id, false, false);
                if (!String.IsNullOrEmpty(metaDescription))
                    _localizedEntityService.SaveLocalizedValue(productCopy, x => x.MetaDescription, metaDescription, lang.Id);

                var metaTitle = product.GetLocalized(x => x.MetaTitle, lang.Id, false, false);
                if (!String.IsNullOrEmpty(metaTitle))
                    _localizedEntityService.SaveLocalizedValue(productCopy, x => x.MetaTitle, metaTitle, lang.Id);

				var bundleTitleText = product.GetLocalized(x => x.BundleTitleText, lang.Id, false, false);
				if (!String.IsNullOrEmpty(bundleTitleText))
					_localizedEntityService.SaveLocalizedValue(productCopy, x => x.BundleTitleText, bundleTitleText, lang.Id);

                //search engine name
                _urlRecordService.SaveSlug(productCopy, productCopy.ValidateSeName("", name, false), lang.Id);

            }

            // product pictures
            var newPictureIds = new Dictionary<int, string>();

            if (copyImages)
            {
                foreach (var productPicture in product.ProductPictures)
                {
                    var picture = productPicture.Picture;
                    var pictureCopy = _pictureService.InsertPicture(
                        _pictureService.LoadPictureBinary(picture),
                        picture.MimeType, 
                        _pictureService.GetPictureSeName(newName), 
                        true,
						false,
						false);

                    _productService.InsertProductPicture(new ProductPicture
                    {
                        ProductId = productCopy.Id,
                        PictureId = pictureCopy.Id,
                        DisplayOrder = productPicture.DisplayOrder
                    });

                    newPictureIds.Add(productPicture.PictureId, pictureCopy.Id.ToString());
                }
            }

            // product <-> categories mappings
            foreach (var productCategory in product.ProductCategories)
            {
                var productCategoryCopy = new ProductCategory
                {
                    ProductId = productCopy.Id,
                    CategoryId = productCategory.CategoryId,
                    IsFeaturedProduct = productCategory.IsFeaturedProduct,
                    DisplayOrder = productCategory.DisplayOrder
                };

                _categoryService.InsertProductCategory(productCategoryCopy);
            }

            // product <-> manufacturers mappings
            foreach (var productManufacturers in product.ProductManufacturers)
            {
                var productManufacturerCopy = new ProductManufacturer
                {
                    ProductId = productCopy.Id,
                    ManufacturerId = productManufacturers.ManufacturerId,
                    IsFeaturedProduct = productManufacturers.IsFeaturedProduct,
                    DisplayOrder = productManufacturers.DisplayOrder
                };

                _manufacturerService.InsertProductManufacturer(productManufacturerCopy);
            }

            // product <-> releated products mappings
            foreach (var relatedProduct in _productService.GetRelatedProductsByProductId1(product.Id, true))
            {
                _productService.InsertRelatedProduct(new RelatedProduct
                {
                    ProductId1 = productCopy.Id,
                    ProductId2 = relatedProduct.ProductId2,
                    DisplayOrder = relatedProduct.DisplayOrder
                });
            }

            // product <-> cross sells mappings
            foreach (var csProduct in _productService.GetCrossSellProductsByProductId1(product.Id, true))
            {
                _productService.InsertCrossSellProduct(new CrossSellProduct
                {
                    ProductId1 = productCopy.Id,
                    ProductId2 = csProduct.ProductId2,
                });
            }

            // product specifications
            foreach (var productSpecificationAttribute in product.ProductSpecificationAttributes)
            {
                var psaCopy = new ProductSpecificationAttribute
                {
                    ProductId = productCopy.Id,
                    SpecificationAttributeOptionId = productSpecificationAttribute.SpecificationAttributeOptionId,
                    AllowFiltering = productSpecificationAttribute.AllowFiltering,
                    ShowOnProductPage = productSpecificationAttribute.ShowOnProductPage,
                    DisplayOrder = productSpecificationAttribute.DisplayOrder
                };

                _specificationAttributeService.InsertProductSpecificationAttribute(psaCopy);
            }

			//store mapping
			var selectedStoreIds = _storeMappingService.GetStoresIdsWithAccess(product);
			foreach (var id in selectedStoreIds)
			{
				_storeMappingService.InsertStoreMapping(productCopy, id);
			}

			// product <-> attributes mappings
			var associatedAttributes = new Dictionary<int, int>();
			var associatedAttributeValues = new Dictionary<int, int>();

			foreach (var productVariantAttribute in _productAttributeService.GetProductVariantAttributesByProductId(product.Id))
			{
				var productVariantAttributeCopy = new ProductVariantAttribute
				{
					ProductId = productCopy.Id,
					ProductAttributeId = productVariantAttribute.ProductAttributeId,
					TextPrompt = productVariantAttribute.TextPrompt,
					IsRequired = productVariantAttribute.IsRequired,
					AttributeControlTypeId = productVariantAttribute.AttributeControlTypeId,
					DisplayOrder = productVariantAttribute.DisplayOrder
				};

				_productAttributeService.InsertProductVariantAttribute(productVariantAttributeCopy);
				//save associated value (used for combinations copying)
				associatedAttributes.Add(productVariantAttribute.Id, productVariantAttributeCopy.Id);

				// product variant attribute values
				var productVariantAttributeValues = _productAttributeService.GetProductVariantAttributeValues(productVariantAttribute.Id);

				foreach (var productVariantAttributeValue in productVariantAttributeValues)
				{

                    var newPictureId = 0;

                    if (copyImages)
                    {
                        var picture = _pictureService.GetPictureById(productVariantAttributeValue.PictureId);
                        if (picture != null)
                        {
                            var pictureCopy = _pictureService.InsertPicture(
                            _pictureService.LoadPictureBinary(picture),
                            picture.MimeType,
                            _pictureService.GetPictureSeName(newName),
                            true, false, false);

                            newPictureId = pictureCopy.Id;
                        }
                    }

                    var pvavCopy = new ProductVariantAttributeValue
					{
						ProductVariantAttributeId = productVariantAttributeCopy.Id,
						Name = productVariantAttributeValue.Name,
						Color = productVariantAttributeValue.Color,
						PriceAdjustment = productVariantAttributeValue.PriceAdjustment,
						WeightAdjustment = productVariantAttributeValue.WeightAdjustment,
						IsPreSelected = productVariantAttributeValue.IsPreSelected,
						DisplayOrder = productVariantAttributeValue.DisplayOrder,
						ValueTypeId = productVariantAttributeValue.ValueTypeId,
						LinkedProductId = productVariantAttributeValue.LinkedProductId,
						Quantity = productVariantAttributeValue.Quantity,
                        PictureId = newPictureId
                    };

					_productAttributeService.InsertProductVariantAttributeValue(pvavCopy);

					//save associated value (used for combinations copying)
					associatedAttributeValues.Add(productVariantAttributeValue.Id, pvavCopy.Id);

					//localization
					foreach (var lang in languages)
					{
						var name = productVariantAttributeValue.GetLocalized(x => x.Name, lang.Id, false, false);
						if (!String.IsNullOrEmpty(name))
							_localizedEntityService.SaveLocalizedValue(pvavCopy, x => x.Name, name, lang.Id);
					}
				}
			}

			// attribute combinations
			using (var scope = new DbContextScope(lazyLoading: false, forceNoTracking: false))
			{
				scope.LoadCollection(product, (Product p) => p.ProductVariantAttributeCombinations);
			}

			foreach (var combination in product.ProductVariantAttributeCombinations)
			{
				//generate new AttributesXml according to new value IDs
				string newAttributesXml = "";
				var parsedProductVariantAttributes = _productAttributeParser.ParseProductVariantAttributes(combination.AttributesXml);
				foreach (var oldPva in parsedProductVariantAttributes)
				{
					if (associatedAttributes.ContainsKey(oldPva.Id))
					{
						int newPvaId = associatedAttributes[oldPva.Id];
						var newPva = _productAttributeService.GetProductVariantAttributeById(newPvaId);
						if (newPva != null)
						{
							var oldPvaValuesStr = _productAttributeParser.ParseValues(combination.AttributesXml, oldPva.Id);
							foreach (var oldPvaValueStr in oldPvaValuesStr)
							{
								if (newPva.ShouldHaveValues())
								{
									//attribute values
									int oldPvaValue = int.Parse(oldPvaValueStr);
									if (associatedAttributeValues.ContainsKey(oldPvaValue))
									{
										int newPvavId = associatedAttributeValues[oldPvaValue];
										var newPvav = _productAttributeService.GetProductVariantAttributeValueById(newPvavId);
										if (newPvav != null)
										{
											newAttributesXml = _productAttributeParser.AddProductAttribute(newAttributesXml, newPva, newPvav.Id.ToString());
										}
									}
								}
								else
								{
									//just a text
									newAttributesXml = _productAttributeParser.AddProductAttribute(newAttributesXml, newPva, oldPvaValueStr);
								}
							}
						}
					}
				}

                var newAssignedPictureIds = new List<string>();

                if(!String.IsNullOrEmpty(combination.AssignedPictureIds))
                {
                    combination.AssignedPictureIds.Split(',').Each(x => {
                        newAssignedPictureIds.Add(newPictureIds[Convert.ToInt32(x)]);
                    });
                }
                
                var combinationCopy = new ProductVariantAttributeCombination
				{
					ProductId = productCopy.Id,
					AttributesXml = newAttributesXml,
					StockQuantity = combination.StockQuantity,
					AllowOutOfStockOrders = combination.AllowOutOfStockOrders,

					// SmartStore extension
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
				_productAttributeService.InsertProductVariantAttributeCombination(combinationCopy);
			}

			// tier prices
			foreach (var tierPrice in product.TierPrices)
			{
				_productService.InsertTierPrice(
					new TierPrice()
					{
						ProductId = productCopy.Id,
						StoreId = tierPrice.StoreId,
						CustomerRoleId = tierPrice.CustomerRoleId,
						Quantity = tierPrice.Quantity,
						Price = tierPrice.Price,
                        CalculationMethod = tierPrice.CalculationMethod
					});
			}

			// product <-> discounts mapping
			foreach (var discount in product.AppliedDiscounts)
			{
				productCopy.AppliedDiscounts.Add(discount);
				_productService.UpdateProduct(productCopy);
			}

			// update "HasTierPrices" and "HasDiscountsApplied" properties
			_productService.UpdateHasTierPricesProperty(productCopy);
			_productService.UpdateLowestAttributeCombinationPriceProperty(productCopy);
			_productService.UpdateHasDiscountsApplied(productCopy);

			// associated products
			if (copyAssociatedProducts && product.ProductType != ProductType.BundledProduct)
			{
				var copyOf = _localizationService.GetResource("Admin.Common.CopyOf");
				var searchQuery = new CatalogSearchQuery()
					.HasParentGroupedProduct(product.Id);

				var query = _catalogSearchService.PrepareQuery(searchQuery);
				var associatedProducts = query.OrderBy(p => p.DisplayOrder).ToList();

				foreach (var associatedProduct in associatedProducts)
				{
					var associatedProductCopy = CopyProduct(associatedProduct, $"{copyOf} {associatedProduct.Name}", isPublished, copyImages, false);
					associatedProductCopy.ParentGroupedProductId = productCopy.Id;

					_productService.UpdateProduct(productCopy);
				}
			}

			// bundled products
			var bundledItems = _productService.GetBundleItems(product.Id, true);

			foreach (var bundleItem in bundledItems)
			{
				var newBundleItem = bundleItem.Item.Clone();
				newBundleItem.BundleProductId = productCopy.Id;

				_productService.InsertBundleItem(newBundleItem);

				foreach (var itemFilter in bundleItem.Item.AttributeFilters)
				{
					var newItemFilter = itemFilter.Clone();
					newItemFilter.BundleItemId = newBundleItem.Id;

					_productAttributeService.InsertProductBundleItemAttributeFilter(newItemFilter);
				}
			}

            return productCopy;
        }

        #endregion
    }
}
