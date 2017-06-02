using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Services.Catalog;
using SmartStore.Services.Catalog.Extensions;
using SmartStore.Services.DataExchange.Export;
using SmartStore.Services.Localization;
using SmartStore.Services.Search;
using SmartStore.Services.Security;
using SmartStore.Services.Seo;
using SmartStore.Web.Framework.UI;
using SmartStore.Web.Infrastructure.Cache;
using SmartStore.Web.Models.Catalog;
using SmartStore.Web.Models.Media;

namespace SmartStore.Web.Controllers
{
	public partial class CatalogHelper
	{
		public void MapListActions(ProductSummaryModel model, IPagingOptions entity, string defaultPageSizeOptions)
		{
			var searchQuery = _catalogSearchQueryFactory.Current;
			
			// View mode
			model.AllowViewModeChanging = _catalogSettings.AllowProductViewModeChanging;

			// Sorting
			model.AllowSorting = _catalogSettings.AllowProductSorting;
			if (model.AllowSorting)
			{
				model.CurrentSortOrder = searchQuery?.CustomData.Get("CurrentSortOrder").Convert<int?>();

				model.AvailableSortOptions = _services.Cache.Get("pres:productlistsortoptions-{0}".FormatInvariant(_services.WorkContext.WorkingLanguage.Id), () => 
				{
					var dict = new Dictionary<int, string>();
					foreach (ProductSortingEnum enumValue in Enum.GetValues(typeof(ProductSortingEnum)))
					{
						if (enumValue == ProductSortingEnum.CreatedOnAsc || enumValue == ProductSortingEnum.Initial)
							continue;

						dict[(int)enumValue] = enumValue.GetLocalizedEnum(_localizationService, _services.WorkContext);
					}

					return dict;
				});

				if (!searchQuery.Origin.IsCaseInsensitiveEqual("Search/Search"))
				{
					model.RelevanceSortOrderName = T("Products.Sorting.Featured");
					if ((int)ProductSortingEnum.Relevance == (model.CurrentSortOrder ?? 1))
					{
						model.CurrentSortOrderName = model.RelevanceSortOrderName;
					}
				}

				if (model.CurrentSortOrderName.IsEmpty())
				{
					model.CurrentSortOrderName = model.AvailableSortOptions.Get(model.CurrentSortOrder ?? 1) ?? model.AvailableSortOptions.First().Value;
				}
			}
			
			// Pagination
			if (entity?.AllowCustomersToSelectPageSize ?? _catalogSettings.AllowCustomersToSelectPageSize)
			{
				try
				{
					model.AvailablePageSizes = (entity?.PageSizeOptions.NullEmpty() ?? defaultPageSizeOptions).Convert<List<int>>();
				}
				catch
				{
					model.AvailablePageSizes = new int[] { 12, 24, 36, 48, 72, 120 };
				}
			}

			model.AllowFiltering = true;
		}

		public ProductSummaryMappingSettings GetBestFitProductSummaryMappingSettings(ProductSummaryViewMode viewMode)
		{
			return GetBestFitProductSummaryMappingSettings(viewMode, null);
		}

		public ProductSummaryMappingSettings GetBestFitProductSummaryMappingSettings(ProductSummaryViewMode viewMode, Action<ProductSummaryMappingSettings> fn)
		{
			var settings = new ProductSummaryMappingSettings
			{
				ViewMode = viewMode,
				MapPrices = true,
				MapPictures = true,
				ThumbnailSize = _mediaSettings.ProductThumbPictureSize
			};

			if (viewMode == ProductSummaryViewMode.Grid)
			{
				settings.MapShortDescription = _catalogSettings.ShowShortDescriptionInGridStyleLists;
				settings.MapManufacturers = _catalogSettings.ShowManufacturerInGridStyleLists;
				settings.MapColorAttributes = _catalogSettings.ShowColorSquaresInLists;
				settings.MapAttributes = _catalogSettings.ShowProductOptionsInLists;
				settings.MapReviews = _catalogSettings.ShowProductReviewsInProductLists;
				settings.MapDeliveryTimes = _catalogSettings.ShowDeliveryTimesInProductLists;
			}
			else if (viewMode == ProductSummaryViewMode.List)
			{
				settings.MapShortDescription = true;
				settings.MapLegalInfo = _taxSettings.ShowLegalHintsInProductList;
				settings.MapManufacturers = true;
				settings.MapColorAttributes = _catalogSettings.ShowColorSquaresInLists;
				settings.MapAttributes = _catalogSettings.ShowProductOptionsInLists;
				//settings.MapSpecificationAttributes = true; // TODO: (mc) What about SpecAttrs in List-Mode (?) Option?
				settings.MapReviews = _catalogSettings.ShowProductReviewsInProductLists;
				settings.MapDeliveryTimes = _catalogSettings.ShowDeliveryTimesInProductLists;
				settings.MapDimensions = _catalogSettings.ShowDimensions;
			}
			else if (viewMode == ProductSummaryViewMode.Compare)
			{
				settings.MapShortDescription = _catalogSettings.IncludeShortDescriptionInCompareProducts;
				settings.MapFullDescription = _catalogSettings.IncludeFullDescriptionInCompareProducts;
				settings.MapLegalInfo = _taxSettings.ShowLegalHintsInProductList;
				settings.MapManufacturers = true;
				settings.MapAttributes = true;
				settings.MapSpecificationAttributes = true;
				settings.MapReviews = _catalogSettings.ShowProductReviewsInProductLists;
				settings.MapDeliveryTimes = _catalogSettings.ShowDeliveryTimesInProductLists;
				settings.MapDimensions = _catalogSettings.ShowDimensions;
			}

			fn?.Invoke(settings);

			return settings;
		}

		public virtual ProductSummaryModel MapProductSummaryModel(IList<Product> products, ProductSummaryMappingSettings settings)
		{
			Guard.NotNull(products, nameof(products));

			return MapProductSummaryModel(new PagedList<Product>(products, 0, int.MaxValue), settings);
		}

		public virtual ProductSummaryModel MapProductSummaryModel(IPagedList<Product> products, ProductSummaryMappingSettings settings)
		{
			Guard.NotNull(products, nameof(products));

			if (settings == null)
			{
				settings = new ProductSummaryMappingSettings();
			}

			using (_services.Chronometer.Step("MapProductSummaryModel"))
			{
				// PERF!!
				var store = _services.StoreContext.CurrentStore;
				var customer = _services.WorkContext.CurrentCustomer;
				var currency = _services.WorkContext.WorkingCurrency;
				var allowPrices = _services.Permissions.Authorize(StandardPermissionProvider.DisplayPrices);
				var sllowShoppingCart = _services.Permissions.Authorize(StandardPermissionProvider.EnableShoppingCart);
				var allowWishlist = _services.Permissions.Authorize(StandardPermissionProvider.EnableWishlist);
				var taxDisplayType = _services.WorkContext.GetTaxDisplayTypeFor(customer, store.Id);
				var cachedManufacturerModels = new Dictionary<int, ManufacturerOverviewModel>();

				string taxInfo = T(taxDisplayType == TaxDisplayType.IncludingTax ? "Tax.InclVAT" : "Tax.ExclVAT");
				var legalInfo = "";

				var res = new Dictionary<string, LocalizedString>(StringComparer.OrdinalIgnoreCase)
				{
					{ "Products.CallForPrice", T("Products.CallForPrice") },
					{ "Products.PriceRangeFrom", T("Products.PriceRangeFrom") },
					{ "Media.Product.ImageLinkTitleFormat", T("Media.Product.ImageLinkTitleFormat") },
					{ "Media.Product.ImageAlternateTextFormat", T("Media.Product.ImageAlternateTextFormat") },
					{ "Products.DimensionsValue", T("Products.DimensionsValue") },
					{ "Common.AdditionalShippingSurcharge", T("Common.AdditionalShippingSurcharge") }
				};
				
				if (settings.MapLegalInfo)
				{
					if (_topicService.Value.GetTopicBySystemName("ShippingInfo", store.Id) == null)
					{
						legalInfo = T("Tax.LegalInfoShort2").Text.FormatInvariant(taxInfo);
					}
					else
					{
						var shippingInfoLink = _urlHelper.RouteUrl("Topic", new { SystemName = "shippinginfo" });
						legalInfo = T("Tax.LegalInfoShort").Text.FormatInvariant(taxInfo, shippingInfoLink);
					}
				}

				using (var scope = new DbContextScope(ctx: _services.DbContext, autoCommit: false, validateOnSave: false))
				{
					// Run in uncommitting scope, because pictures could be updated (IsNew property) 
					var batchContext = _dataExporter.Value.CreateProductExportContext(products);

					if (settings.MapPrices)
					{
						batchContext.AppliedDiscounts.LoadAll();
						batchContext.TierPrices.LoadAll();
					}
					
					if (settings.MapAttributes || settings.MapColorAttributes)
					{
						batchContext.Attributes.LoadAll();
					}

					if (settings.MapManufacturers)
					{
						batchContext.ProductManufacturers.LoadAll();
					}

					if (settings.MapSpecificationAttributes)
					{
						batchContext.SpecificationAttributes.LoadAll();
					}

					var model = new ProductSummaryModel(products)
					{
						ViewMode = settings.ViewMode,
						GridColumnSpan = _catalogSettings.GridStyleListColumnSpan,
						ShowSku = _catalogSettings.ShowProductSku,
						ShowWeight = _catalogSettings.ShowWeight,
						ShowDimensions = settings.MapDimensions,
						ShowLegalInfo = settings.MapLegalInfo,
						ShowDescription = settings.MapShortDescription,
						ShowFullDescription = settings.MapFullDescription,
						ShowRatings = settings.MapReviews,
						ShowDeliveryTimes = settings.MapDeliveryTimes,
						ShowPrice = settings.MapPrices,
						ShowBasePrice = settings.MapPrices && _catalogSettings.ShowBasePriceInProductLists && settings.ViewMode != ProductSummaryViewMode.Mini,
						ShowShippingSurcharge = settings.MapPrices && settings.ViewMode != ProductSummaryViewMode.Mini,
						ShowButtons = settings.ViewMode != ProductSummaryViewMode.Mini,
						ShowBrand = settings.MapManufacturers,
						ForceRedirectionAfterAddingToCart = settings.ForceRedirectionAfterAddingToCart,
						CompareEnabled = _catalogSettings.CompareProductsEnabled,
						WishlistEnabled = _permissionService.Value.Authorize(StandardPermissionProvider.EnableWishlist),
						BuyEnabled = !_catalogSettings.HideBuyButtonInLists,
						ThumbSize = settings.ThumbnailSize,
						ShowDiscountBadge = _catalogSettings.ShowDiscountSign,
						ShowNewBadge = _catalogSettings.LabelAsNewForMaxDays.HasValue
					};

					var mapItemContext = new MapProductSummaryItemContext
					{
						BatchContext = batchContext,
						CachedManufacturerModels = cachedManufacturerModels,
						Currency = currency,
						LegalInfo = legalInfo,
						Model = model,
						Resources = res,
						Settings = settings,
						Customer = customer,
						Store = store,
						AllowPrices = allowPrices,
						AllowShoppingCart = sllowShoppingCart,
						AllowWishlist = allowWishlist,
						TaxDisplayType = taxDisplayType
					};

					foreach (var product in products)
					{
						MapProductSummaryItem(product, mapItemContext);
					}

					_services.DisplayControl.AnnounceRange(products);

					scope.Commit();

					batchContext.Clear();

					// don't show stuff without data at all
					model.ShowDescription = model.ShowDescription && model.Items.Any(x => x.ShortDescription.HasValue());
					model.ShowBrand = model.ShowBrand && model.Items.Any(x => x.Manufacturer != null);

					return model;
				}
			}		
		}

		private void MapProductSummaryItem(Product product, MapProductSummaryItemContext ctx)
		{
			var contextProduct = product;
			var finalPrice = decimal.Zero;
			var model = ctx.Model;
			var settings = ctx.Settings;

			var item = new ProductSummaryModel.SummaryItem(ctx.Model)
			{
				Id = product.Id,
				Name = product.GetLocalized(x => x.Name).EmptyNull(),
				SeName = product.GetSeName()
			};

			if (model.ShowDescription)
			{
				item.ShortDescription = product.GetLocalized(x => x.ShortDescription);
			}

			if (settings.MapFullDescription)
			{
				item.FullDescription = product.GetLocalized(x => x.FullDescription);
			}

			// Price
			if (settings.MapPrices)
			{
				finalPrice = MapSummaryItemPrice(product, ref contextProduct, item, ctx);
			}

			// (Color) Attributes
			if (settings.MapColorAttributes || settings.MapAttributes)
			{
				#region Map (color) attributes

				var attributes = ctx.BatchContext.Attributes.GetOrLoad(contextProduct.Id);

				var cachedAttributeNames = new Dictionary<int, string>();

				// Color squares
				if (attributes.Any() && settings.MapColorAttributes)
				{
					var colorAttributes = attributes
						.Where(x => x.IsListTypeAttribute())
						.SelectMany(x => x.ProductVariantAttributeValues)
						.Where(x => x.Color.HasValue() && !x.Color.IsCaseInsensitiveEqual("transparent"))
						.Distinct()
						.Take(20) // limit results
						.Select(x => 
						{
							var attr = x.ProductVariantAttribute.ProductAttribute;
							var attrName = cachedAttributeNames.Get(attr.Id) ?? (cachedAttributeNames[attr.Id] = attr.GetLocalized(l => l.Name));

							return new ProductSummaryModel.ColorAttributeValue
							{
								Id = x.Id,
								Color = x.Color,
								Alias = x.Alias,
								FriendlyName = x.GetLocalized(l => l.Name),
								AttributeId = x.ProductVariantAttributeId,
								AttributeName = attrName,
								ProductAttributeId = attr.Id,
								ProductUrl = _productUrlHelper.GetProductUrl(product.Id, item.SeName, 0, x)
							};
						})
						.ToList();

					item.ColorAttributes = colorAttributes;

					// TODO: (mc) Resolve attribute value images also
				}

				// Variant Attributes
				if (attributes.Any() && settings.MapAttributes)
				{
					if (item.ColorAttributes != null && item.ColorAttributes.Any())
					{
						var processedIds = item.ColorAttributes.Select(x => x.AttributeId).Distinct().ToArray();
						attributes = attributes.Where(x => !processedIds.Contains(x.Id)).ToList();
					}

					foreach (var attr in attributes)
					{
						var pa = attr.ProductAttribute;
						item.Attributes.Add(new ProductSummaryModel.Attribute
						{
							Id = attr.Id,
							Alias = pa.Alias,
							Name = cachedAttributeNames.Get(pa.Id) ?? (cachedAttributeNames[pa.Id] = pa.GetLocalized(l => l.Name))
						});
					}
				}

				#endregion
			}

			// Picture
			if (settings.MapPictures)
			{
				#region Map product picture

				// If a size has been set in the view, we use it in priority
				int pictureSize = model.ThumbSize.HasValue ? model.ThumbSize.Value : _mediaSettings.ProductThumbPictureSize;

				// Prepare picture model
				var defaultProductPictureCacheKey = string.Format(
					ModelCacheEventConsumer.PRODUCT_DEFAULTPICTURE_MODEL_KEY,
					product.Id,
					pictureSize,
					true,
					_services.WorkContext.WorkingLanguage.Id,
					ctx.Store.Id);

				item.Picture = _services.Cache.Get(defaultProductPictureCacheKey, () =>
				{
					if (!ctx.BatchContext.Pictures.FullyLoaded)
					{
						ctx.BatchContext.Pictures.LoadAll();
					}

					var picture = ctx.BatchContext.Pictures.GetOrLoad(product.Id).FirstOrDefault();
					var pictureModel = new PictureModel
					{
						Size = pictureSize,
						ImageUrl = _pictureService.GetPictureUrl(picture, pictureSize, !_catalogSettings.HideProductDefaultPictures),
						FullSizeImageUrl = _pictureService.GetPictureUrl(picture, 0, !_catalogSettings.HideProductDefaultPictures),
						Title = string.Format(ctx.Resources["Media.Product.ImageLinkTitleFormat"], item.Name),
						AlternateText = string.Format(ctx.Resources["Media.Product.ImageAlternateTextFormat"], item.Name),
						PictureId = picture == null ? 0 : picture.Id
					};

					return pictureModel;
				}, TimeSpan.FromHours(6));

				#endregion
			}

			// Manufacturers
			if (settings.MapManufacturers)
			{
				item.Manufacturer = PrepareManufacturersOverviewModel(
					ctx.BatchContext.ProductManufacturers.GetOrLoad(product.Id), 
					ctx.CachedManufacturerModels,
					_catalogSettings.ShowManufacturerLogoInLists && settings.ViewMode == ProductSummaryViewMode.List).FirstOrDefault();
			}

			// Spec Attributes
			if (settings.MapSpecificationAttributes)
			{
				item.SpecificationAttributes.AddRange(MapProductSpecificationModels(ctx.BatchContext.SpecificationAttributes.GetOrLoad(product.Id)));
			}

			item.MinPriceProductId = contextProduct.Id;
			item.Sku = contextProduct.Sku;

			// Measure Dimensions
			if (model.ShowDimensions && (contextProduct.Width != 0 || contextProduct.Height != 0 || contextProduct.Length != 0))
			{
				item.Dimensions = ctx.Resources["Products.DimensionsValue"].Text.FormatCurrent(
					contextProduct.Width.ToString("N2"),
					contextProduct.Height.ToString("N2"),
					contextProduct.Length.ToString("N2")
				);
				item.DimensionMeasureUnit = _measureService.GetMeasureDimensionById(_measureSettings.BaseDimensionId).SystemKeyword;
			}

			// Delivery Times
			item.HideDeliveryTime = (product.ProductType == ProductType.GroupedProduct);
			if (model.ShowDeliveryTimes && !item.HideDeliveryTime)
			{
				item.StockAvailablity = contextProduct.FormatStockMessage(_localizationService);
				item.DisplayDeliveryTimeAccordingToStock = contextProduct.DisplayDeliveryTimeAccordingToStock(_catalogSettings);

				var deliveryTime = _deliveryTimeService.GetDeliveryTime(contextProduct);
				if (deliveryTime != null)
				{
					item.DeliveryTimeName = deliveryTime.GetLocalized(x => x.Name);
					item.DeliveryTimeHexValue = deliveryTime.ColorHexValue;
				}
			}
			
			item.LegalInfo = ctx.LegalInfo;
			item.RatingSum = product.ApprovedRatingSum;
			item.TotalReviews = product.ApprovedTotalReviews;
			item.IsShippingEnabled = contextProduct.IsShipEnabled;

			if (finalPrice != decimal.Zero && model.ShowBasePrice)
			{
				item.BasePriceInfo = contextProduct.GetBasePriceInfo(finalPrice, _localizationService, _priceFormatter, ctx.Currency);
			}

			if (settings.MapPrices)
			{
				var addShippingPrice = _currencyService.ConvertCurrency(contextProduct.AdditionalShippingCharge, ctx.Store.PrimaryStoreCurrency, ctx.Currency);

				if (addShippingPrice > 0)
				{
					item.TransportSurcharge = ctx.Resources["Common.AdditionalShippingSurcharge"].Text.FormatCurrent(_priceFormatter.FormatPrice(addShippingPrice, true, false));
				}
			}

			if (model.ShowWeight && contextProduct.Weight > 0)
			{
				item.Weight = "{0} {1}".FormatCurrent(contextProduct.Weight.ToString("N2"), _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId).Name);
			}

			// New Badge
			if (product.IsNew(_catalogSettings))
			{
				item.Badges.Add(new ProductSummaryModel.Badge
				{
					Label = T("Common.New"),
					Style = BadgeStyle.Success
				});
			}

			model.Items.Add(item);
		}

		private decimal MapSummaryItemPrice(Product product, ref Product contextProduct, ProductSummaryModel.SummaryItem item, MapProductSummaryItemContext ctx)
		{
			// Returns the final price
			var finalPrice = decimal.Zero;
			var model = ctx.Model;

			var priceModel = new ProductSummaryModel.PriceModel();

			if (product.ProductType == ProductType.BundledProduct && product.BundlePerItemPricing && !ctx.BatchContext.ProductBundleItems.FullyLoaded)
			{
				ctx.BatchContext.ProductBundleItems.LoadAll();
			}

			if (product.ProductType == ProductType.GroupedProduct)
			{
				#region Grouped product
				
				if (ctx.GroupedProducts == null)
				{
					// One-time batched retrieval of all associated products
					var searchQuery = new CatalogSearchQuery()
						.PublishedOnly(true)
						.HasStoreId(ctx.Store.Id)
						.HasParentGroupedProduct(ctx.BatchContext.ProductIds.ToArray());

					// Get all associated products for this batch grouped by ParentGroupedProductId
					var allAssociatedProducts = _catalogSearchService.Search(searchQuery).Hits
						.OrderBy(x => x.ParentGroupedProductId)
						.ThenBy(x => x.DisplayOrder);

					ctx.GroupedProducts = allAssociatedProducts.ToMultimap(x => x.ParentGroupedProductId, x => x);

					if (ctx.GroupedProducts.Any())
					{
						ctx.BatchContext.AppliedDiscounts.Collect(allAssociatedProducts.Select(x => x.Id));
					}
				}

				var associatedProducts = ctx.GroupedProducts[product.Id];

				priceModel.DisableBuyButton = true;
				priceModel.DisableWishlistButton = true;
				priceModel.AvailableForPreOrder = false;

				if (associatedProducts.Count > 0)
				{
					contextProduct = associatedProducts.OrderBy(x => x.DisplayOrder).First();

					_services.DisplayControl.Announce(contextProduct);

					if (ctx.AllowPrices && _catalogSettings.PriceDisplayType != PriceDisplayType.Hide)
					{
						decimal? displayPrice = null;
						bool displayFromMessage = false;

						if (_catalogSettings.PriceDisplayType == PriceDisplayType.PreSelectedPrice)
						{
							displayPrice = _priceCalculationService.GetPreselectedPrice(contextProduct, ctx.Customer, ctx.BatchContext);
						}
						else if (_catalogSettings.PriceDisplayType == PriceDisplayType.PriceWithoutDiscountsAndAttributes)
						{
							displayPrice = _priceCalculationService.GetFinalPrice(contextProduct, null, ctx.Customer, decimal.Zero, false, 1, null, ctx.BatchContext);
						}
						else
						{
							displayFromMessage = true;
							displayPrice = _priceCalculationService.GetLowestPrice(product, ctx.Customer, ctx.BatchContext, associatedProducts, out contextProduct);
						}

						if (contextProduct != null && !contextProduct.CustomerEntersPrice)
						{
							if (contextProduct.CallForPrice)
							{
								priceModel.RegularPriceValue = null;
								priceModel.PriceValue = 0;
								priceModel.RegularPrice = null;
								priceModel.Price = ctx.Resources["Products.CallForPrice"];
							}
							else if (displayPrice.HasValue)
							{
								// Calculate prices
								decimal taxRate = decimal.Zero;
								decimal oldPriceBase = _taxService.GetProductPrice(contextProduct, contextProduct.OldPrice, out taxRate);
								decimal finalPriceBase = _taxService.GetProductPrice(contextProduct, displayPrice.Value, out taxRate);
								finalPrice = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceBase, ctx.Currency);

								priceModel.RegularPriceValue = null;
								priceModel.PriceValue = finalPrice;
								priceModel.RegularPrice = null;

								if (displayFromMessage)
								{
									priceModel.Price = String.Format(ctx.Resources["Products.PriceRangeFrom"], _priceFormatter.FormatPrice(finalPrice));
								}
								else
								{
									priceModel.Price = _priceFormatter.FormatPrice(finalPrice);
								}

								if (oldPriceBase > 0)
								{
									priceModel.RegularPriceValue = _currencyService.ConvertFromPrimaryStoreCurrency(oldPriceBase, ctx.Currency);
								}

								priceModel.HasDiscount = (finalPriceBase != oldPriceBase && oldPriceBase != decimal.Zero);
							}
							else
							{
								// Actually it's not possible (we presume that displayPrice always has a value). We never should get here
								Debug.WriteLine(string.Format("Cannot calculate displayPrice for product #{0}", product.Id));
							}
						}
					}
				}

				#endregion
			}
			else
			{
				#region Simple product

				//add to cart button
				priceModel.DisableBuyButton = product.DisableBuyButton || !ctx.AllowShoppingCart || !ctx.AllowPrices;

				//add to wishlist button
				priceModel.DisableWishlistButton = product.DisableWishlistButton || !ctx.AllowWishlist || !ctx.AllowPrices;

				//pre-order
				priceModel.AvailableForPreOrder = product.AvailableForPreOrder;

				//prices
				if (ctx.AllowPrices && _catalogSettings.PriceDisplayType != PriceDisplayType.Hide && !product.CustomerEntersPrice)
				{
					if (product.CallForPrice)
					{
						// call for price
						priceModel.RegularPriceValue = null;
						priceModel.PriceValue = 0;
						priceModel.RegularPrice = null;
						priceModel.Price = ctx.Resources["Products.CallForPrice"];
					}
					else
					{
						//calculate prices
						bool displayFromMessage = false;
						decimal displayPrice = decimal.Zero;

						if (_catalogSettings.PriceDisplayType == PriceDisplayType.PreSelectedPrice)
						{
							displayPrice = _priceCalculationService.GetPreselectedPrice(product, ctx.Customer, ctx.BatchContext);
						}
						else if (_catalogSettings.PriceDisplayType == PriceDisplayType.PriceWithoutDiscountsAndAttributes)
						{
							displayPrice = _priceCalculationService.GetFinalPrice(product, null, ctx.Customer, decimal.Zero, false, 1, null, ctx.BatchContext);
						}
						else
						{
							displayPrice = _priceCalculationService.GetLowestPrice(product, ctx.Customer, ctx.BatchContext, out displayFromMessage);
						}

						decimal taxRate = decimal.Zero;
						decimal oldPriceBase = _taxService.GetProductPrice(product, product.OldPrice, out taxRate);
						decimal finalPriceBase = _taxService.GetProductPrice(product, displayPrice, out taxRate);

						decimal oldPrice = _currencyService.ConvertFromPrimaryStoreCurrency(oldPriceBase, ctx.Currency);
						finalPrice = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceBase, ctx.Currency);

						priceModel.HasDiscount = (finalPriceBase != oldPriceBase && oldPriceBase != decimal.Zero);

						if (displayFromMessage)
						{
							priceModel.RegularPriceValue = null;
							priceModel.RegularPrice = null;
							priceModel.Price = String.Format(ctx.Resources["Products.PriceRangeFrom"], _priceFormatter.FormatPrice(finalPrice));
						}
						else
						{
							priceModel.PriceValue = finalPrice;
							if (priceModel.HasDiscount)
							{
								priceModel.RegularPriceValue = oldPrice;
								priceModel.RegularPrice = _priceFormatter.FormatPrice(oldPrice);
								priceModel.Price = _priceFormatter.FormatPrice(finalPrice);
							}
							else
							{
								priceModel.RegularPriceValue = null;
								priceModel.RegularPrice = null;
								priceModel.Price = _priceFormatter.FormatPrice(finalPrice);
							}
						}
					}
				}

				#endregion
			}

			var regularPriceValue = priceModel.RegularPriceValue.GetValueOrDefault();
			if (priceModel.HasDiscount && regularPriceValue > 0 && regularPriceValue > priceModel.PriceValue)
			{
				priceModel.SavingPercent = (float)((priceModel.RegularPriceValue - priceModel.PriceValue) / priceModel.RegularPriceValue) * 100;
				priceModel.SavingAmount = _priceFormatter.FormatPrice(regularPriceValue - priceModel.PriceValue, true, false);

				if (model.ShowDiscountBadge)
				{
					item.Badges.Add(new ProductSummaryModel.Badge
					{
						Label = T("Products.SavingBadgeLabel", priceModel.SavingPercent.ToString("N0")),
						Style = BadgeStyle.Danger
					});
				}
			}

			priceModel.CallForPrice = product.CallForPrice;

			item.Price = priceModel;

			return finalPrice;
		}

		private IEnumerable<ProductSpecificationModel> MapProductSpecificationModels(IEnumerable<ProductSpecificationAttribute> attributes)
		{
			Guard.NotNull(attributes, nameof(attributes));

			if (!attributes.Any())
				return Enumerable.Empty<ProductSpecificationModel>();

			var productId = attributes.First().ProductId;

			string cacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_SPECS_MODEL_KEY, productId, _services.WorkContext.WorkingLanguage.Id);
			return _services.Cache.Get(cacheKey, () =>
			{
				var model = attributes.Select(psa =>
				{
					return new ProductSpecificationModel
					{
						SpecificationAttributeId = psa.SpecificationAttributeOption.SpecificationAttributeId,
						SpecificationAttributeName = psa.SpecificationAttributeOption.SpecificationAttribute.GetLocalized(x => x.Name),
						SpecificationAttributeOption = psa.SpecificationAttributeOption.GetLocalized(x => x.Name)
					};
				}).ToList();

				return model;
			});
		}

		private class MapProductSummaryItemContext
		{
			public ProductSummaryModel Model { get; set; }
			public ProductSummaryMappingSettings Settings { get; set; }
			public ProductExportContext BatchContext { get; set; }
			public Multimap<int, Product> GroupedProducts { get; set; }
			public Dictionary<int, ManufacturerOverviewModel> CachedManufacturerModels { get; set; }
			public Dictionary<string, LocalizedString> Resources { get; set; }
			public string LegalInfo { get; set; }
			public Customer Customer { get; set; }
			public Store Store { get; set; }
			public Currency Currency { get; set; }

			public bool AllowPrices { get; set; }
			public bool AllowShoppingCart { get; set; }
			public bool AllowWishlist { get; set; }
			public TaxDisplayType TaxDisplayType { get; set; }
		}
	}

	public class ProductSummaryMappingSettings
	{
		public ProductSummaryMappingSettings()
		{
			MapPrices = true;
			MapPictures = true;
			ViewMode = ProductSummaryViewMode.Grid;
		}

		public ProductSummaryViewMode ViewMode { get; set; }

		public bool MapPrices { get; set; }
		public bool MapPictures{ get; set; }
		public bool MapDimensions { get; set; }
		public bool MapSpecificationAttributes { get; set; }
		public bool MapColorAttributes { get; set; }
		public bool MapAttributes { get; set; }
		public bool MapManufacturers { get; set; }
		public bool MapShortDescription { get; set; }
		public bool MapFullDescription { get; set; }
		public bool MapLegalInfo { get; set; }
		public bool MapReviews { get; set; }
		public bool MapDeliveryTimes { get; set; }

		public bool ForceRedirectionAfterAddingToCart { get; set; }
		public int? ThumbnailSize { get; set; }	
	}
}