using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SmartStore.Collections;
using SmartStore.Core.Caching;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Localization; 
using SmartStore.Core.Logging;
using SmartStore.Services;
using SmartStore.Services.Catalog;
using SmartStore.Services.Configuration;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Media; 
using SmartStore.Services.Security;
using SmartStore.Services.Seo;
using SmartStore.Services.Tax;
using SmartStore.Web.Framework.UI;
using SmartStore.Web.Framework.UI.Captcha;
using SmartStore.Web.Infrastructure.Cache;
using SmartStore.Web.Models.Catalog;
using SmartStore.Web.Models.Media;

namespace SmartStore.Web.Controllers
{
	public class CatalogHelper
	{
		private static object s_lock = new object();
		
		private readonly ICommonServices _services;
		private readonly ICategoryService _categoryService;
		private readonly IManufacturerService _manufacturerService;
		private readonly IProductService _productService;
		private readonly IProductTemplateService _productTemplateService;
		private readonly IProductAttributeService _productAttributeService;
		private readonly IProductAttributeParser _productAttributeParser;
		private readonly IProductAttributeFormatter _productAttributeFormatter;
		private readonly ITaxService _taxService;
		private readonly ICurrencyService _currencyService;
		private readonly IPictureService _pictureService;
		private readonly ILocalizationService _localizationService;
		private readonly IPriceCalculationService _priceCalculationService;
		private readonly IPriceFormatter _priceFormatter;
		private readonly ISpecificationAttributeService _specificationAttributeService;
		private readonly IDateTimeHelper _dateTimeHelper;
		private readonly IBackInStockSubscriptionService _backInStockSubscriptionService;
		private readonly IDownloadService _downloadService;
		private readonly MediaSettings _mediaSettings;
		private readonly CatalogSettings _catalogSettings;
		private readonly CustomerSettings _customerSettings;
		private readonly CaptchaSettings _captchaSettings;
		private readonly TaxSettings _taxSettings;
		private readonly IMeasureService _measureService;
        private readonly IQuantityUnitService _quantityUnitService;
		private readonly MeasureSettings _measureSettings;
		private readonly IDeliveryTimeService _deliveryTimeService;
		private readonly ISettingService _settingService;
		private readonly Lazy<IMenuPublisher> _menuPublisher;

		private readonly HttpRequestBase _httpRequest;
		private readonly UrlHelper _urlHelper;

		public CatalogHelper(
			ICommonServices services,
			ICategoryService categoryService,
			IManufacturerService manufacturerService,
			IProductService productService,
			IProductTemplateService productTemplateService,
			IProductAttributeService productAttributeService,
			IProductAttributeParser productAttributeParser,
			IProductAttributeFormatter productAttributeFormatter,
			ITaxService taxService,
			ICurrencyService currencyService,
			IPictureService pictureService,
			IPriceCalculationService priceCalculationService,
			IPriceFormatter priceFormatter,
			ISpecificationAttributeService specificationAttributeService,
			IDateTimeHelper dateTimeHelper,
			IBackInStockSubscriptionService backInStockSubscriptionService,
			IDownloadService downloadService,
			MediaSettings mediaSettings,
			CatalogSettings catalogSettings,
			CustomerSettings customerSettings,
			CaptchaSettings captchaSettings,
			IMeasureService measureService,
            IQuantityUnitService quantityUnitService,
			MeasureSettings measureSettings,
			TaxSettings taxSettings,
			IDeliveryTimeService deliveryTimeService,
			ISettingService settingService,
			Lazy<IMenuPublisher> _menuPublisher,
			HttpRequestBase httpRequest,
			UrlHelper urlHelper)
		{
			this._services = services;
			this._categoryService = categoryService;
			this._manufacturerService = manufacturerService;
			this._productService = productService;
			this._productTemplateService = productTemplateService;
			this._productAttributeService = productAttributeService;
			this._productAttributeParser = productAttributeParser;
			this._productAttributeFormatter = productAttributeFormatter;
			this._taxService = taxService;
			this._currencyService = currencyService;
			this._pictureService = pictureService;
			this._localizationService = _services.Localization;
			this._priceCalculationService = priceCalculationService;
			this._priceFormatter = priceFormatter;
			this._specificationAttributeService = specificationAttributeService;
			this._dateTimeHelper = dateTimeHelper;
			this._backInStockSubscriptionService = backInStockSubscriptionService;
			this._downloadService = downloadService;
			this._measureService = measureService;
            this._quantityUnitService = quantityUnitService;
			this._measureSettings = measureSettings;
			this._taxSettings = taxSettings;
			this._deliveryTimeService = deliveryTimeService;
			this._settingService = settingService;
			this._mediaSettings = mediaSettings;
			this._catalogSettings = catalogSettings;
			this._customerSettings = customerSettings;
			this._captchaSettings = captchaSettings;
			this._menuPublisher = _menuPublisher;
			this._httpRequest = httpRequest;
			this._urlHelper = urlHelper;

			T = NullLocalizer.Instance;
		}

		public Localizer T { get; set; }

		public ILogger Logger { get; set; }

		public ProductDetailsModel PrepareProductDetailsPageModel(Product product, bool isAssociatedProduct = false,
			ProductBundleItemData productBundleItem = null, IList<ProductBundleItemData> productBundleItems = null, FormCollection selectedAttributes = null)
		{
			if (product == null)
				throw new ArgumentNullException("product");

			var model = new ProductDetailsModel
			{
				Id = product.Id,
				Name = product.GetLocalized(x => x.Name),
				ShortDescription = product.GetLocalized(x => x.ShortDescription),
				FullDescription = product.GetLocalized(x => x.FullDescription),
				MetaKeywords = product.GetLocalized(x => x.MetaKeywords),
				MetaDescription = product.GetLocalized(x => x.MetaDescription),
				MetaTitle = product.GetLocalized(x => x.MetaTitle),
				SeName = product.GetSeName(),
				ProductType = product.ProductType,
				VisibleIndividually = product.VisibleIndividually,
				//Manufacturers = _manufacturerService.GetProductManufacturersByProductId(product.Id),
				Manufacturers = PrepareManufacturersOverviewModel(_manufacturerService.GetProductManufacturersByProductId(product.Id)),
				ReviewCount = product.ApprovedTotalReviews,
				DisplayAdminLink = _services.Permissions.Authorize(StandardPermissionProvider.AccessAdminPanel),
				//EnableHtmlTextCollapser = Convert.ToBoolean(_settingService.GetSettingByKey<string>("CatalogSettings.EnableHtmlTextCollapser")),
				//HtmlTextCollapsedHeight = Convert.ToString(_settingService.GetSettingByKey<string>("CatalogSettings.HtmlTextCollapsedHeight")),
				ShowSku = _catalogSettings.ShowProductSku,
				Sku = product.Sku,
				ShowManufacturerPartNumber = _catalogSettings.ShowManufacturerPartNumber,
                DisplayProductReviews = _catalogSettings.ShowProductReviewsInProductDetail,
				ManufacturerPartNumber = product.ManufacturerPartNumber,
				ShowGtin = _catalogSettings.ShowGtin,
				Gtin = product.Gtin,
				StockAvailability = product.FormatStockMessage(_localizationService),
				HasSampleDownload = product.IsDownload && product.HasSampleDownload,
				IsCurrentCustomerRegistered = _services.WorkContext.CurrentCustomer.IsRegistered()
			};

			// Back in stock subscriptions
			if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
				 product.BackorderMode == BackorderMode.NoBackorders &&
				 product.AllowBackInStockSubscriptions &&
				 product.StockQuantity <= 0)
			{
				//out of stock
				model.DisplayBackInStockSubscription = true;
				model.BackInStockAlreadySubscribed = _backInStockSubscriptionService
					.FindSubscription(_services.WorkContext.CurrentCustomer.Id, product.Id, _services.StoreContext.CurrentStore.Id) != null;
			}

			//template
			var templateCacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_TEMPLATE_MODEL_KEY, product.ProductTemplateId);
			model.ProductTemplateViewPath = _services.Cache.Get(templateCacheKey, () =>
			{
				var template = _productTemplateService.GetProductTemplateById(product.ProductTemplateId);
				if (template == null)
					template = _productTemplateService.GetAllProductTemplates().FirstOrDefault();
				return template.ViewPath;
			});

			IList<ProductBundleItemData> bundleItems = null;
			ProductVariantAttributeCombination combination = null;
			var combinationImageIds = new List<int>();

			if (product.ProductType == ProductType.GroupedProduct && !isAssociatedProduct)	// associated products
			{
				var searchContext = new ProductSearchContext()
				{
					StoreId = _services.StoreContext.CurrentStore.Id,
					ParentGroupedProductId = product.Id,
					PageSize = int.MaxValue,
					VisibleIndividuallyOnly = false
				};

				var associatedProducts = _productService.SearchProducts(searchContext);

				foreach (var associatedProduct in associatedProducts)
					model.AssociatedProducts.Add(PrepareProductDetailsPageModel(associatedProduct, true));
			}
			else if (product.ProductType == ProductType.BundledProduct && productBundleItem == null)		// bundled items
			{
				bundleItems = _productService.GetBundleItems(product.Id);

				foreach (var itemData in bundleItems.Where(x => x.Item.Product.CanBeBundleItem()))
				{
					var item = itemData.Item;
					var bundledProductModel = PrepareProductDetailsPageModel(item.Product, false, itemData);

					bundledProductModel.BundleItem.Id = item.Id;
					bundledProductModel.BundleItem.Quantity = item.Quantity;
					bundledProductModel.BundleItem.HideThumbnail = item.HideThumbnail;
					bundledProductModel.BundleItem.Visible = item.Visible;
					bundledProductModel.BundleItem.IsBundleItemPricing = item.BundleProduct.BundlePerItemPricing;

					string bundleItemName = item.GetLocalized(x => x.Name);
					if (bundleItemName.HasValue())
						bundledProductModel.Name = bundleItemName;

					string bundleItemShortDescription = item.GetLocalized(x => x.ShortDescription);
					if (bundleItemShortDescription.HasValue())
						bundledProductModel.ShortDescription = bundleItemShortDescription;

					model.BundledItems.Add(bundledProductModel);
				}
			}

			model = PrepareProductDetailModel(model, product, isAssociatedProduct, productBundleItem, bundleItems, selectedAttributes);

			if (productBundleItem == null)
			{
				model.Combinations.GetAllCombinationImageIds(combinationImageIds);

				if (combination == null && model.CombinationSelected != null)
					combination = model.CombinationSelected;
			}

			// pictures
			var pictures = _pictureService.GetPicturesByProductId(product.Id);
			PrepareProductDetailsPictureModel(model.DetailsPictureModel, pictures, model.Name, combinationImageIds, isAssociatedProduct, productBundleItem, combination);

			return model;
		}

		public void PrepareProductReviewsModel(ProductReviewsModel model, Product product)
		{
			if (product == null)
				throw new ArgumentNullException("product");

			if (model == null)
				throw new ArgumentNullException("model");

			model.ProductId = product.Id;
			model.ProductName = product.GetLocalized(x => x.Name);
			model.ProductSeName = product.GetSeName();

			var productReviews = product.ProductReviews.Where(pr => pr.IsApproved).OrderBy(pr => pr.CreatedOnUtc);
			foreach (var pr in productReviews)
			{
				model.Items.Add(new ProductReviewModel()
				{
					Id = pr.Id,
					CustomerId = pr.CustomerId,
					CustomerName = pr.Customer.FormatUserName(),
					AllowViewingProfiles = _customerSettings.AllowViewingProfiles && pr.Customer != null && !pr.Customer.IsGuest(),
					Title = pr.Title,
					ReviewText = pr.ReviewText,
					Rating = pr.Rating,
					Helpfulness = new ProductReviewHelpfulnessModel()
					{
						ProductReviewId = pr.Id,
						HelpfulYesTotal = pr.HelpfulYesTotal,
						HelpfulNoTotal = pr.HelpfulNoTotal,
					},
					WrittenOnStr = _dateTimeHelper.ConvertToUserTime(pr.CreatedOnUtc, DateTimeKind.Utc).ToString("g"),
				});
			}

			model.AddProductReview.CanCurrentCustomerLeaveReview = _catalogSettings.AllowAnonymousUsersToReviewProduct || !_services.WorkContext.CurrentCustomer.IsGuest();
			model.AddProductReview.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnProductReviewPage;
		}

		private PictureModel CreatePictureModel(ProductDetailsPictureModel model, Picture picture, int pictureSize)
		{
			var result = new PictureModel()
			{
				PictureId = picture.Id,
				ThumbImageUrl = _pictureService.GetPictureUrl(picture, _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage),
				ImageUrl = _pictureService.GetPictureUrl(picture, pictureSize),
				FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
				Title = model.Name,
				AlternateText = model.AlternateText
			};

			return result;
		}

		public void PrepareProductDetailsPictureModel(ProductDetailsPictureModel model, IList<Picture> pictures, string name, List<int> allCombinationImageIds,
			bool isAssociatedProduct, ProductBundleItemData bundleItem = null, ProductVariantAttributeCombination combination = null)
		{
			model.Name = name;
			model.DefaultPictureZoomEnabled = _mediaSettings.DefaultPictureZoomEnabled;
			model.PictureZoomType = _mediaSettings.PictureZoomType;
			model.AlternateText = T("Media.Product.ImageAlternateTextFormat", model.Name);

			Picture defaultPicture = null;
			var combiAssignedImages = (combination == null ? null : combination.GetAssignedPictureIds());
			int defaultPictureSize;

			if (isAssociatedProduct)
				defaultPictureSize = _mediaSettings.AssociatedProductPictureSize;
			else if (bundleItem != null)
				defaultPictureSize = _mediaSettings.BundledProductPictureSize;
			else
				defaultPictureSize = _mediaSettings.ProductDetailsPictureSize;

			if (pictures.Count > 0)
			{
				if (pictures.Count <= _catalogSettings.DisplayAllImagesNumber)
				{
					// show all images
					foreach (var picture in pictures)
					{
						model.PictureModels.Add(CreatePictureModel(model, picture, _mediaSettings.ProductDetailsPictureSize));

						if (defaultPicture == null && combiAssignedImages != null && combiAssignedImages.Contains(picture.Id))
						{
							model.GalleryStartIndex = model.PictureModels.Count - 1;
							defaultPicture = picture;
						}
					}
				}
				else
				{
					// images not belonging to any combination...
					foreach (var picture in pictures.Where(p => !allCombinationImageIds.Contains(p.Id)))
					{
						model.PictureModels.Add(CreatePictureModel(model, picture, _mediaSettings.ProductDetailsPictureSize));
					}

					// plus images belonging to selected combination
					if (combiAssignedImages != null)
					{
						foreach (var picture in pictures.Where(p => combiAssignedImages.Contains(p.Id)))
						{
							model.PictureModels.Add(CreatePictureModel(model, picture, _mediaSettings.ProductDetailsPictureSize));

							if (defaultPicture == null)
							{
								model.GalleryStartIndex = model.PictureModels.Count - 1;
								defaultPicture = picture;
							}
						}
					}
				}

				if (defaultPicture == null)
				{
					model.GalleryStartIndex = 0;
					defaultPicture = pictures.First();
				}
			}

			// default picture
			if (defaultPicture == null)
			{
				model.DefaultPictureModel = new PictureModel()
				{
					ThumbImageUrl = _pictureService.GetDefaultPictureUrl(_mediaSettings.ProductThumbPictureSizeOnProductDetailsPage),
					ImageUrl = _pictureService.GetDefaultPictureUrl(defaultPictureSize),
					FullSizeImageUrl = _pictureService.GetDefaultPictureUrl(),
					Title = T("Media.Product.ImageLinkTitleFormat", model.Name),
					AlternateText = model.AlternateText
				};
			}
			else
			{
				model.DefaultPictureModel = CreatePictureModel(model, defaultPicture, defaultPictureSize);
			}
		}

		/// <param name="selectedAttributes">Attributes explicitly selected by user or by query string.</param>
		public ProductDetailsModel PrepareProductDetailModel(
			ProductDetailsModel model,
			Product product,
			bool isAssociatedProduct = false,
			ProductBundleItemData productBundleItem = null,
			IList<ProductBundleItemData> productBundleItems = null,
			FormCollection selectedAttributes = null,
			int selectedQuantity = 1)
		{
			if (product == null)
				throw new ArgumentNullException("product");

			if (model == null)
				throw new ArgumentNullException("model");

			if (selectedAttributes == null)
				selectedAttributes = new FormCollection();

			decimal preSelectedPriceAdjustmentBase = decimal.Zero;
			decimal preSelectedWeightAdjustment = decimal.Zero;
			bool displayPrices = _services.Permissions.Authorize(StandardPermissionProvider.DisplayPrices);
			bool isBundle = (product.ProductType == ProductType.BundledProduct);
			bool isBundleItemPricing = (productBundleItem != null && productBundleItem.Item.BundleProduct.BundlePerItemPricing);
			bool isBundlePricing = (productBundleItem != null && !productBundleItem.Item.BundleProduct.BundlePerItemPricing);
			int bundleItemId = (productBundleItem == null ? 0 : productBundleItem.Item.Id);

			bool hasSelectedAttributesValues = false;
			bool hasSelectedAttributes = (selectedAttributes.Count > 0);
			List<ProductVariantAttributeValue> selectedAttributeValues = null;

			var variantAttributes = (isBundle ? new List<ProductVariantAttribute>() : _productAttributeService.GetProductVariantAttributesByProductId(product.Id));

			model.ProductPrice.DynamicPriceUpdate = _catalogSettings.EnableDynamicPriceUpdate;
			model.ProductPrice.BundleItemShowBasePrice = _catalogSettings.BundleItemShowBasePrice;

			if (!model.ProductPrice.DynamicPriceUpdate)
				selectedQuantity = 1;

			#region Product attributes

			if (!isBundle)		// bundles doesn't have attributes
			{
				foreach (var attribute in variantAttributes)
				{
					var pvaModel = new ProductDetailsModel.ProductVariantAttributeModel
					{
						Id = attribute.Id,
						ProductId = attribute.ProductId,
						BundleItemId = bundleItemId,
						ProductAttributeId = attribute.ProductAttributeId,
						Alias = attribute.ProductAttribute.Alias,
						Name = attribute.ProductAttribute.GetLocalized(x => x.Name),
						Description = attribute.ProductAttribute.GetLocalized(x => x.Description),
						TextPrompt = attribute.TextPrompt,
						IsRequired = attribute.IsRequired,
						AttributeControlType = attribute.AttributeControlType,
						AllowedFileExtensions = _catalogSettings.FileUploadAllowedExtensions
					};

					if (attribute.AttributeControlType == AttributeControlType.Datepicker)
					{
						if (pvaModel.Alias.HasValue() && RegularExpressions.IsYearRange.IsMatch(pvaModel.Alias))
						{
							var match = RegularExpressions.IsYearRange.Match(pvaModel.Alias);
							pvaModel.BeginYear = match.Groups[1].Value.ToInt();
							pvaModel.EndYear = match.Groups[2].Value.ToInt();
						}
					}

					int preSelectedValueId = 0;
					var pvaValues = (attribute.ShouldHaveValues() ? _productAttributeService.GetProductVariantAttributeValues(attribute.Id) : new List<ProductVariantAttributeValue>());

					foreach (var pvaValue in pvaValues)
					{
						ProductBundleItemAttributeFilter attributeFilter = null;

						if (productBundleItem.FilterOut(pvaValue, out attributeFilter))
							continue;

						if (preSelectedValueId == 0 && attributeFilter != null && attributeFilter.IsPreSelected)
							preSelectedValueId = attributeFilter.AttributeValueId;

						var linkedProduct = _productService.GetProductById(pvaValue.LinkedProductId);

						var pvaValueModel = new ProductDetailsModel.ProductVariantAttributeValueModel();
						pvaValueModel.Id = pvaValue.Id;
						pvaValueModel.Name = pvaValue.GetLocalized(x => x.Name);
						pvaValueModel.Alias = pvaValue.Alias;
						pvaValueModel.ColorSquaresRgb = pvaValue.ColorSquaresRgb; //used with "Color squares" attribute type
						pvaValueModel.IsPreSelected = pvaValue.IsPreSelected;

						if (linkedProduct != null && linkedProduct.VisibleIndividually)
							pvaValueModel.SeName = linkedProduct.GetSeName();

						if (hasSelectedAttributes)
							pvaValueModel.IsPreSelected = false;	// explicitly selected always discards pre-selected by merchant

						// display price if allowed
						if (displayPrices && !isBundlePricing)
						{
							decimal taxRate = decimal.Zero;
							decimal attributeValuePriceAdjustment = _priceCalculationService.GetProductVariantAttributeValuePriceAdjustment(pvaValue);
							decimal priceAdjustmentBase = _taxService.GetProductPrice(product, attributeValuePriceAdjustment, out taxRate);
							decimal priceAdjustment = _currencyService.ConvertFromPrimaryStoreCurrency(priceAdjustmentBase, _services.WorkContext.WorkingCurrency);

							if (priceAdjustmentBase > decimal.Zero)
								pvaValueModel.PriceAdjustment = "+" + _priceFormatter.FormatPrice(priceAdjustment, true, false);
							else if (priceAdjustmentBase < decimal.Zero)
								pvaValueModel.PriceAdjustment = "-" + _priceFormatter.FormatPrice(-priceAdjustment, true, false);

							if (pvaValueModel.IsPreSelected)
							{
								preSelectedPriceAdjustmentBase = decimal.Add(preSelectedPriceAdjustmentBase, priceAdjustmentBase);
								preSelectedWeightAdjustment = decimal.Add(preSelectedWeightAdjustment, pvaValue.WeightAdjustment);
							}

							if (_catalogSettings.ShowLinkedAttributeValueQuantity && pvaValue.ValueType == ProductVariantAttributeValueType.ProductLinkage)
							{
								pvaValueModel.QuantityInfo = pvaValue.Quantity;
							}

							pvaValueModel.PriceAdjustmentValue = priceAdjustment;
						}

						if (!_catalogSettings.ShowVariantCombinationPriceAdjustment)
						{
							pvaValueModel.PriceAdjustment = "";
						}

						if (_catalogSettings.ShowLinkedAttributeValueImage && pvaValue.ValueType == ProductVariantAttributeValueType.ProductLinkage)
						{
							var linkagePicture = _pictureService.GetPicturesByProductId(pvaValue.LinkedProductId, 1).FirstOrDefault();
							if (linkagePicture != null)
								pvaValueModel.ImageUrl = _pictureService.GetPictureUrl(linkagePicture, _mediaSettings.AutoCompleteSearchThumbPictureSize, false);
						}

						pvaModel.Values.Add(pvaValueModel);
					}

					// we need selected attributes to get initially displayed combination images
					if (!hasSelectedAttributes)
					{
						ProductDetailsModel.ProductVariantAttributeValueModel defaultValue = null;

						if (preSelectedValueId != 0)	// value pre-selected by a bundle item filter discards the default pre-selection
						{
							pvaModel.Values.Each(x => x.IsPreSelected = false);

							if ((defaultValue = pvaModel.Values.FirstOrDefault(v => v.Id == preSelectedValueId)) != null)
							{
								defaultValue.IsPreSelected = true;
								selectedAttributes.AddProductAttribute(attribute.ProductAttributeId, attribute.Id, defaultValue.Id, product.Id, bundleItemId);
							}
						}

						if (defaultValue == null)
						{
							foreach (var value in pvaModel.Values.Where(x => x.IsPreSelected))
							{
								selectedAttributes.AddProductAttribute(attribute.ProductAttributeId, attribute.Id, value.Id, product.Id, bundleItemId);
							}
						}

						//if (defaultValue == null)
						//	defaultValue = pvaModel.Values.FirstOrDefault(v => v.IsPreSelected);

						//if (defaultValue != null)
						//	selectedAttributes.AddProductAttribute(attribute.ProductAttributeId, attribute.Id, defaultValue.Id, product.Id, bundleItemId);
					}

					model.ProductVariantAttributes.Add(pvaModel);
				}
			}

			#endregion

			#region Attribute combinations

			if (!isBundle)
			{
				model.Combinations = _productAttributeService.GetAllProductVariantAttributeCombinations(product.Id);

				if (selectedAttributes.Count > 0)
				{
					// merge with combination data if there's a match
					var warnings = new List<string>();
					string attributeXml = selectedAttributes.CreateSelectedAttributesXml(product.Id, variantAttributes, _productAttributeParser, _localizationService,
						_downloadService, _catalogSettings, _httpRequest, warnings, true, bundleItemId);

					selectedAttributeValues = _productAttributeParser.ParseProductVariantAttributeValues(attributeXml).ToList();
					hasSelectedAttributesValues = (selectedAttributeValues.Count > 0);

					if (isBundlePricing)
					{
						model.AttributeInfo = _productAttributeFormatter.FormatAttributes(product, attributeXml, _services.WorkContext.CurrentCustomer,
							renderPrices: false, renderGiftCardAttributes: false, allowHyperlinks: false);
					}

					model.CombinationSelected = model.Combinations
						.FirstOrDefault(x => _productAttributeParser.AreProductAttributesEqual(x.AttributesXml, attributeXml, variantAttributes));

					if (model.CombinationSelected != null && model.CombinationSelected.IsActive == false)
					{
						model.IsAvailable = false;
                        model.StockAvailability = T("Products.Availability.IsNotActive");
					}

					product.MergeWithCombination(model.CombinationSelected);

					// mark explicitly selected as pre-selected
					foreach (var attribute in model.ProductVariantAttributes)
					{
						foreach (var value in attribute.Values)
						{
							if (selectedAttributeValues.FirstOrDefault(v => v.Id == value.Id) != null)
								value.IsPreSelected = true;

							if (!_catalogSettings.ShowVariantCombinationPriceAdjustment)
								value.PriceAdjustment = "";
						}
					}
				}
			}

			#endregion

			#region Properties

			if ((productBundleItem != null && !productBundleItem.Item.BundleProduct.BundlePerItemShoppingCart) ||
				(product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes && !hasSelectedAttributesValues))
			{
				// cases where stock inventory is not functional. determined by what ShoppingCartService.GetStandardWarnings and ProductService.AdjustInventory is not handling.
				model.IsAvailable = true;
                model.StockAvailability = product.ProductVariantAttributeCombinations.Count == 0 ? product.FormatStockMessage(_localizationService) : "";
			}
			else if (model.IsAvailable)
			{
				model.IsAvailable = product.IsAvailableByStock();
				model.StockAvailability = product.FormatStockMessage(_localizationService);
			}

			model.Id = product.Id;
			model.Name = product.GetLocalized(x => x.Name);
			model.ShowSku = _catalogSettings.ShowProductSku;
			model.Sku = product.Sku;
			model.ShortDescription = product.GetLocalized(x => x.ShortDescription);
			model.FullDescription = product.GetLocalized(x => x.FullDescription);
			model.MetaKeywords = product.GetLocalized(x => x.MetaKeywords);
			model.MetaDescription = product.GetLocalized(x => x.MetaDescription);
			model.MetaTitle = product.GetLocalized(x => x.MetaTitle);
			model.SeName = product.GetSeName();
			model.ShowManufacturerPartNumber = _catalogSettings.ShowManufacturerPartNumber;
			model.ManufacturerPartNumber = product.ManufacturerPartNumber;
			model.ShowDimensions = _catalogSettings.ShowDimensions;
			model.ShowWeight = _catalogSettings.ShowWeight;
			model.ShowGtin = _catalogSettings.ShowGtin;
			model.Gtin = product.Gtin;
			model.HasSampleDownload = product.IsDownload && product.HasSampleDownload;
			model.IsCurrentCustomerRegistered = _services.WorkContext.CurrentCustomer.IsRegistered();
			model.IsBasePriceEnabled = product.BasePriceEnabled;
			model.BasePriceInfo = product.GetBasePriceInfo(_localizationService, _priceFormatter);
			model.ShowLegalInfo = _taxSettings.ShowLegalHintsInProductDetails;
			model.BundleTitleText = product.GetLocalized(x => x.BundleTitleText);
			model.BundlePerItemPricing = product.BundlePerItemPricing;
			model.BundlePerItemShipping = product.BundlePerItemShipping;
			model.BundlePerItemShoppingCart = product.BundlePerItemShoppingCart;

			//_taxSettings.TaxDisplayType == TaxDisplayType.ExcludingTax;

			string taxInfo = (_services.WorkContext.GetTaxDisplayTypeFor(_services.WorkContext.CurrentCustomer, _services.StoreContext.CurrentStore.Id) == TaxDisplayType.IncludingTax)
				? T("Tax.InclVAT")
				: T("Tax.ExclVAT");

			string defaultTaxRate = "";
			var taxrate = Convert.ToString(_taxService.GetTaxRate(product, _services.WorkContext.CurrentCustomer));
			if (_taxSettings.DisplayTaxRates && !taxrate.Equals("0", StringComparison.InvariantCultureIgnoreCase))
			{
				defaultTaxRate = "({0}%)".FormatWith(taxrate);
			}

			var addShippingPrice = _currencyService.ConvertFromPrimaryStoreCurrency(product.AdditionalShippingCharge, _services.WorkContext.WorkingCurrency);
			string additionalShippingCosts = String.Empty;
			if (addShippingPrice > 0)
			{
				additionalShippingCosts = T("Common.AdditionalShippingSurcharge").Text.FormatWith(_priceFormatter.FormatPrice(addShippingPrice, true, false)) + ", ";
			}

			string shippingInfoLink = _urlHelper.RouteUrl("Topic", new { SystemName = "shippinginfo" });

            if (!product.IsShipEnabled || (addShippingPrice == 0 && product.IsFreeShipping))
            {
                model.LegalInfo += "{0} {1}, {2}".FormatWith(
                    product.IsTaxExempt ? "" : taxInfo,
                    product.IsTaxExempt ? "" : defaultTaxRate,
                    T("Common.FreeShipping"));
            }
            else
            {
                model.LegalInfo = T("Tax.LegalInfoProductDetail",
                    product.IsTaxExempt ? "" : taxInfo,
                    product.IsTaxExempt ? "" : defaultTaxRate,
                    additionalShippingCosts,
                    shippingInfoLink);
            }

			string dimension = _measureService.GetMeasureDimensionById(_measureSettings.BaseDimensionId).Name;

			model.WeightValue = product.Weight;
			if (!isBundle)
			{
				if (selectedAttributeValues != null)
				{
					foreach (var attributeValue in selectedAttributeValues)
						model.WeightValue = decimal.Add(model.WeightValue, attributeValue.WeightAdjustment);
				}
				else
				{
					model.WeightValue = decimal.Add(model.WeightValue, preSelectedWeightAdjustment);
				}
			}

			model.Weight = (model.WeightValue > 0) ? "{0} {1}".FormatCurrent(model.WeightValue.ToString("F2"), _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId).Name) : "";
			model.Height = (product.Height > 0) ? "{0} {1}".FormatCurrent(product.Height.ToString("F2"), dimension) : "";
			model.Length = (product.Length > 0) ? "{0} {1}".FormatCurrent(product.Length.ToString("F2"), dimension) : "";
			model.Width = (product.Width > 0) ? "{0} {1}".FormatCurrent(product.Width.ToString("F2"), dimension) : "";

			if (productBundleItem != null)
				model.ThumbDimensions = _mediaSettings.BundledProductPictureSize;
			else if (isAssociatedProduct)
				model.ThumbDimensions = _mediaSettings.AssociatedProductPictureSize;

			if (model.IsAvailable)
			{
				var deliveryTime = _deliveryTimeService.GetDeliveryTime(product);
				if (deliveryTime != null)
				{
					model.DeliveryTimeName = deliveryTime.GetLocalized(x => x.Name);
					model.DeliveryTimeHexValue = deliveryTime.ColorHexValue;
				}
			}

			model.DisplayDeliveryTime = _catalogSettings.ShowDeliveryTimesInProductDetail;
			model.IsShipEnabled = product.IsShipEnabled;
			model.DisplayDeliveryTimeAccordingToStock = product.DisplayDeliveryTimeAccordingToStock(_catalogSettings);

			if (model.DeliveryTimeName.IsEmpty() && model.DisplayDeliveryTime)
			{
				model.DeliveryTimeName = T("ShoppingCart.NotAvailable");
			}

            var quantityUnit = _quantityUnitService.GetQuantityUnit(product);
            if (quantityUnit != null)
            {
                model.QuantityUnitName = quantityUnit.GetLocalized(x => x.Name);
            }

			//back in stock subscriptions)
			if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
				product.BackorderMode == BackorderMode.NoBackorders &&
				product.AllowBackInStockSubscriptions &&
				product.StockQuantity <= 0)
			{
				//out of stock
				model.DisplayBackInStockSubscription = true;
				model.BackInStockAlreadySubscribed = _backInStockSubscriptionService
					 .FindSubscription(_services.WorkContext.CurrentCustomer.Id, product.Id, _services.StoreContext.CurrentStore.Id) != null;
			}

			#endregion

			#region Product price

			model.ProductPrice.ProductId = product.Id;

			if (displayPrices)
			{
				model.ProductPrice.HidePrices = false;

				if (product.CustomerEntersPrice && !isBundleItemPricing)
				{
					model.ProductPrice.CustomerEntersPrice = true;
				}
				else
				{
					if (product.CallForPrice && !isBundleItemPricing)
					{
						model.ProductPrice.CallForPrice = true;
					}
					else
					{
						decimal taxRate = decimal.Zero;
						decimal oldPrice = decimal.Zero;
						decimal finalPriceWithoutDiscountBase = decimal.Zero;
						decimal finalPriceWithDiscountBase = decimal.Zero;
						decimal attributesTotalPriceBase = decimal.Zero;
						decimal finalPriceWithoutDiscount = decimal.Zero;
						decimal finalPriceWithDiscount = decimal.Zero;

						decimal oldPriceBase = _taxService.GetProductPrice(product, product.OldPrice, out taxRate);

						if (model.ProductPrice.DynamicPriceUpdate && !isBundlePricing)
						{
							if (selectedAttributeValues != null)
							{
								selectedAttributeValues.Each(x => attributesTotalPriceBase += _priceCalculationService.GetProductVariantAttributeValuePriceAdjustment(x));
							}
							else
							{
								attributesTotalPriceBase = preSelectedPriceAdjustmentBase;
							}
						}

						if (productBundleItem != null)
						{
							productBundleItem.AdditionalCharge = attributesTotalPriceBase;
						}

						finalPriceWithoutDiscountBase = _priceCalculationService.GetFinalPrice(product, productBundleItems,
							_services.WorkContext.CurrentCustomer, attributesTotalPriceBase, false, selectedQuantity, productBundleItem);

						finalPriceWithDiscountBase = _priceCalculationService.GetFinalPrice(product, productBundleItems,
							_services.WorkContext.CurrentCustomer, attributesTotalPriceBase, true, selectedQuantity, productBundleItem);

						finalPriceWithoutDiscountBase = _taxService.GetProductPrice(product, finalPriceWithoutDiscountBase, out taxRate);
						finalPriceWithDiscountBase = _taxService.GetProductPrice(product, finalPriceWithDiscountBase, out taxRate);

						oldPrice = _currencyService.ConvertFromPrimaryStoreCurrency(oldPriceBase, _services.WorkContext.WorkingCurrency);

						finalPriceWithoutDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithoutDiscountBase, _services.WorkContext.WorkingCurrency);
						finalPriceWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithDiscountBase, _services.WorkContext.WorkingCurrency);

						if (productBundleItem == null || isBundleItemPricing)
						{
							if (finalPriceWithoutDiscountBase != oldPriceBase && oldPriceBase > decimal.Zero)
								model.ProductPrice.OldPrice = _priceFormatter.FormatPrice(oldPrice);

							model.ProductPrice.Price = _priceFormatter.FormatPrice(finalPriceWithoutDiscount);

							if (finalPriceWithoutDiscountBase != finalPriceWithDiscountBase)
								model.ProductPrice.PriceWithDiscount = _priceFormatter.FormatPrice(finalPriceWithDiscount);
						}

						model.ProductPrice.PriceValue = finalPriceWithoutDiscount;
						model.ProductPrice.PriceWithDiscountValue = finalPriceWithDiscount;
						model.BasePriceInfo = product.GetBasePriceInfo(_localizationService, _priceFormatter, attributesTotalPriceBase);

						if (!string.IsNullOrWhiteSpace(model.ProductPrice.OldPrice) || !string.IsNullOrWhiteSpace(model.ProductPrice.PriceWithDiscount))
						{
							model.ProductPrice.NoteWithoutDiscount = T(isBundle && product.BundlePerItemPricing ? "Products.Bundle.PriceWithoutDiscount.Note" : "Products.Price");
						}

						if ((isBundle && product.BundlePerItemPricing && !string.IsNullOrWhiteSpace(model.ProductPrice.PriceWithDiscount)) || product.HasTierPrices)
						{
                            if (!product.HasTierPrices)
                            {
                                model.ProductPrice.NoteWithDiscount = T("Products.Bundle.PriceWithDiscount.Note");
                            }
                            model.BasePriceInfo = product.GetBasePriceInfo(_localizationService, _priceFormatter, (product.Price - finalPriceWithDiscount) * (-1));
						}
					}
				}
			}
			else
			{
				model.ProductPrice.HidePrices = true;
				model.ProductPrice.OldPrice = null;
				model.ProductPrice.Price = null;
			}
			#endregion

			#region 'Add to cart' model

			model.AddToCart.ProductId = product.Id;

			//quantity
			model.AddToCart.EnteredQuantity = product.OrderMinimumQuantity;

			//'add to cart', 'add to wishlist' buttons
			model.AddToCart.DisableBuyButton = product.DisableBuyButton || !_services.Permissions.Authorize(StandardPermissionProvider.EnableShoppingCart);
			model.AddToCart.DisableWishlistButton = product.DisableWishlistButton || !_services.Permissions.Authorize(StandardPermissionProvider.EnableWishlist);
			if (!displayPrices)
			{
				model.AddToCart.DisableBuyButton = true;
				model.AddToCart.DisableWishlistButton = true;
			}
			//pre-order
			model.AddToCart.AvailableForPreOrder = product.AvailableForPreOrder;

			//customer entered price
			model.AddToCart.CustomerEntersPrice = product.CustomerEntersPrice;
			if (model.AddToCart.CustomerEntersPrice)
			{
				decimal minimumCustomerEnteredPrice = _currencyService.ConvertFromPrimaryStoreCurrency(product.MinimumCustomerEnteredPrice, _services.WorkContext.WorkingCurrency);
				decimal maximumCustomerEnteredPrice = _currencyService.ConvertFromPrimaryStoreCurrency(product.MaximumCustomerEnteredPrice, _services.WorkContext.WorkingCurrency);

				model.AddToCart.CustomerEnteredPrice = minimumCustomerEnteredPrice;
				model.AddToCart.CustomerEnteredPriceRange = string.Format(T("Products.EnterProductPrice.Range"),
					_priceFormatter.FormatPrice(minimumCustomerEnteredPrice, true, false),
					_priceFormatter.FormatPrice(maximumCustomerEnteredPrice, true, false));
			}
			//allowed quantities
			var allowedQuantities = product.ParseAllowedQuatities();
			foreach (var qty in allowedQuantities)
			{
				model.AddToCart.AllowedQuantities.Add(new SelectListItem()
				{
					Text = qty.ToString(),
					Value = qty.ToString()
				});
			}

			#endregion

			#region Gift card

			model.GiftCard.IsGiftCard = product.IsGiftCard;
			if (model.GiftCard.IsGiftCard)
			{
				model.GiftCard.GiftCardType = product.GiftCardType;
				model.GiftCard.SenderName = _services.WorkContext.CurrentCustomer.GetFullName();
				model.GiftCard.SenderEmail = _services.WorkContext.CurrentCustomer.Email;
			}

			#endregion

			return model;
		}

		public List<int> GetChildCategoryIds(int parentCategoryId)
		{
			var customerRolesIds = _services.WorkContext.CurrentCustomer.CustomerRoles.Where(cr => cr.Active).Select(cr => cr.Id).ToList();
			string cacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_CHILD_IDENTIFIERS_MODEL_KEY,
				parentCategoryId,
				false,
				string.Join(",", customerRolesIds),
				_services.StoreContext.CurrentStore.Id);

			return _services.Cache.Get(cacheKey, () =>
			{
				var root = GetCategoryMenu();
				var node = root.SelectNode(x => x.Value.EntityId == parentCategoryId);
				if (node != null)
				{
					var ids = node.Flatten(false).Select(x => x.EntityId).ToList();
					return ids;
				}
				return new List<int>();
			});
		}

		public IList<MenuItem> GetCategoryBreadCrumb(int currentCategoryId, int currentProductId)
		{
			var requestCache = SmartStore.Core.Infrastructure.EngineContext.Current.Resolve<ICacheManager>();
			string cacheKey = "sm.temp.category.path.{0}-{1}".FormatInvariant(currentCategoryId, currentProductId);

			var breadcrumb = requestCache.Get(cacheKey, () =>
			{
				var root = GetCategoryMenu();
				TreeNode<MenuItem> node = null;

				if (currentCategoryId > 0)
				{
					node = root.SelectNode(x => x.Value.EntityId == currentCategoryId);
				}

				if (node == null && currentProductId > 0)
				{
					var productCategories = _categoryService.GetProductCategoriesByProductId(currentProductId);
					if (productCategories.Count > 0)
					{
						currentCategoryId = productCategories[0].Category.Id;
						node = root.SelectNode(x => x.Value.EntityId == currentCategoryId);
					}
				}

				if (node != null)
				{
					var path = node.GetBreadcrumb();
					return path;
				}

				return new List<MenuItem>();
			});

			return breadcrumb;
		}

		public IEnumerable<ProductOverviewModel> PrepareProductOverviewModels(
			IEnumerable<Product> products,
			bool preparePriceModel = true, 
			bool preparePictureModel = true,
			int? productThumbPictureSize = null, 
			bool prepareSpecificationAttributes = false,
			bool forceRedirectionAfterAddingToCart = false, 
			bool prepareColorAttributes = false,
			bool prepareManufacturers = false,
            bool isCompact = false,
			bool prepareFullDescription = false)
		{
			if (products == null)
				throw new ArgumentNullException("products");

			// PERF!!
			var currentStore = _services.StoreContext.CurrentStore;
			var currentCustomer = _services.WorkContext.CurrentCustomer;
			var workingCurrency = _services.WorkContext.WorkingCurrency;
			var displayPrices = _services.Permissions.Authorize(StandardPermissionProvider.DisplayPrices);
			var enableShoppingCart = _services.Permissions.Authorize(StandardPermissionProvider.EnableShoppingCart);
			var enableWishlist = _services.Permissions.Authorize(StandardPermissionProvider.EnableWishlist);
			var taxDisplayType = _services.WorkContext.GetTaxDisplayTypeFor(currentCustomer, currentStore.Id);
			string taxInfo = T(taxDisplayType == TaxDisplayType.IncludingTax ? "Tax.InclVAT" : "Tax.ExclVAT");
			string shippingInfoLink = _urlHelper.RouteUrl("Topic", new { SystemName = "shippinginfo" });
			var cachedManufacturerModels = new Dictionary<int, ManufacturerOverviewModel>();

			var res = new Dictionary<string, LocalizedString>(StringComparer.OrdinalIgnoreCase)
			{
				{ "Products.CallForPrice", T("Products.CallForPrice") },
				{ "Products.PriceRangeFrom", T("Products.PriceRangeFrom") },
				{ "Media.Product.ImageLinkTitleFormat", T("Media.Product.ImageLinkTitleFormat") },
				{ "Media.Product.ImageAlternateTextFormat", T("Media.Product.ImageAlternateTextFormat") },
				{ "Products.DimensionsValue", T("Products.DimensionsValue") },
				{ "Tax.LegalInfoFooter", T("Tax.LegalInfoFooter") },
				{ "Common.AdditionalShippingSurcharge", T("Common.AdditionalShippingSurcharge") }
			};

			var cargoData = _priceCalculationService.CreatePriceCalculationContext(products);

			var models = new List<ProductOverviewModel>();

			foreach (var product in products)
			{
				var contextProduct = product;

				var model = new ProductOverviewModel
				{
					Id = product.Id,
					Name = product.GetLocalized(x => x.Name).EmptyNull(),
					ShortDescription = product.GetLocalized(x => x.ShortDescription),
					SeName = product.GetSeName()
				};

				if (prepareFullDescription)
				{
					model.FullDescription = product.GetLocalized(x => x.FullDescription);
				}

				// price
				if (preparePriceModel)
				{
					#region Prepare product price

					var priceModel = new ProductOverviewModel.ProductPriceModel
					{
						ForceRedirectionAfterAddingToCart = forceRedirectionAfterAddingToCart,
						ShowDiscountSign = _catalogSettings.ShowDiscountSign
					};

					if (product.ProductType == ProductType.GroupedProduct)
					{
						#region Grouped product

						priceModel.DisableBuyButton = true;
						priceModel.DisableWishListButton = true;
						priceModel.AvailableForPreOrder = false;

						var searchContext = new ProductSearchContext
						{
							StoreId = currentStore.Id,
							ParentGroupedProductId = product.Id,
							PageSize = int.MaxValue,
							VisibleIndividuallyOnly = false
						};

						var associatedProducts = _productService.SearchProducts(searchContext);

						if (associatedProducts.Count > 0)
						{
							contextProduct = associatedProducts.OrderBy(x => x.DisplayOrder).First();

							if (displayPrices && _catalogSettings.PriceDisplayType != PriceDisplayType.Hide)
							{
								decimal? displayPrice = null;
								bool displayFromMessage = false;

								if (_catalogSettings.PriceDisplayType == PriceDisplayType.PreSelectedPrice)
								{
									displayPrice = _priceCalculationService.GetPreselectedPrice(contextProduct, cargoData);
								}
								else if (_catalogSettings.PriceDisplayType == PriceDisplayType.PriceWithoutDiscountsAndAttributes)
								{
									displayPrice = _priceCalculationService.GetFinalPrice(contextProduct, null, currentCustomer, decimal.Zero, false, 1, null, cargoData);
								}
								else
								{
									displayFromMessage = true;
									displayPrice = _priceCalculationService.GetLowestPrice(product, cargoData, associatedProducts, out contextProduct);
								}	

								if (contextProduct != null && !contextProduct.CustomerEntersPrice)
								{
									if (contextProduct.CallForPrice)
									{
										priceModel.OldPrice = null;
										priceModel.Price = res["Products.CallForPrice"];
									}
									else if (displayPrice.HasValue)
									{
										//calculate prices
										decimal taxRate = decimal.Zero;
										decimal oldPriceBase = _taxService.GetProductPrice(contextProduct, contextProduct.OldPrice, out taxRate);
										decimal finalPriceBase = _taxService.GetProductPrice(contextProduct, displayPrice.Value, out taxRate);
										decimal finalPrice = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceBase, workingCurrency);

										priceModel.OldPrice = null;

										if (displayFromMessage)
											priceModel.Price = String.Format(res["Products.PriceRangeFrom"], _priceFormatter.FormatPrice(finalPrice));
										else
											priceModel.Price = _priceFormatter.FormatPrice(finalPrice);

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
						priceModel.DisableBuyButton = product.DisableBuyButton || !enableShoppingCart || !displayPrices;

						//add to wishlist button
						priceModel.DisableWishListButton = product.DisableWishlistButton || !enableWishlist || !displayPrices;

						//pre-order
						priceModel.AvailableForPreOrder = product.AvailableForPreOrder;

						//prices
						if (displayPrices && _catalogSettings.PriceDisplayType != PriceDisplayType.Hide && !product.CustomerEntersPrice)
						{
							if (product.CallForPrice)
							{
								//call for price
								priceModel.OldPrice = null;
								priceModel.Price = res["Products.CallForPrice"];
							}
							else
							{
								//calculate prices
								bool displayFromMessage = false;
								decimal displayPrice = decimal.Zero;

								if (_catalogSettings.PriceDisplayType == PriceDisplayType.PreSelectedPrice)
								{
									displayPrice = _priceCalculationService.GetPreselectedPrice(product, cargoData);
								}
								else if (_catalogSettings.PriceDisplayType == PriceDisplayType.PriceWithoutDiscountsAndAttributes)
								{
									displayPrice = _priceCalculationService.GetFinalPrice(product, null, currentCustomer, decimal.Zero, false, 1, null, cargoData);
								}
								else
								{
									displayPrice = _priceCalculationService.GetLowestPrice(product, cargoData, out displayFromMessage);
								}

								decimal taxRate = decimal.Zero;
								decimal oldPriceBase = _taxService.GetProductPrice(product, product.OldPrice, out taxRate);
								decimal finalPriceBase = _taxService.GetProductPrice(product, displayPrice, out taxRate);

								decimal oldPrice = _currencyService.ConvertFromPrimaryStoreCurrency(oldPriceBase, workingCurrency);
								decimal finalPrice = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceBase, workingCurrency);

								priceModel.HasDiscount = (finalPriceBase != oldPriceBase && oldPriceBase != decimal.Zero);

								if (displayFromMessage)
								{
									priceModel.OldPrice = null;
									priceModel.Price = String.Format(res["Products.PriceRangeFrom"], _priceFormatter.FormatPrice(finalPrice));
								}
								else
								{
									if (priceModel.HasDiscount)
									{
										priceModel.OldPrice = _priceFormatter.FormatPrice(oldPrice);
										priceModel.Price = _priceFormatter.FormatPrice(finalPrice);
									}
									else
									{
										priceModel.OldPrice = null;
										priceModel.Price = _priceFormatter.FormatPrice(finalPrice);
									}
								}
							}
						}

						#endregion
					}

					model.ProductPrice = priceModel;
                    model.ProductPrice.CallForPrice = product.CallForPrice;

					#endregion
				}

				// color squares
				if (prepareColorAttributes && _catalogSettings.ShowColorSquaresInLists)
				{
					#region Prepare color attributes

					var attributes = cargoData.Attributes.Load(contextProduct.Id);
					var colorAttribute = attributes.FirstOrDefault(x => x.AttributeControlType == AttributeControlType.ColorSquares);

					if (colorAttribute != null)
					{
						var colorValues =
							from a in colorAttribute.ProductVariantAttributeValues.Take(50)
							where (a.ColorSquaresRgb.HasValue() && !a.ColorSquaresRgb.IsCaseInsensitiveEqual("transparent"))
							select new ProductOverviewModel.ColorAttributeModel
							{
								Color = a.ColorSquaresRgb,
								Alias = a.Alias,
								FriendlyName = a.GetLocalized(l => l.Name)
							};

						if (colorValues.Any())
						{
							model.ColorAttributes.AddRange(colorValues.Distinct());
						}
					}

					#endregion
				}

				// picture
				if (preparePictureModel)
				{
					#region Prepare product picture

					//If a size has been set in the view, we use it in priority
					int pictureSize = productThumbPictureSize.HasValue ? productThumbPictureSize.Value : _mediaSettings.ProductThumbPictureSize;

					//prepare picture model
					var defaultProductPictureCacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_DEFAULTPICTURE_MODEL_KEY, product.Id, pictureSize, true,
						_services.WorkContext.WorkingLanguage.Id, _services.WebHelper.IsCurrentConnectionSecured(), currentStore.Id);

					model.DefaultPictureModel = _services.Cache.Get(defaultProductPictureCacheKey, () =>
					{
						var picture = product.GetDefaultProductPicture(_pictureService);
						var pictureModel = new PictureModel
						{
							ImageUrl = _pictureService.GetPictureUrl(picture, pictureSize),
							FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
							Title = string.Format(res["Media.Product.ImageLinkTitleFormat"], model.Name),
							AlternateText = string.Format(res["Media.Product.ImageAlternateTextFormat"], model.Name)
						};
						return pictureModel;
					});

					#endregion
				}

				// specs
				if (prepareSpecificationAttributes)
				{
					model.SpecificationAttributeModels = PrepareProductSpecificationModel(product);
				}
				
				model.MinPriceProductId = contextProduct.Id;
				model.ShowSku = _catalogSettings.ShowProductSku;
				model.ShowWeight = _catalogSettings.ShowWeight;
				model.ShowDimensions = _catalogSettings.ShowDimensions;
				model.Sku = contextProduct.Sku;
				model.Dimensions = res["Products.DimensionsValue"].Text.FormatCurrent(
					contextProduct.Width.ToString("F2"),
					contextProduct.Height.ToString("F2"),
					contextProduct.Length.ToString("F2")
				);
                model.DimensionMeasureUnit = _measureService.GetMeasureDimensionById(_measureSettings.BaseDimensionId).Name;
				model.ThumbDimension = _mediaSettings.ProductThumbPictureSize;
				model.ShowLegalInfo = _taxSettings.ShowLegalHintsInProductList;
				model.LegalInfo = res["Tax.LegalInfoFooter"].Text.FormatWith(taxInfo, shippingInfoLink);
				model.RatingSum = product.ApprovedRatingSum;
				model.TotalReviews = product.ApprovedTotalReviews;
				model.ShowReviews = _catalogSettings.ShowProductReviewsInProductLists;
				model.ShowDeliveryTimes = _catalogSettings.ShowDeliveryTimesInProductLists;
				model.InvisibleDeliveryTime = (product.ProductType == ProductType.GroupedProduct);

				model.IsShipEnabled = contextProduct.IsShipEnabled;
				model.DisplayDeliveryTimeAccordingToStock = contextProduct.DisplayDeliveryTimeAccordingToStock(_catalogSettings);
				model.StockAvailablity = contextProduct.FormatStockMessage(_localizationService);

				model.DisplayBasePrice = _catalogSettings.ShowBasePriceInProductLists;
				model.CompareEnabled = _catalogSettings.CompareProductsEnabled;
				model.HideBuyButtonInLists = _catalogSettings.HideBuyButtonInLists;

				if (model.ShowDeliveryTimes)
				{
					var deliveryTime = _deliveryTimeService.GetDeliveryTime(contextProduct);
					if (deliveryTime != null)
					{
						model.DeliveryTimeName = deliveryTime.GetLocalized(x => x.Name);
						model.DeliveryTimeHexValue = deliveryTime.ColorHexValue;
					}
				}

				if (prepareManufacturers)
				{
					model.Manufacturers = PrepareManufacturersOverviewModel(_manufacturerService.GetProductManufacturersByProductId(product.Id), cachedManufacturerModels);
				}

				if (_catalogSettings.ShowBasePriceInProductLists)
				{
					model.BasePriceInfo = contextProduct.GetBasePriceInfo(_localizationService, _priceFormatter);
				}

				var addShippingPrice = _currencyService.ConvertCurrency(contextProduct.AdditionalShippingCharge, currentStore.PrimaryStoreCurrency, workingCurrency);

				if (addShippingPrice > 0 && displayPrices)
				{
					model.TransportSurcharge = res["Common.AdditionalShippingSurcharge"].Text.FormatWith(_priceFormatter.FormatPrice(addShippingPrice, true, false));
				}

				if (contextProduct.Weight > 0)
				{
					model.Weight = "{0} {1}".FormatCurrent(contextProduct.Weight.ToString("F2"), _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId).Name);
				}

				// IsNew
				if (_catalogSettings.LabelAsNewForMaxDays.HasValue)
				{
					model.IsNew = (DateTime.UtcNow - product.CreatedOnUtc).Days <= _catalogSettings.LabelAsNewForMaxDays.Value;
				}

				models.Add(model);
			}

			return models;
		}

		public IList<ProductSpecificationModel> PrepareProductSpecificationModel(Product product)
		{
			if (product == null)
				throw new ArgumentNullException("product");

			string cacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_SPECS_MODEL_KEY, product.Id, _services.WorkContext.WorkingLanguage.Id);
			return _services.Cache.Get(cacheKey, () =>
			{
				var model = _specificationAttributeService.GetProductSpecificationAttributesByProductId(product.Id, null, true)
				   .Select(psa =>
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

		public NavigationModel PrepareCategoryNavigationModel(int currentCategoryId, int currentProductId)
		{
			var root = GetCategoryMenu();
			var breadcrumb = GetCategoryBreadCrumb(currentCategoryId, currentProductId);

			// resolve number of products
			if (_catalogSettings.ShowCategoryProductNumber)
			{
				var curItem = breadcrumb.LastOrDefault();
				var curNode = curItem == null ? root.Root : root.Find(curItem);

				this.ResolveCategoryProductsCount(curNode);
			}

			var model = new NavigationModel
			{
				Root = root,
				Path = breadcrumb,
			};

			return model;
		}

		protected void ResolveCategoryProductsCount(TreeNode<MenuItem> curNode)
		{
			try
			{
				// Perf: only resolve counts for categories in the current path.
				while (curNode != null)
				{
					if (curNode.Children.Any(x => !x.Value.ElementsCount.HasValue))
					{
						lock (s_lock)
						{
							if (curNode.Children.Any(x => !x.Value.ElementsCount.HasValue))
							{
								foreach (var node in curNode.Children)
								{
									var categoryIds = new List<int>();

									if (_catalogSettings.ShowCategoryProductNumberIncludingSubcategories)
									{
										// include subcategories
										node.TraverseTree(x => categoryIds.Add(x.Value.EntityId));
									}
									else
									{
										categoryIds.Add(node.Value.EntityId);
									}

									var ctx = new ProductSearchContext();
									ctx.CategoryIds = categoryIds;
									ctx.StoreId = _services.StoreContext.CurrentStoreIdIfMultiStoreMode;
									node.Value.ElementsCount = _productService.CountProducts(ctx);
								}
							}
						}
					}

					curNode = curNode.Parent;
				}
			}
			catch (Exception exc)
			{
				Logger.Error(exc.Message, exc);
			}
		}

		public TreeNode<MenuItem> GetCategoryMenu()
		{
			var customerRolesIds = _services.WorkContext.CurrentCustomer.CustomerRoles.Where(cr => cr.Active).Select(cr => cr.Id).ToList();
			string cacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_NAVIGATION_MODEL_KEY,
				_services.WorkContext.WorkingLanguage.Id,
				string.Join(",", customerRolesIds),
				_services.StoreContext.CurrentStore.Id);

			var model = _services.Cache.Get(cacheKey, () =>
			{
				var curParent = new TreeNode<MenuItem>(new MenuItem
				{
					EntityId = 0,
					Text = "Home",
					RouteName = "HomePage"
				});

				Category prevCat = null;

				var categories = _categoryService.GetAllCategories();
				foreach (var category in categories)
				{
					var menuItem = new MenuItem
					{
						EntityId = category.Id,
						Text = category.GetLocalized(x => x.Name),
						RouteName = "Category"
					};
					menuItem.RouteValues.Add("SeName", category.GetSeName());

					// determine parent
					if (prevCat != null)
					{
						if (category.ParentCategoryId != curParent.Value.EntityId)
						{
							if (category.ParentCategoryId == prevCat.Id)
							{
								// level +1
								curParent = curParent.LastChild;
							}
							else
							{
								// level -x
								while (!curParent.IsRoot)
								{
									if (curParent.Value.EntityId == category.ParentCategoryId)
									{
										break;
									}
									curParent = curParent.Parent;
								}
							}
						}
					}

					// add to parent
					curParent.Append(menuItem);

					prevCat = category;
				}

				var root = curParent.Root;

				// menu publisher
				_menuPublisher.Value.RegisterMenus(root, "catalog");

				// event
				_services.EventPublisher.Publish(new NavigationModelBuiltEvent(root));

				return root;
			});

			return model;
		}

		public void PreparePagingFilteringModel(PagingFilteringModel model, PagingFilteringModel command, PageSizeContext pageSizeContext)
		{
			//sorting
			model.AllowProductSorting = _catalogSettings.AllowProductSorting;
			if (model.AllowProductSorting)
			{
				model.OrderBy = command.OrderBy;

				foreach (ProductSortingEnum enumValue in Enum.GetValues(typeof(ProductSortingEnum)))
				{
                    if (enumValue == ProductSortingEnum.CreatedOnAsc || enumValue == ProductSortingEnum.Initial)
					{
						// TODO: (MC) das von uns eingeführte "CreatedOnAsc" schmeiß ich
						// jetzt deshalb aus der UI raus, weil wir diese Sortier-Option
						// auch ins StoredProc (ProductsLoadAllpaged) reinpacken müssten.
						// Ist eigentlich kein Problem, ABER: Wir müssten immer wenn SmartStore
						// Änderungen an dieser Proc vornimmt und wir diese Änderungen
						// übernehmen müssen auch ständig an unseren Mod denken. Lass ma'!
						continue;
					}

					var currentPageUrl = _services.WebHelper.GetThisPageUrl(true);
					var sortUrl = _services.WebHelper.ModifyQueryString(currentPageUrl, "orderby=" + ((int)enumValue).ToString(), null);

					var sortValue = enumValue.GetLocalizedEnum(_localizationService, _services.WorkContext);
					model.AvailableSortOptions.Add(new ListOptionItem()
					{
						Text = sortValue,
						Url = sortUrl,
						Selected = enumValue == (ProductSortingEnum)command.OrderBy
					});
				}
			}

			//view mode
			model.AllowProductViewModeChanging = _catalogSettings.AllowProductViewModeChanging;
			var viewMode = !string.IsNullOrEmpty(command.ViewMode)
							? command.ViewMode
							: _catalogSettings.DefaultViewMode;

			model.ViewMode = viewMode;

			if (model.AllowProductViewModeChanging)
			{
				var currentPageUrl = _services.WebHelper.GetThisPageUrl(true);
				//grid
				model.AvailableViewModes.Add(new ListOptionItem()
				{
					Text = T("Categories.ViewMode.Grid"),
					Url = _services.WebHelper.ModifyQueryString(currentPageUrl, "viewmode=grid", null),
					Selected = viewMode == "grid",
					ExtraData = "grid"
				});
				//list
				model.AvailableViewModes.Add(new ListOptionItem()
				{
					Text = T("Categories.ViewMode.List"),
					Url = _services.WebHelper.ModifyQueryString(currentPageUrl, "viewmode=list", null),
					Selected = viewMode == "list",
					ExtraData = "list"
				});
			}

			//page size
			model.AllowCustomersToSelectPageSize = false;
			if (pageSizeContext.AllowCustomersToSelectPageSize && pageSizeContext.PageSizeOptions.IsEmpty())
			{
				pageSizeContext.PageSizeOptions = _catalogSettings.DefaultPageSizeOptions; // "12, 18, 36, 72, 150";

			}
			if (pageSizeContext.AllowCustomersToSelectPageSize && pageSizeContext.PageSizeOptions.HasValue())
			{
				var pageSizes = pageSizeContext.PageSizeOptions.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

				if (pageSizes.Any())
				{
					// get the first page size entry to use as the default (category page load) or if customer enters invalid value via query string
					if (command.PageSize <= 0 || !pageSizes.Contains(command.PageSize.ToString()))
					{
						int temp = 0;

						if (int.TryParse(pageSizes.FirstOrDefault(), out temp))
						{
							if (temp > 0)
							{
								command.PageSize = temp;
							}
						}
					}

					var currentPageUrl = _services.WebHelper.GetThisPageUrl(true);
					var sortUrl = _services.WebHelper.ModifyQueryString(currentPageUrl, "pagesize=__pagesize__", null);
					sortUrl = _services.WebHelper.RemoveQueryString(sortUrl, "pagenumber");

					foreach (var pageSize in pageSizes)
					{
						int temp = 0;
						if (!int.TryParse(pageSize, out temp) || temp <= 0)
						{
							continue;
						}

						model.PageSizeOptions.Add(new ListOptionItem()
						{
							Text = pageSize,
							Url = sortUrl.Replace("__pagesize__", pageSize),
							Selected = pageSize.Equals(command.PageSize.ToString(), StringComparison.InvariantCultureIgnoreCase)
						});
					}

					if (model.PageSizeOptions.Any())
					{
						model.PageSizeOptions = model.PageSizeOptions.OrderBy(x => int.Parse(x.Text)).ToList();
						model.AllowCustomersToSelectPageSize = true;

						if (command.PageSize <= 0)
						{
							command.PageSize = int.Parse(model.PageSizeOptions.FirstOrDefault().Text);
						}
					}
				}
			}
			else
			{
				//customer is not allowed to select a page size
				command.PageSize = pageSizeContext.PageSize;
			}

			if (command.PageSize <= 0)
				command.PageSize = pageSizeContext.PageSize;
		}

		public List<ManufacturerOverviewModel> PrepareManufacturersOverviewModel(
			ICollection<ProductManufacturer> manufacturers, 
			IDictionary<int, ManufacturerOverviewModel> cachedModels = null)
		{
			var model = new List<ManufacturerOverviewModel>();

			if (cachedModels == null)
			{
				cachedModels = new Dictionary<int, ManufacturerOverviewModel>();
			}

			foreach (var pm in manufacturers)
			{
				var manufacturer = pm.Manufacturer;
				ManufacturerOverviewModel item;

				if (!cachedModels.TryGetValue(manufacturer.Id, out item))
				{
					item = new ManufacturerOverviewModel
					{
						Id = manufacturer.Id,
						Name = manufacturer.Name,
						Description = manufacturer.Description,
						SeName = manufacturer.GetSeName()

					};

                    item.PictureModel = PrepareManufacturerPictureModel(manufacturer, manufacturer.GetLocalized(x => x.Name));

					cachedModels.Add(item.Id, item);
				}

				model.Add(item);
			}

			return model;
		}

        public PictureModel PrepareManufacturerPictureModel(Manufacturer manufacturer, string localizedName)
        {
            var model = new PictureModel();

            int pictureSize = _mediaSettings.ManufacturerThumbPictureSize;
            var manufacturerPictureCacheKey = string.Format(ModelCacheEventConsumer.MANUFACTURER_PICTURE_MODEL_KEY,
                manufacturer.Id,
                pictureSize,
                true,
                _services.WorkContext.WorkingLanguage.Id,
                _services.WebHelper.IsCurrentConnectionSecured(),
                _services.StoreContext.CurrentStore.Id);

            model = _services.Cache.Get(manufacturerPictureCacheKey, () =>
            {
                var pictureModel = new PictureModel
                {
                    PictureId = manufacturer.PictureId.GetValueOrDefault(),
                    //FullSizeImageUrl = _pictureService.GetPictureUrl(manufacturer.PictureId.GetValueOrDefault()),
                    ImageUrl = _pictureService.GetPictureUrl(manufacturer.PictureId.GetValueOrDefault(), pictureSize),
                    Title = string.Format(T("Media.Manufacturer.ImageLinkTitleFormat"), localizedName),
                    AlternateText = string.Format(T("Media.Manufacturer.ImageAlternateTextFormat"), localizedName)
                };
                return pictureModel;
            });

            return model;
        }

	}

	#region Nested Classes

	public class PageSizeContext
	{
		public bool AllowCustomersToSelectPageSize { get; set; }
		public string PageSizeOptions { get; set; }
		public int PageSize { get; set; }
	}

	#endregion
}