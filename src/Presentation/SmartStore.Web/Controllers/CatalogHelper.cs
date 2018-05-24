using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SmartStore.Collections;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Services;
using SmartStore.Services.Catalog;
using SmartStore.Services.Catalog.Extensions;
using SmartStore.Services.Catalog.Modelling;
using SmartStore.Services.Configuration;
using SmartStore.Services.Customers;
using SmartStore.Services.DataExchange.Export;
using SmartStore.Services.Directory;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Search;
using SmartStore.Services.Search.Modelling;
using SmartStore.Services.Security;
using SmartStore.Services.Seo;
using SmartStore.Services.Tax;
using SmartStore.Services.Topics;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.UI;
using SmartStore.Web.Infrastructure.Cache;
using SmartStore.Web.Models.Catalog;
using SmartStore.Web.Models.Media;

namespace SmartStore.Web.Controllers
{
	public partial class CatalogHelper
	{
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
		private readonly Lazy<ITopicService> _topicService;
		private readonly Lazy<IDataExporter> _dataExporter;
        private readonly Lazy<IPermissionService> _permissionService;
        private readonly ICatalogSearchService _catalogSearchService;
		private readonly ICatalogSearchQueryFactory _catalogSearchQueryFactory;
		private readonly ISiteMapService _siteMapService;
		private readonly HttpRequestBase _httpRequest;
		private readonly UrlHelper _urlHelper;
		private readonly ProductUrlHelper _productUrlHelper;

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
			Lazy<ITopicService> topicService,
			Lazy<IDataExporter> dataExporter,
            Lazy<IPermissionService> permissionService,
            ICatalogSearchService catalogSearchService,
			ICatalogSearchQueryFactory catalogSearchQueryFactory,
			ISiteMapService siteMapService,
			HttpRequestBase httpRequest,
			UrlHelper urlHelper,
			ProductUrlHelper productUrlHelper)
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
			this._topicService = topicService;
			this._dataExporter = dataExporter;
            this._permissionService = permissionService;
            this._catalogSearchService = catalogSearchService;
			this._catalogSearchQueryFactory = catalogSearchQueryFactory;
			this._siteMapService = siteMapService;
			this._httpRequest = httpRequest;
			this._urlHelper = urlHelper;
			this._productUrlHelper = productUrlHelper;

			T = NullLocalizer.Instance;
		}

		public Localizer T { get; set; }

		public ILogger Logger { get; set; }

		public ProductDetailsModel PrepareProductDetailsPageModel(
			Product product,
			ProductVariantQuery query,
			bool isAssociatedProduct = false,
			ProductBundleItemData productBundleItem = null, 
			IList<ProductBundleItemData> productBundleItems = null)
		{
			Guard.NotNull(product, nameof(product));

			var customer = _services.WorkContext.CurrentCustomer;
			var store = _services.StoreContext.CurrentStore;

			using (_services.Chronometer.Step("PrepareProductDetailsPageModel"))
			{
				var model = new ProductDetailsModel
				{
					Id = product.Id,
					Name = product.GetLocalized(x => x.Name),
					ShortDescription = product.GetLocalized(x => x.ShortDescription),
					FullDescription = product.GetLocalized(x => x.FullDescription, detectEmptyHtml: true),
					MetaKeywords = product.GetLocalized(x => x.MetaKeywords),
					MetaDescription = product.GetLocalized(x => x.MetaDescription),
					MetaTitle = product.GetLocalized(x => x.MetaTitle),
					SeName = product.GetSeName(),
					ProductType = product.ProductType,
					VisibleIndividually = product.VisibleIndividually,
					Manufacturers = _catalogSettings.ShowManufacturerInProductDetail 
						? PrepareManufacturersOverviewModel(_manufacturerService.GetProductManufacturersByProductId(product.Id), null, _catalogSettings.ShowManufacturerPicturesInProductDetail)
						: null,
					ReviewCount = product.ApprovedTotalReviews,
					DisplayAdminLink = _services.Permissions.Authorize(StandardPermissionProvider.AccessAdminPanel, customer),
					ShowSku = _catalogSettings.ShowProductSku,
					Sku = product.Sku,
					ShowManufacturerPartNumber = _catalogSettings.ShowManufacturerPartNumber,
                    DisplayProductReviews = _catalogSettings.ShowProductReviewsInProductDetail && product.AllowCustomerReviews,
                    ManufacturerPartNumber = product.ManufacturerPartNumber,
					ShowGtin = _catalogSettings.ShowGtin,
					Gtin = product.Gtin,
					StockAvailability = product.FormatStockMessage(_localizationService),
					HasSampleDownload = product.IsDownload && product.HasSampleDownload,
					IsCurrentCustomerRegistered = customer.IsRegistered(),
					IsAssociatedProduct = isAssociatedProduct,
					CompareEnabled = !isAssociatedProduct && _catalogSettings.CompareProductsEnabled,
					TellAFriendEnabled = !isAssociatedProduct && _catalogSettings.EmailAFriendEnabled,
					AskQuestionEnabled = !isAssociatedProduct && _catalogSettings.AskQuestionEnabled
				};

				// Social share code
				if (_catalogSettings.ShowShareButton && _catalogSettings.PageShareCode.HasValue())
				{
					var shareCode = _catalogSettings.PageShareCode;
					if (_services.WebHelper.IsCurrentConnectionSecured())
					{
						// Need to change the addthis link to be https linked when the page is, so that the page doesn't ask about mixed mode when viewed in https...
						shareCode = shareCode.Replace("http://", "https://");
					}

					model.ProductShareCode = shareCode;
				}

				// Get gift card values from query string.
				if (product.IsGiftCard)
				{
					model.GiftCard.RecipientName = query.GetGiftCardValue(product.Id, 0, "RecipientName");
					model.GiftCard.RecipientEmail = query.GetGiftCardValue(product.Id, 0, "RecipientEmail");
					model.GiftCard.SenderName = query.GetGiftCardValue(product.Id, 0, "SenderName");
					model.GiftCard.SenderEmail = query.GetGiftCardValue(product.Id, 0, "SenderEmail");
					model.GiftCard.Message = query.GetGiftCardValue(product.Id, 0, "Message");
				}

				// Back in stock subscriptions
				if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
					 product.BackorderMode == BackorderMode.NoBackorders &&
					 product.AllowBackInStockSubscriptions &&
					 product.StockQuantity <= 0)
				{
					//out of stock
					model.DisplayBackInStockSubscription = true;
					model.BackInStockAlreadySubscribed = _backInStockSubscriptionService.FindSubscription(customer.Id, product.Id, store.Id) != null;
				}

				// template
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

				if (product.ProductType == ProductType.GroupedProduct && !isAssociatedProduct)
				{
					// associated products
					var searchQuery = new CatalogSearchQuery()
						.VisibleOnly(customer)
						.HasStoreId(store.Id)
						.HasParentGroupedProduct(product.Id);

					var associatedProducts = _catalogSearchService.Search(searchQuery).Hits;

					foreach (var associatedProduct in associatedProducts)
					{
						var assciatedProductModel = PrepareProductDetailsPageModel(associatedProduct, query, true, null, null);
						model.AssociatedProducts.Add(assciatedProductModel);
					}
				}
				else if (product.ProductType == ProductType.BundledProduct && productBundleItem == null)
				{
					// bundled items
					bundleItems = _productService.GetBundleItems(product.Id);

					foreach (var itemData in bundleItems.Where(x => x.Item.Product.CanBeBundleItem()))
					{
						var item = itemData.Item;
						var bundledProductModel = PrepareProductDetailsPageModel(item.Product, query, false, itemData, null);

						bundledProductModel.ShowLegalInfo = false;
						bundledProductModel.DisplayDeliveryTime = false;

						bundledProductModel.BundleItem.Id = item.Id;
						bundledProductModel.BundleItem.Quantity = item.Quantity;
						bundledProductModel.BundleItem.HideThumbnail = item.HideThumbnail;
						bundledProductModel.BundleItem.Visible = item.Visible;
						bundledProductModel.BundleItem.IsBundleItemPricing = item.BundleProduct.BundlePerItemPricing;

						var bundleItemName = item.GetLocalized(x => x.Name);
						if (bundleItemName.Value.HasValue())
							bundledProductModel.Name = bundleItemName;

						var bundleItemShortDescription = item.GetLocalized(x => x.ShortDescription);
						if (bundleItemShortDescription.Value.HasValue())
							bundledProductModel.ShortDescription = bundleItemShortDescription;

						model.BundledItems.Add(bundledProductModel);
					}
				}

				model = PrepareProductDetailModel(model, product, query, isAssociatedProduct, productBundleItem, bundleItems);

				// Action items
				{
					if (model.HasSampleDownload)
					{
						model.ActionItems["sample"] = new ProductDetailsModel.ActionItemModel
						{
							Key = "sample",
							Title = T("Products.DownloadSample"),
							CssClass = "action-download-sample",
							IconCssClass = "fa fa-download",
							Href = _urlHelper.Action("Sample", "Download", new { productId = model.Id }),
							IsPrimary = true,
							PrimaryActionColor = "danger"
						};
					}

					if (!model.AddToCart.DisableWishlistButton && model.ProductType != ProductType.GroupedProduct)
					{
						model.ActionItems["wishlist"] = new ProductDetailsModel.ActionItemModel
						{
							Key = "wishlist",
							Title = T("ShoppingCart.AddToWishlist.Short"),
							Tooltip = T("ShoppingCart.AddToWishlist"),
							CssClass = "ajax-cart-link action-add-to-wishlist",
							IconCssClass = "icm icm-heart",
							Href = _urlHelper.Action("AddProduct", "ShoppingCart", new { productId = model.Id, shoppingCartTypeId = (int)ShoppingCartType.Wishlist })
						};
					}

					if (model.CompareEnabled)
					{
						model.ActionItems["compare"] = new ProductDetailsModel.ActionItemModel
						{
							Key = "compare",
							Title = T("Common.Shopbar.Compare"),
							Tooltip = T("Products.Compare.AddToCompareList"),
							CssClass = "action-compare ajax-cart-link",
							IconCssClass = "icm icm-repeat",
							Href = _urlHelper.Action("AddProductToCompare", "Catalog", new { id = model.Id })
						};
					}

					if (model.AskQuestionEnabled && !model.ProductPrice.CallForPrice)
					{
						model.ActionItems["ask"] = new ProductDetailsModel.ActionItemModel
						{
							Key = "ask",
							Title = T("Products.AskQuestion.Short"),
							Tooltip = T("Products.AskQuestion"),
							CssClass = "action-ask-question",
							IconCssClass = "icm icm-envelope",
							Href = _urlHelper.Action("AskQuestion", new { id = model.Id })
						};
					}

					if (model.TellAFriendEnabled)
					{
						model.ActionItems["tell"] = new ProductDetailsModel.ActionItemModel
						{
							Key = "tell",
							Title = T("Products.EmailAFriend"),
							CssClass = "action-bullhorn",
							IconCssClass = "icm icm-bullhorn",
							Href = _urlHelper.Action("EmailAFriend", new { id = model.Id })
						};
					}
				}

				IList<int> combinationPictureIds = null;

				if (productBundleItem == null)
				{
					combinationPictureIds = _productAttributeService.GetAllProductVariantAttributeCombinationPictureIds(product.Id);
					if (combination == null && model.SelectedCombination != null)
						combination = model.SelectedCombination;
				}

				// pictures
				var pictures = _pictureService.GetPicturesByProductId(product.Id);
				PrepareProductDetailsPictureModel(model.DetailsPictureModel, pictures, model.Name, combinationPictureIds, isAssociatedProduct, productBundleItem, combination);

				return model;
			}
		}

		public void PrepareProductReviewsModel(ProductReviewsModel model, Product product, int take = int.MaxValue)
		{
			Guard.NotNull(product, nameof(product));
			Guard.NotNull(model, nameof(model));

			model.ProductId = product.Id;
			model.ProductName = product.GetLocalized(x => x.Name);
			model.ProductSeName = product.GetSeName();

			var query = _services.DbContext
				.QueryForCollection<Product, ProductReview>(product, x => x.ProductReviews)
				.Where(x => x.IsApproved);

			if (DataSettings.Current.IsSqlServer)
			{
				// SQCE throw NotImplementedException with .Expand()
				query = query
					.Expand(x => x.Customer.CustomerRoles)
					.Expand(x => x.Customer.CustomerContent);
			}

			int totalCount = query.Count();
			model.TotalReviewsCount = totalCount;

			var reviews = query
				.OrderByDescending(x => x.CreatedOnUtc)
				.Take(take)
				.ToList();

			foreach (var review in reviews)
			{
				model.Items.Add(new ProductReviewModel
				{
					Id = review.Id,
					CustomerId = review.CustomerId,
					CustomerName = review.Customer.FormatUserName(),
					AllowViewingProfiles = _customerSettings.AllowViewingProfiles && review.Customer != null && !review.Customer.IsGuest(),
					Title = review.Title,
					ReviewText = review.ReviewText,
					Rating = review.Rating,
					Helpfulness = new ProductReviewHelpfulnessModel
					{
						ProductReviewId = review.Id,
						HelpfulYesTotal = review.HelpfulYesTotal,
						HelpfulNoTotal = review.HelpfulNoTotal,
					},
					WrittenOnStr = _dateTimeHelper.ConvertToUserTime(review.CreatedOnUtc, DateTimeKind.Utc).ToString("D"),
				});
			}

			model.CanCurrentCustomerLeaveReview = _catalogSettings.AllowAnonymousUsersToReviewProduct || !_services.WorkContext.CurrentCustomer.IsGuest();
			model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnProductReviewPage;
		}

		private PictureModel CreatePictureModel(ProductDetailsPictureModel model, Picture picture, int pictureSize)
		{
			var info = _pictureService.GetPictureInfo(picture);

			var result = new PictureModel
			{
				PictureId = info?.Id ?? 0,
				Size = pictureSize,
				ThumbImageUrl = _pictureService.GetUrl(info, _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage),
				ImageUrl = _pictureService.GetUrl(info, pictureSize, !_catalogSettings.HideProductDefaultPictures),
				FullSizeImageUrl = _pictureService.GetUrl(info, 0, false),
				FullSizeImageWidth = info?.Width,
				FullSizeImageHeight = info?.Height,
				Title = model.Name,
				AlternateText = model.AlternateText
			};

			return result;
		}

        public IList<ProductDetailsModel.TierPriceModel> CreateTierPriceModel (Product product, decimal adjustment = decimal.Zero)
        {
            var model = product.TierPrices
                .OrderBy(x => x.Quantity)
                .FilterByStore(_services.StoreContext.CurrentStore.Id)
                .FilterForCustomer(_services.WorkContext.CurrentCustomer)
                .ToList()
                .RemoveDuplicatedQuantities()
                .Select(tierPrice =>
                {
                    var currentAdjustment = adjustment;
                    var m = new ProductDetailsModel.TierPriceModel
                    {
                        Quantity = tierPrice.Quantity,
                    };

                    if (currentAdjustment != 0 && tierPrice.CalculationMethod == TierPriceCalculationMethod.Percental && _catalogSettings.ApplyTierPricePercentageToAttributePriceAdjustments)
                    {
                        currentAdjustment = currentAdjustment - (currentAdjustment / 100 * tierPrice.Price);
                    }
                    else
                    {
                        currentAdjustment = decimal.Zero;
                    }

                    decimal taxRate = decimal.Zero;
                    decimal priceBase = _taxService.GetProductPrice(product, _priceCalculationService.GetFinalPrice(product, _services.WorkContext.CurrentCustomer, currentAdjustment, _catalogSettings.DisplayTierPricesWithDiscounts, tierPrice.Quantity, null, null, true), out taxRate);
                    decimal price = _currencyService.ConvertFromPrimaryStoreCurrency(priceBase, _services.WorkContext.WorkingCurrency);
                    m.Price = _priceFormatter.FormatPrice(price, true, false);
                    return m;
                })
                .ToList();

            return model;
        }

		public void PrepareProductDetailsPictureModel(
			ProductDetailsPictureModel model, 
			IList<Picture> pictures, 
			string name, 
			IList<int> allCombinationImageIds,
			bool isAssociatedProduct, 
			ProductBundleItemData bundleItem = null, 
			ProductVariantAttributeCombination combination = null)
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

			using (var scope = new DbContextScope(_services.DbContext, autoCommit: false))
			{
				// Scope this part: it's quite possible that IPictureService.UpdatePicture()
				// is called when a picture is new or its size is missing in DB.

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
						allCombinationImageIds = allCombinationImageIds ?? new List<int>();
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

				scope.Commit();
			}

			if (defaultPicture == null)
			{
				model.DefaultPictureModel = new PictureModel
				{
					Title = T("Media.Product.ImageLinkTitleFormat", model.Name),
					AlternateText = model.AlternateText
				};

				if (!_catalogSettings.HideProductDefaultPictures)
				{
					model.DefaultPictureModel.Size = defaultPictureSize;
					model.DefaultPictureModel.ThumbImageUrl = _pictureService.GetFallbackUrl(_mediaSettings.ProductThumbPictureSizeOnProductDetailsPage);
					model.DefaultPictureModel.ImageUrl = _pictureService.GetFallbackUrl(defaultPictureSize);
					model.DefaultPictureModel.FullSizeImageUrl = _pictureService.GetFallbackUrl();
                }
			}
			else
			{
				model.DefaultPictureModel = CreatePictureModel(model, defaultPicture, defaultPictureSize);
			}
		}

		public ProductDetailsModel PrepareProductDetailModel(
			ProductDetailsModel model,
			Product product,
			ProductVariantQuery query,
			bool isAssociatedProduct = false,
			ProductBundleItemData productBundleItem = null,
			IList<ProductBundleItemData> productBundleItems = null,
			int selectedQuantity = 1)
		{
			if (product == null)
				throw new ArgumentNullException("product");

			if (model == null)
				throw new ArgumentNullException("model");

			var store = _services.StoreContext.CurrentStore;
			var customer = _services.WorkContext.CurrentCustomer;
			var currency = _services.WorkContext.WorkingCurrency;

			decimal preSelectedPriceAdjustmentBase = decimal.Zero;
			decimal preSelectedWeightAdjustment = decimal.Zero;
			bool displayPrices = _services.Permissions.Authorize(StandardPermissionProvider.DisplayPrices);
			bool isBundle = (product.ProductType == ProductType.BundledProduct);
			bool isBundleItemPricing = (productBundleItem != null && productBundleItem.Item.BundleProduct.BundlePerItemPricing);
			bool isBundlePricing = (productBundleItem != null && !productBundleItem.Item.BundleProduct.BundlePerItemPricing);
			int bundleItemId = (productBundleItem == null ? 0 : productBundleItem.Item.Id);

			bool hasSelectedAttributesValues = false;
			bool hasSelectedAttributes = query.Variants.Count > 0;
			List<ProductVariantAttributeValue> selectedAttributeValues = null;

			var variantAttributes = (isBundle ? new List<ProductVariantAttribute>() : _productAttributeService.GetProductVariantAttributesByProductId(product.Id));

			model.IsBundlePart = product.ProductType != ProductType.BundledProduct && productBundleItem != null;
			model.ProductPrice.DynamicPriceUpdate = _catalogSettings.EnableDynamicPriceUpdate;
			model.ProductPrice.BundleItemShowBasePrice = _catalogSettings.BundleItemShowBasePrice;

			if (!model.ProductPrice.DynamicPriceUpdate)
				selectedQuantity = 1;

			#region Product attributes

			// Bundles doesn't have attributes.
			if (!isBundle)
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

					if (hasSelectedAttributes)
					{
						var selectedVariant = query.Variants.FirstOrDefault(x =>
							x.ProductId == product.Id &&
							x.BundleItemId == bundleItemId &&
							x.AttributeId == attribute.ProductAttributeId &&
							x.VariantAttributeId == attribute.Id);

						if (selectedVariant != null)
						{
							switch (attribute.AttributeControlType)
							{
								case AttributeControlType.Datepicker:
									if (selectedVariant.Date.HasValue)
									{
										pvaModel.SelectedDay = selectedVariant.Date.Value.Day;
										pvaModel.SelectedMonth = selectedVariant.Date.Value.Month;
										pvaModel.SelectedYear = selectedVariant.Date.Value.Year;
									}
									break;
								case AttributeControlType.FileUpload:
									pvaModel.UploadedFileGuid = selectedVariant.Value;

									Guid guid;
									if (selectedVariant.Value.HasValue() && Guid.TryParse(selectedVariant.Value, out guid))
									{										
										var download = _downloadService.GetDownloadByGuid(guid);
										if (download != null)
										{
											pvaModel.UploadedFileName = string.Concat(download.Filename ?? download.DownloadGuid.ToString(), download.Extension);
										}
									}
									break;
								case AttributeControlType.TextBox:
								case AttributeControlType.MultilineTextbox:
									pvaModel.TextValue = selectedVariant.Value;
									break;
							}
						}
					}

					// TODO: obsolete? Alias field is not used for custom values anymore, only for URL as URL variant alias.
					if (attribute.AttributeControlType == AttributeControlType.Datepicker && pvaModel.Alias.HasValue() && RegularExpressions.IsYearRange.IsMatch(pvaModel.Alias))
					{
						var match = RegularExpressions.IsYearRange.Match(pvaModel.Alias);
						pvaModel.BeginYear = match.Groups[1].Value.ToInt();
						pvaModel.EndYear = match.Groups[2].Value.ToInt();
					}

					var preSelectedValueId = 0;
					var pvaValues = attribute.ShouldHaveValues() 
						? _productAttributeService.GetProductVariantAttributeValues(attribute.Id)
						: new List<ProductVariantAttributeValue>();

					foreach (var pvaValue in pvaValues)
					{
						ProductBundleItemAttributeFilter attributeFilter = null;

						if (productBundleItem.FilterOut(pvaValue, out attributeFilter))
							continue;

						if (preSelectedValueId == 0 && attributeFilter != null && attributeFilter.IsPreSelected)
							preSelectedValueId = attributeFilter.AttributeValueId;

						var linkedProduct = _productService.GetProductById(pvaValue.LinkedProductId);

						var pvaValueModel = new ProductDetailsModel.ProductVariantAttributeValueModel();
						pvaValueModel.PriceAdjustment = string.Empty;
						pvaValueModel.Id = pvaValue.Id;
						pvaValueModel.Name = pvaValue.GetLocalized(x => x.Name);
						pvaValueModel.Alias = pvaValue.Alias;
						pvaValueModel.Color = pvaValue.Color; // used with "Boxes" attribute type
						pvaValueModel.IsPreSelected = pvaValue.IsPreSelected;

						if (linkedProduct != null && linkedProduct.VisibleIndividually)
							pvaValueModel.SeName = linkedProduct.GetSeName();

						// Explicitly selected always discards pre-selected by merchant.
						if (hasSelectedAttributes)
							pvaValueModel.IsPreSelected = false;

						// Display price if allowed.
						if (displayPrices && !isBundlePricing)
						{
							decimal taxRate = decimal.Zero;
							decimal attributeValuePriceAdjustment = _priceCalculationService.GetProductVariantAttributeValuePriceAdjustment(pvaValue, 
                                product, _services.WorkContext.CurrentCustomer, null, selectedQuantity);
							decimal priceAdjustmentBase = _taxService.GetProductPrice(product, attributeValuePriceAdjustment, out taxRate);
							decimal priceAdjustment = _currencyService.ConvertFromPrimaryStoreCurrency(priceAdjustmentBase, currency);

							if (_catalogSettings.ShowVariantCombinationPriceAdjustment)
							{
								if (priceAdjustmentBase > decimal.Zero)
									pvaValueModel.PriceAdjustment = "+" + _priceFormatter.FormatPrice(priceAdjustment, true, false);
								else if (priceAdjustmentBase < decimal.Zero)
									pvaValueModel.PriceAdjustment = "-" + _priceFormatter.FormatPrice(-priceAdjustment, true, false);
							}

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

						if (_catalogSettings.ShowLinkedAttributeValueImage && pvaValue.ValueType == ProductVariantAttributeValueType.ProductLinkage)
						{
							var linkagePicture = _pictureService.GetPicturesByProductId(pvaValue.LinkedProductId, 1).FirstOrDefault();
							if (linkagePicture != null)
								pvaValueModel.ImageUrl = _pictureService.GetUrl(linkagePicture, _mediaSettings.VariantValueThumbPictureSize, false);
						}
                        else if (pvaValue.PictureId != 0)
                        {
                            pvaValueModel.ImageUrl = _pictureService.GetUrl(pvaValue.PictureId, _mediaSettings.VariantValueThumbPictureSize, false);
                        }

						pvaModel.Values.Add(pvaValueModel);
					}

					// We need selected attributes to get initially displayed combination images.
					if (!hasSelectedAttributes)
					{
						ProductDetailsModel.ProductVariantAttributeValueModel defaultValue = null;

						// Value pre-selected by a bundle item filter discards the default pre-selection.
						if (preSelectedValueId != 0)
						{
							pvaModel.Values.Each(x => x.IsPreSelected = false);

							if ((defaultValue = pvaModel.Values.OfType<ProductDetailsModel.ProductVariantAttributeValueModel>().FirstOrDefault(v => v.Id == preSelectedValueId)) != null)
							{
								defaultValue.IsPreSelected = true;
								query.AddVariant(new ProductVariantQueryItem(defaultValue.Id.ToString())
								{
									ProductId = product.Id,
									BundleItemId = bundleItemId,
									AttributeId = attribute.ProductAttributeId,
									VariantAttributeId = attribute.Id,
									Alias = attribute.ProductAttribute.Alias,
									ValueAlias = defaultValue.Alias
								});
							}
						}

						if (defaultValue == null)
						{
							foreach (var value in pvaModel.Values.Where(x => x.IsPreSelected))
							{
								query.AddVariant(new ProductVariantQueryItem(value.Id.ToString())
								{
									ProductId = product.Id,
									BundleItemId = bundleItemId,
									AttributeId = attribute.ProductAttributeId,
									VariantAttributeId = attribute.Id,
									Alias = attribute.ProductAttribute.Alias,
									ValueAlias = value.Alias
								});
							}
						}
					}

					model.ProductVariantAttributes.Add(pvaModel);
				}
			}

			#endregion

			#region Attribute combinations

			if (!isBundle)
			{
				if (query.Variants.Count > 0)
				{
					// Merge with combination data if there's a match.
					var warnings = new List<string>();
					var attributeXml = query.CreateSelectedAttributesXml(product.Id, bundleItemId, variantAttributes, _productAttributeParser, _localizationService,
						_downloadService, _catalogSettings, _httpRequest, warnings);

					selectedAttributeValues = _productAttributeParser.ParseProductVariantAttributeValues(attributeXml).ToList();
					hasSelectedAttributesValues = (selectedAttributeValues.Count > 0);

					if (isBundlePricing)
					{
						model.AttributeInfo = _productAttributeFormatter.FormatAttributes(
							product, 
							attributeXml, 
							customer,
							serapator: ", ",
							renderPrices: false, 
							renderGiftCardAttributes: false, 
							allowHyperlinks: false);
					}

					model.SelectedCombination = _productAttributeParser.FindProductVariantAttributeCombination(product.Id, attributeXml);

					if (model.SelectedCombination != null && model.SelectedCombination.IsActive == false)
					{
						model.IsAvailable = false;
                        model.StockAvailability = T("Products.Availability.IsNotActive");
					}

					product.MergeWithCombination(model.SelectedCombination);

					// Mark explicitly selected as pre-selected.
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
				// Cases where stock inventory is not functional. Determined by what ShoppingCartService.GetStandardWarnings and ProductService.AdjustInventory is not handling.
				model.IsAvailable = true;
				var hasAttributeCombinations = _services.DbContext.QueryForCollection(product, (Product p) => p.ProductVariantAttributeCombinations).Any();
                model.StockAvailability = !hasAttributeCombinations ? product.FormatStockMessage(_localizationService) : "";
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
			model.FullDescription = product.GetLocalized(x => x.FullDescription, detectEmptyHtml: true);
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
			model.IsCurrentCustomerRegistered = customer.IsRegistered();
			model.IsBasePriceEnabled = product.BasePriceEnabled && !(isBundle && product.BundlePerItemPricing);
            model.BasePriceInfo = product.GetBasePriceInfo(_localizationService, _priceFormatter, _currencyService, _taxService, _priceCalculationService, customer, currency);
			model.ShowLegalInfo = !model.IsBundlePart && _taxSettings.ShowLegalHintsInProductDetails;
			model.BundleTitleText = product.GetLocalized(x => x.BundleTitleText);
			model.BundlePerItemPricing = product.BundlePerItemPricing;
			model.BundlePerItemShipping = product.BundlePerItemShipping;
			model.BundlePerItemShoppingCart = product.BundlePerItemShoppingCart;

			//_taxSettings.TaxDisplayType == TaxDisplayType.ExcludingTax;

			var taxDisplayType = _services.WorkContext.GetTaxDisplayTypeFor(customer, store.Id);
			string taxInfo = T(taxDisplayType == TaxDisplayType.IncludingTax ? "Tax.InclVAT" : "Tax.ExclVAT");

			var defaultTaxRate = string.Empty;
			if (_taxSettings.DisplayTaxRates)
			{
				var taxRate = _taxService.GetTaxRate(product, customer);
				if (taxRate != decimal.Zero)
				{
					var formattedTaxRate = _priceFormatter.FormatTaxRate(taxRate);
					defaultTaxRate = $"({formattedTaxRate}%)";
				}
			}

			var additionalShippingCosts = string.Empty;
			var addShippingPrice = _currencyService.ConvertFromPrimaryStoreCurrency(product.AdditionalShippingCharge, currency);

			if (addShippingPrice > 0)
			{
				additionalShippingCosts = T("Common.AdditionalShippingSurcharge").Text.FormatInvariant(_priceFormatter.FormatPrice(addShippingPrice, true, false)) + ", ";
			}

            if (!product.IsShipEnabled || (addShippingPrice == 0 && product.IsFreeShipping))
            {
                model.LegalInfo += "{0} {1}, {2}".FormatInvariant(
                    product.IsTaxExempt ? "" : taxInfo,
                    product.IsTaxExempt ? "" : defaultTaxRate,
                    T("Common.FreeShipping"));
            }
            else
            {
				var shippingInfoUrl = _urlHelper.TopicUrl("ShippingInfo");

				if (shippingInfoUrl.IsEmpty())
				{
					model.LegalInfo = T("Tax.LegalInfoProductDetail2",
						product.IsTaxExempt ? "" : taxInfo,
						product.IsTaxExempt ? "" : defaultTaxRate,
						additionalShippingCosts);
				}
				else
				{
					model.LegalInfo = T("Tax.LegalInfoProductDetail",
						product.IsTaxExempt ? "" : taxInfo,
						product.IsTaxExempt ? "" : defaultTaxRate,
						additionalShippingCosts,
						shippingInfoUrl);
				}
            }

			var dimension = _measureService.GetMeasureDimensionById(_measureSettings.BaseDimensionId)?.SystemKeyword ?? string.Empty;

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

			model.Weight = (model.WeightValue > 0) ? "{0} {1}".FormatCurrent(model.WeightValue.ToString("N2"), _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId).SystemKeyword) : "";
			model.Height = (product.Height > 0) ? "{0} {1}".FormatCurrent(product.Height.ToString("N2"), dimension) : "";
			model.Length = (product.Length > 0) ? "{0} {1}".FormatCurrent(product.Length.ToString("N2"), dimension) : "";
			model.Width = (product.Width > 0) ? "{0} {1}".FormatCurrent(product.Width.ToString("N2"), dimension) : "";

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
				model.BackInStockAlreadySubscribed = _backInStockSubscriptionService.FindSubscription(customer.Id, product.Id, store.Id) != null;
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
                        decimal attributesTotalPriceBaseOrig = decimal.Zero;
                        decimal finalPriceWithoutDiscount = decimal.Zero;
						decimal finalPriceWithDiscount = decimal.Zero;

						decimal oldPriceBase = _taxService.GetProductPrice(product, product.OldPrice, out taxRate);

						if (model.ProductPrice.DynamicPriceUpdate && !isBundlePricing)
						{
							if (selectedAttributeValues != null)
							{
								selectedAttributeValues.Each(x => attributesTotalPriceBase += _priceCalculationService.GetProductVariantAttributeValuePriceAdjustment(x, 
                                    product, _services.WorkContext.CurrentCustomer, null, selectedQuantity));

                                selectedAttributeValues.Each(x => attributesTotalPriceBaseOrig += _priceCalculationService.GetProductVariantAttributeValuePriceAdjustment(x,
                                    product, _services.WorkContext.CurrentCustomer, null, 1));
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
							customer, attributesTotalPriceBaseOrig, false, selectedQuantity, productBundleItem);
                        
						finalPriceWithDiscountBase = _priceCalculationService.GetFinalPrice(product, productBundleItems,
							customer, attributesTotalPriceBase, true, selectedQuantity, productBundleItem);

						finalPriceWithoutDiscountBase = _taxService.GetProductPrice(product, finalPriceWithoutDiscountBase, out taxRate);
						finalPriceWithDiscountBase = _taxService.GetProductPrice(product, finalPriceWithDiscountBase, out taxRate);

						oldPrice = _currencyService.ConvertFromPrimaryStoreCurrency(oldPriceBase, currency);

						finalPriceWithoutDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithoutDiscountBase, currency);
						finalPriceWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithDiscountBase, currency);

						if (productBundleItem == null || isBundleItemPricing)
						{
							if (oldPriceBase > decimal.Zero && oldPriceBase > finalPriceWithoutDiscountBase)
							{
								model.ProductPrice.OldPriceValue = oldPrice;
								model.ProductPrice.OldPrice = _priceFormatter.FormatPrice(oldPrice);
							}		

							model.ProductPrice.Price = _priceFormatter.FormatPrice(finalPriceWithoutDiscount);

							if (finalPriceWithoutDiscountBase != finalPriceWithDiscountBase)
							{
								model.ProductPrice.PriceWithDiscount = _priceFormatter.FormatPrice(finalPriceWithDiscount);
							}	
						}

						model.ProductPrice.PriceValue = finalPriceWithoutDiscount;
						model.ProductPrice.PriceWithDiscountValue = finalPriceWithDiscount;
                        model.BasePriceInfo = product.GetBasePriceInfo(
                            _localizationService, 
                            _priceFormatter, 
                            _currencyService, 
                            _taxService, 
                            _priceCalculationService,
							customer,
                            currency, 
                            attributesTotalPriceBase);

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

                            model.BasePriceInfo = product.GetBasePriceInfo(
                                _localizationService, 
                                _priceFormatter, 
                                _currencyService,
                                _taxService, 
                                _priceCalculationService,
								customer,
                                currency, 
                                (product.Price - finalPriceWithDiscount) * (-1));
						}

						// Calculate saving.
						// Discounted price has priority over the old price (avoids differing percentage discount in product lists and detail page).
						//var regularPrice = Math.Max(finalPriceWithoutDiscount, oldPrice);
						var regularPrice = finalPriceWithDiscount < finalPriceWithoutDiscount
							? finalPriceWithoutDiscount
							: oldPrice;

						if (regularPrice > 0 && regularPrice > finalPriceWithDiscount)
						{
							model.ProductPrice.SavingPercent = (float)((regularPrice - finalPriceWithDiscount) / regularPrice) * 100;
							model.ProductPrice.SavingAmount = _priceFormatter.FormatPrice(regularPrice - finalPriceWithDiscount, true, false);
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

            model.AddToCart.MinOrderAmount = product.OrderMinimumQuantity;
            model.AddToCart.MaxOrderAmount = product.OrderMaximumQuantity;
			model.AddToCart.QuantityUnitName = model.QuantityUnitName; // TODO: (mc) remove 'QuantityUnitName' from parent model later
            model.AddToCart.QuantityStep = product.QuantityStep > 0 ? product.QuantityStep : 1;
            model.AddToCart.HideQuantityControl = product.HideQuantityControl;
            model.AddToCart.QuantiyControlType = product.QuantiyControlType;

            //'add to cart', 'add to wishlist' buttons
            model.AddToCart.DisableBuyButton = !displayPrices || product.DisableBuyButton || !_services.Permissions.Authorize(StandardPermissionProvider.EnableShoppingCart);
			model.AddToCart.DisableWishlistButton = !displayPrices || product.DisableWishlistButton 
				|| !_services.Permissions.Authorize(StandardPermissionProvider.EnableWishlist)
				|| product.ProductType == ProductType.GroupedProduct;

			//pre-order
			model.AddToCart.AvailableForPreOrder = product.AvailableForPreOrder;

			//customer entered price
			model.AddToCart.CustomerEntersPrice = product.CustomerEntersPrice;
			if (model.AddToCart.CustomerEntersPrice)
			{
				var minimumCustomerEnteredPrice = _currencyService.ConvertFromPrimaryStoreCurrency(product.MinimumCustomerEnteredPrice, currency);
				var maximumCustomerEnteredPrice = _currencyService.ConvertFromPrimaryStoreCurrency(product.MaximumCustomerEnteredPrice, currency);

				model.AddToCart.CustomerEnteredPrice = minimumCustomerEnteredPrice;

				model.AddToCart.CustomerEnteredPriceRange = string.Format(T("Products.EnterProductPrice.Range"),
					_priceFormatter.FormatPrice(minimumCustomerEnteredPrice, true, false),
					_priceFormatter.FormatPrice(maximumCustomerEnteredPrice, true, false));
			}

			//allowed quantities
			var allowedQuantities = product.ParseAllowedQuatities();
			foreach (var qty in allowedQuantities)
			{
				model.AddToCart.AllowedQuantities.Add(new SelectListItem
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
				model.GiftCard.SenderName = customer.GetFullName();
				model.GiftCard.SenderEmail = customer.Email;
			}

			#endregion

			_services.DisplayControl.Announce(product);

			return model;
		}

		public IEnumerable<int> GetChildCategoryIds(int parentCategoryId, bool deep = true)
		{
			var root = GetCategoryMenu();
			var node = root.SelectNodeById(parentCategoryId) ?? root.SelectNode(x => x.Value.EntityId == parentCategoryId);
			if (node != null)
			{
				var children = deep ? node.Flatten(false) : node.Children.Select(x => x.Value);
				var ids = children.Select(x => x.EntityId);
				return ids;
			}

			return Enumerable.Empty<int>();
		}

		public IList<TreeNode<MenuItem>> GetCategoryBreadCrumb(int currentCategoryId, int currentProductId)
		{
			var requestCache = EngineContext.Current.Resolve<IRequestCache>();
			string cacheKey = "sm.temp.category.path.{0}-{1}".FormatInvariant(currentCategoryId, currentProductId);

			var breadcrumb = requestCache.Get(cacheKey, () =>
			{
				var root = GetCategoryMenu();
				TreeNode<MenuItem> node = null;

				if (currentCategoryId > 0)
				{
					node = root.SelectNodeById(currentCategoryId) ?? root.SelectNode(x => x.Value.EntityId == currentCategoryId);
				}

				if (node == null && currentProductId > 0)
				{
					var productCategories = _categoryService.GetProductCategoriesByProductId(currentProductId);
					if (productCategories.Count > 0)
					{
						currentCategoryId = productCategories[0].Category.Id;
						node = root.SelectNodeById(currentCategoryId) ?? root.SelectNode(x => x.Value.EntityId == currentCategoryId);
					}
				}

				if (node != null)
				{
					var path = node.GetBreadcrumb().ToList();
					return path;
				}

				return new List<TreeNode<MenuItem>>();
			});

			return breadcrumb;
		}

		public IList<ProductSpecificationModel> PrepareProductSpecificationModel(Product product)
		{
			Guard.NotNull(product, nameof(product));
			
			if (_services.Cache.IsDistributedCache)
			{
				// How bad we cannot cache LocalizedValue in distributed caches
				return Execute();
			}
			else
			{
				string cacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_SPECS_MODEL_KEY, product.Id, _services.WorkContext.WorkingLanguage.Id);
				return _services.Cache.Get(cacheKey, () =>
				{
					return Execute();
				});
			}

			List<ProductSpecificationModel> Execute()
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
			}
		}

		public NavigationModel PrepareCategoryNavigationModel(int currentCategoryId, int currentProductId)
		{
			var root = GetCategoryMenu();
		
			var breadcrumb = GetCategoryBreadCrumb(currentCategoryId, currentProductId);

			var model = new NavigationModel
			{
				Root = root,
				Path = breadcrumb
			};

			// Resolve number of products
			if (_catalogSettings.ShowCategoryProductNumber)
			{
				_siteMapService.ResolveElementCounts("catalog", model.SelectedNode, false);
			}

			return model;
		}

		public TreeNode<MenuItem> GetCategoryMenu()
		{
			return _siteMapService.GetRootNode("catalog");
		}

		public List<ManufacturerOverviewModel> PrepareManufacturersOverviewModel(
			ICollection<ProductManufacturer> manufacturers, 
			IDictionary<int, ManufacturerOverviewModel> cachedModels = null,
			bool withPicture = false)
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
						Name = manufacturer.GetLocalized(x => x.Name),
						Description = manufacturer.GetLocalized(x => x.Description, true),
						SeName = manufacturer.GetSeName()

					};

					if (withPicture)
					{
						item.Picture = PrepareManufacturerPictureModel(manufacturer, manufacturer.GetLocalized(x => x.Name));
					}

					cachedModels.Add(item.Id, item);
				}

				model.Add(item);
			}

			return model;
		}

        public PictureModel PrepareManufacturerPictureModel(Manufacturer manufacturer, string localizedName)
        {
            var model = new PictureModel();
			var pictureSize = _mediaSettings.ManufacturerThumbPictureSize;
			var pictureInfo = _pictureService.GetPictureInfo(manufacturer.PictureId);

			model = new PictureModel
			{
				PictureId = manufacturer.PictureId.GetValueOrDefault(),
				Size = pictureSize,
				//FullSizeImageUrl = _pictureService.GetPictureUrl(manufacturer.PictureId.GetValueOrDefault()),
				ImageUrl = _pictureService.GetUrl(pictureInfo, pictureSize, !_catalogSettings.HideManufacturerDefaultPictures),
				Title = string.Format(T("Media.Manufacturer.ImageLinkTitleFormat"), localizedName),
				AlternateText = string.Format(T("Media.Manufacturer.ImageAlternateTextFormat"), localizedName)
			};

            return model;
        }

        public ManufacturerNavigationModel PrepareManufacturerNavigationModel(int manufacturerItemsToDisplay)
        {
			var cacheKey = string.Format(ModelCacheEventConsumer.MANUFACTURER_NAVIGATION_MODEL_KEY,
                !_catalogSettings.HideManufacturerDefaultPictures,
                _services.WorkContext.WorkingLanguage.Id,
                _services.StoreContext.CurrentStore.Id,
                manufacturerItemsToDisplay);

            var cacheModel = _services.Cache.Get(cacheKey, () =>
            {
                var manufacturers = _manufacturerService.GetAllManufacturers(null, 0, manufacturerItemsToDisplay + 1, _services.StoreContext.CurrentStore.Id);

                var model = new ManufacturerNavigationModel
                {
                    DisplayManufacturers = _catalogSettings.ShowManufacturersOnHomepage,
                    DisplayImages = _catalogSettings.ShowManufacturerPictures,
                    DisplayAllManufacturersLink = manufacturers.Count > manufacturerItemsToDisplay
                };

                foreach (var manufacturer in manufacturers.Take(manufacturerItemsToDisplay))
                {
                    var modelMan = new ManufacturerBriefInfoModel
                    {
                        Id = manufacturer.Id,
                        Name = manufacturer.GetLocalized(x => x.Name),
                        SeName = manufacturer.GetSeName(),
                        DisplayOrder = manufacturer.DisplayOrder,
						PictureId = manufacturer.PictureId,
						PictureUrl = _pictureService.GetUrl(manufacturer.PictureId.GetValueOrDefault(), _mediaSettings.ManufacturerThumbPictureSize, !_catalogSettings.HideManufacturerDefaultPictures),
						HasPicture = manufacturer.PictureId != null
                    };
                    model.Manufacturers.Add(modelMan);
                }
                
                return model;
            }, TimeSpan.FromHours(6));

            return cacheModel;
        }
    }
}