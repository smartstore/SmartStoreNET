using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Orders;
using SmartStore.Services;
using SmartStore.Services.Catalog;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Messages;
using SmartStore.Services.Orders;
using SmartStore.Services.Security;
using SmartStore.Services.Seo;
using SmartStore.Services.Stores;
using SmartStore.Services.Tax;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.UI.Captcha;
using SmartStore.Web.Infrastructure.Cache;
using SmartStore.Web.Models.Catalog;

namespace SmartStore.Web.Controllers
{
	public partial class ProductController : PublicControllerBase
	{
		#region Fields

		private readonly ICommonServices _services;
		private readonly IManufacturerService _manufacturerService;
		private readonly IProductService _productService;
		private readonly IProductAttributeService _productAttributeService;
		private readonly IProductAttributeParser _productAttributeParser;
		private readonly ITaxService _taxService;
		private readonly ICurrencyService _currencyService;
		private readonly IPictureService _pictureService;
		private readonly IPriceCalculationService _priceCalculationService;
		private readonly IPriceFormatter _priceFormatter;
		private readonly ICustomerContentService _customerContentService;
		private readonly ICustomerService _customerService;
		private readonly IShoppingCartService _shoppingCartService;
		private readonly IRecentlyViewedProductsService _recentlyViewedProductsService;
		private readonly IWorkflowMessageService _workflowMessageService;
		private readonly IProductTagService _productTagService;
		private readonly IOrderReportService _orderReportService;
		private readonly IBackInStockSubscriptionService _backInStockSubscriptionService;
		private readonly IAclService _aclService;
		private readonly IStoreMappingService _storeMappingService;
		private readonly MediaSettings _mediaSettings;
		private readonly CatalogSettings _catalogSettings;
		private readonly ShoppingCartSettings _shoppingCartSettings;
		private readonly LocalizationSettings _localizationSettings;
		private readonly CaptchaSettings _captchaSettings;
		private readonly CatalogHelper _helper;
        private readonly IDownloadService _downloadService;
        private readonly ILocalizationService _localizationService;

		#endregion

		#region Constructors

		public ProductController(
			ICommonServices services,
			IManufacturerService manufacturerService,
			IProductService productService,
			IProductAttributeService productAttributeService,
			IProductAttributeParser productAttributeParser,
			ITaxService taxService,
			ICurrencyService currencyService,
			IPictureService pictureService,
			IPriceCalculationService priceCalculationService, 
			IPriceFormatter priceFormatter,
			ICustomerContentService customerContentService, 
			ICustomerService customerService,
			IShoppingCartService shoppingCartService,
			IRecentlyViewedProductsService recentlyViewedProductsService, 
			IWorkflowMessageService workflowMessageService, 
			IProductTagService productTagService,
			IOrderReportService orderReportService,
			IBackInStockSubscriptionService backInStockSubscriptionService, 
			IAclService aclService,
			IStoreMappingService storeMappingService,
			MediaSettings mediaSettings, 
			CatalogSettings catalogSettings,
			ShoppingCartSettings shoppingCartSettings,
			LocalizationSettings localizationSettings, 
			CaptchaSettings captchaSettings,
			CatalogHelper helper,
            IDownloadService downloadService,
            ILocalizationService localizationService)
        {
			this._services = services;
			this._manufacturerService = manufacturerService;
			this._productService = productService;
			this._productAttributeService = productAttributeService;
			this._productAttributeParser = productAttributeParser;
			this._taxService = taxService;
			this._currencyService = currencyService;
			this._pictureService = pictureService;
			this._priceCalculationService = priceCalculationService;
			this._priceFormatter = priceFormatter;
			this._customerContentService = customerContentService;
			this._customerService = customerService;
			this._shoppingCartService = shoppingCartService;
			this._recentlyViewedProductsService = recentlyViewedProductsService;
			this._workflowMessageService = workflowMessageService;
			this._productTagService = productTagService;
			this._orderReportService = orderReportService;
			this._backInStockSubscriptionService = backInStockSubscriptionService;
			this._aclService = aclService;
			this._storeMappingService = storeMappingService;
			this._mediaSettings = mediaSettings;
			this._catalogSettings = catalogSettings;
			this._shoppingCartSettings = shoppingCartSettings;
			this._localizationSettings = localizationSettings;
			this._captchaSettings = captchaSettings;
			this._helper = helper;
			this._downloadService = downloadService;
			this._localizationService = localizationService;
        }
        
        #endregion

		#region Products

		[RequireHttpsByConfigAttribute(SslRequirement.No)]
		public ActionResult ProductDetails(int productId, string attributes)
		{
			var product = _productService.GetProductById(productId);
			if (product == null || product.Deleted)
				return HttpNotFound();

			//Is published?
			//Check whether the current user has a "Manage catalog" permission
			//It allows him to preview a product before publishing
			if (!product.Published && !_services.Permissions.Authorize(StandardPermissionProvider.ManageCatalog))
				return HttpNotFound();

			//ACL (access control list)
			if (!_aclService.Authorize(product))
				return HttpNotFound();

			//Store mapping
			if (!_storeMappingService.Authorize(product))
				return HttpNotFound();

			// is product individually visible?
			if (!product.VisibleIndividually)
			{
				// find parent grouped product
				var parentGroupedProduct = _productService.GetProductById(product.ParentGroupedProductId);

				if (parentGroupedProduct == null)
					return HttpNotFound();

				var routeValues = new RouteValueDictionary();
				routeValues.Add("SeName", parentGroupedProduct.GetSeName());

				// add query string parameters
				Request.QueryString.AllKeys.Each(x => routeValues.Add(x, Request.QueryString[x]));

				return RedirectToRoute("Product", routeValues);
			}

			var selectedAttributes = new NameValueCollection();
			var attributesForProductId = 0;

			if (product.ProductType == ProductType.GroupedProduct || (product.ProductType == ProductType.BundledProduct && product.BundlePerItemPricing))
				attributesForProductId = 0;
			else
				attributesForProductId = product.Id;

			// get selected attributes from query string
			selectedAttributes.GetSelectedAttributes(Request.QueryString, _productAttributeParser.DeserializeQueryData(attributes),	attributesForProductId);

			// prepare the view model
			var model = _helper.PrepareProductDetailsPageModel(product, selectedAttributes: selectedAttributes, queryData: Request.QueryString);

			//save as recently viewed
			_recentlyViewedProductsService.AddProductToRecentlyViewedList(product.Id);

			//activity log
			_services.CustomerActivity.InsertActivity("PublicStore.ViewProduct", T("ActivityLog.PublicStore.ViewProduct"), product.Name);

			return View(model.ProductTemplateViewPath, model);
		}

		[ChildActionOnly]
		public ActionResult ProductManufacturers(int productId, bool preparePictureModel = false)
		{
			var cacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_MANUFACTURERS_MODEL_KEY,
				productId,
				!_catalogSettings.HideManufacturerDefaultPictures,
				_services.WorkContext.WorkingLanguage.Id,
				_services.StoreContext.CurrentStore.Id);

			var cacheModel = _services.Cache.Get(cacheKey, () =>
			{
				var model = _manufacturerService.GetProductManufacturersByProductId(productId)
					.Select(x =>
					{
						var m = x.Manufacturer.ToModel();
						if (preparePictureModel)
						{
							m.PictureModel.ImageUrl = _pictureService.GetPictureUrl(x.Manufacturer.PictureId.GetValueOrDefault(), 0, !_catalogSettings.HideManufacturerDefaultPictures);

							var picture = _pictureService.GetPictureUrl(x.Manufacturer.PictureId.GetValueOrDefault());
							if (picture != null)
							{
								m.PictureModel.PictureId = x.Manufacturer.PictureId.GetValueOrDefault();
								m.PictureModel.Title = string.Format(T("Media.Product.ImageLinkTitleFormat"), m.Name);
								m.PictureModel.AlternateText = string.Format(T("Media.Product.ImageAlternateTextFormat"), m.Name);
							}
						}
						return m;
					})
					.ToList();
				return model;
			}, TimeSpan.FromHours(6));

			if (cacheModel.Count == 0)
				return Content("");

			return PartialView(cacheModel);
		}

		[ChildActionOnly]
		public ActionResult ReviewOverview(int id)
		{
			var product = _productService.GetProductById(id);
			if (product == null)
				throw new ArgumentException(T("Products.NotFound", id));

			var model = new ProductReviewOverviewModel()
			{
				ProductId = product.Id,
				RatingSum = product.ApprovedRatingSum,
				TotalReviews = product.ApprovedTotalReviews,
				AllowCustomerReviews = product.AllowCustomerReviews
			};
			return PartialView(model);
		}

		[ChildActionOnly]
		public ActionResult ProductSpecifications(int productId)
		{
			var product = _productService.GetProductById(productId);
			if (product == null)
				throw new ArgumentException(T("Products.NotFound", productId));

			var model = _helper.PrepareProductSpecificationModel(product);

			if (model.Count == 0)
				return Content("");

			return PartialView(model);
		}

		[ChildActionOnly]
		public ActionResult ProductDetailReviews(int productId)
		{
			var product = _productService.GetProductById(productId);
			if (product == null || !product.AllowCustomerReviews)
				return Content("");

			var model = new ProductReviewsModel();
			_helper.PrepareProductReviewsModel(model, product);

			return PartialView(model);
		}

		[ChildActionOnly]
		public ActionResult ProductTierPrices(int productId)
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.DisplayPrices))
				return Content(""); //hide prices

			var product = _productService.GetProductById(productId);
			if (product == null)
				throw new ArgumentException(T("Products.NotFound", productId));

			if (!product.HasTierPrices)
				return Content(""); //no tier prices

			var model = product.TierPrices
				.OrderBy(x => x.Quantity)
				.FilterByStore(_services.StoreContext.CurrentStore.Id)
				.FilterForCustomer(_services.WorkContext.CurrentCustomer)
				.ToList()
				.RemoveDuplicatedQuantities()
				.Select(tierPrice =>
				{
					var m = new ProductDetailsModel.TierPriceModel()
					{
						Quantity = tierPrice.Quantity,
					};
					decimal taxRate = decimal.Zero;
					decimal priceBase = _taxService.GetProductPrice(product, _priceCalculationService.GetFinalPrice(product, _services.WorkContext.CurrentCustomer, decimal.Zero, _catalogSettings.DisplayTierPricesWithDiscounts, tierPrice.Quantity), out taxRate);
					decimal price = _currencyService.ConvertFromPrimaryStoreCurrency(priceBase, _services.WorkContext.WorkingCurrency);
					m.Price = _priceFormatter.FormatPrice(price, true, false);
					return m;
				})
				.ToList();

			return PartialView(model);
		}

		[ChildActionOnly]
		public ActionResult RelatedProducts(int productId, int? productThumbPictureSize)
		{
			var products = new List<Product>();
			var relatedProducts = _productService.GetRelatedProductsByProductId1(productId);

			foreach (var product in _productService.GetProductsByIds(relatedProducts.Select(x => x.ProductId2).ToArray()))
			{
				// Ensure has ACL permission and appropriate store mapping
				if (_aclService.Authorize(product) && _storeMappingService.Authorize(product))
					products.Add(product);
			}

			if (products.Count == 0)
			{
				return Content("");
			}

			var settings = _helper.GetBestFitProductSummaryMappingSettings(ProductSummaryViewMode.Grid, x =>
			{
				x.ThumbnailSize = productThumbPictureSize;
				x.MapDeliveryTimes = false;
			});		

			var model = _helper.MapProductSummaryModel(products, settings);
			model.ShowBasePrice = false;

			return PartialView(model);
		}

		[ChildActionOnly]
		public ActionResult ProductsAlsoPurchased(int productId, int? productThumbPictureSize)
		{
			if (!_catalogSettings.ProductsAlsoPurchasedEnabled)
			{
				return Content("");
			}				

			// load and cache report
			var productIds = _services.Cache.Get(string.Format(ModelCacheEventConsumer.PRODUCTS_ALSO_PURCHASED_IDS_KEY, productId, _services.StoreContext.CurrentStore.Id), () => 
			{
				return _orderReportService.GetAlsoPurchasedProductsIds(_services.StoreContext.CurrentStore.Id, productId, _catalogSettings.ProductsAlsoPurchasedNumber);
			});

			// Load products
			var products = _productService.GetProductsByIds(productIds);

			// ACL and store mapping
			products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();

			if (products.Count == 0)
			{
				return Content("");
			}			

			// Prepare model
			var settings = _helper.GetBestFitProductSummaryMappingSettings(ProductSummaryViewMode.Mini, x =>
			{
				x.ThumbnailSize = productThumbPictureSize;
			});

			var model = _helper.MapProductSummaryModel(products, settings);

			return PartialView(model);
		}

		[ChildActionOnly]
		public ActionResult ShareButton()
		{
			if (_catalogSettings.ShowShareButton && !String.IsNullOrEmpty(_catalogSettings.PageShareCode))
			{
				var shareCode = _catalogSettings.PageShareCode;
				if (_services.WebHelper.IsCurrentConnectionSecured())
				{
					//need to change the addthis link to be https linked when the page is, so that the page doesnt ask about mixed mode when viewed in https...
					shareCode = shareCode.Replace("http://", "https://");
				}

				return PartialView("ShareButton", shareCode);
			}

			return Content("");
		}

		[ChildActionOnly]
		public ActionResult CrossSellProducts(int? productThumbPictureSize)
		{
			var cart = _services.WorkContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _services.StoreContext.CurrentStore.Id);

			var products = _productService.GetCrosssellProductsByShoppingCart(cart, _shoppingCartSettings.CrossSellsNumber);

			//ACL and store mapping
			products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();

			if (products.Any())
			{
				// Cross-sell products are dispalyed on the shopping cart page.
				// We know that the entire shopping cart page is not refresh
				// even if "ShoppingCartSettings.DisplayCartAfterAddingProduct" setting  is enabled.
				// That's why we force page refresh (redirect) in this case
				var settings = _helper.GetBestFitProductSummaryMappingSettings(ProductSummaryViewMode.Grid, x =>
				{
					x.ThumbnailSize = productThumbPictureSize;
					x.ForceRedirectionAfterAddingToCart = true;
				});

				// TODO: (mc) Display this as carousel/slider
				var model = _helper.MapProductSummaryModel(products, settings);

				return PartialView(model);
			}

			return PartialView(ProductSummaryModel.Empty);
		}

		[ActionName("BackInStockSubscribe")]
		public ActionResult BackInStockSubscribePopup(int id /* productId */)
		{
			var product = _productService.GetProductById(id);
			if (product == null || product.Deleted)
				throw new ArgumentException(T("Products.NotFound", id));

			var model = new BackInStockSubscribeModel();
			model.ProductId = product.Id;
			model.ProductName = product.GetLocalized(x => x.Name);
			model.ProductSeName = product.GetSeName();
			model.IsCurrentCustomerRegistered = _services.WorkContext.CurrentCustomer.IsRegistered();
			model.MaximumBackInStockSubscriptions = _catalogSettings.MaximumBackInStockSubscriptions;
			model.CurrentNumberOfBackInStockSubscriptions = _backInStockSubscriptionService
				 .GetAllSubscriptionsByCustomerId(_services.WorkContext.CurrentCustomer.Id, _services.StoreContext.CurrentStore.Id, 0, 1)
				 .TotalCount;
			if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
				product.BackorderMode == BackorderMode.NoBackorders &&
				product.AllowBackInStockSubscriptions &&
				product.StockQuantity <= 0)
			{
				//out of stock
				model.SubscriptionAllowed = true;
				model.AlreadySubscribed = _backInStockSubscriptionService
					.FindSubscription(_services.WorkContext.CurrentCustomer.Id, product.Id, _services.StoreContext.CurrentStore.Id) != null;
			}
			return View("BackInStockSubscribePopup", model);
		}

		[HttpPost, ActionName("BackInStockSubscribe")]
		public ActionResult BackInStockSubscribePopupPOST(int id /* productId */)
		{
			var product = _productService.GetProductById(id);
			if (product == null || product.Deleted)
				throw new ArgumentException(T("Products.NotFound", id));

			if (!_services.WorkContext.CurrentCustomer.IsRegistered())
				return Content(T("BackInStockSubscriptions.OnlyRegistered"));

			if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
				product.BackorderMode == BackorderMode.NoBackorders &&
				product.AllowBackInStockSubscriptions &&
				product.StockQuantity <= 0)
			{
				//out of stock
				var subscription = _backInStockSubscriptionService
					.FindSubscription(_services.WorkContext.CurrentCustomer.Id, product.Id, _services.StoreContext.CurrentStore.Id);
				if (subscription != null)
				{
					//unsubscribe
					_backInStockSubscriptionService.DeleteSubscription(subscription);
					return Content("Unsubscribed");
				}
				else
				{
					if (_backInStockSubscriptionService
						.GetAllSubscriptionsByCustomerId(_services.WorkContext.CurrentCustomer.Id, _services.StoreContext.CurrentStore.Id, 0, 1)
						.TotalCount >= _catalogSettings.MaximumBackInStockSubscriptions)
						return Content(string.Format(T("BackInStockSubscriptions.MaxSubscriptions"), _catalogSettings.MaximumBackInStockSubscriptions));

					//subscribe   
					subscription = new BackInStockSubscription()
					{
						Customer = _services.WorkContext.CurrentCustomer,
						Product = product,
						StoreId = _services.StoreContext.CurrentStore.Id,
						CreatedOnUtc = DateTime.UtcNow
					};
					_backInStockSubscriptionService.InsertSubscription(subscription);
					return Content("Subscribed");
				}

			}
			else
			{
				return Content(T("BackInStockSubscriptions.NotAllowed"));
			}
		}

		[HttpPost]
		public ActionResult UpdateProductDetails(int productId, string itemType, int bundleItemId, FormCollection form)
		{
			int quantity = 1;
			int galleryStartIndex = -1;
			string galleryHtml = null;
			string dynamicThumbUrl = null;
			bool isAssociated = itemType.IsCaseInsensitiveEqual("associateditem");
			var pictureModel = new ProductDetailsPictureModel();
			var m = new ProductDetailsModel();
			var product = _productService.GetProductById(productId);
			var bItem = _productService.GetBundleItemById(bundleItemId);
			IList<ProductBundleItemData> bundleItems = null;
			ProductBundleItemData bundleItem = (bItem == null ? null : new ProductBundleItemData(bItem));

			var warnings = new List<string>();
			var attributes = _productAttributeService.GetProductVariantAttributesByProductId(productId);

			string attributeXml = form.CreateSelectedAttributesXml(productId, attributes, _productAttributeParser,
				_localizationService, _downloadService, _catalogSettings, this.Request, warnings, true);

			var areAllAttributesForCombinationSelected = _shoppingCartService.AreAllAttributesForCombinationSelected(attributeXml, product);

			// quantity required for tier prices
			string quantityKey = form.AllKeys.FirstOrDefault(k => k.EndsWith("EnteredQuantity"));
			if (quantityKey.HasValue())
				int.TryParse(form[quantityKey], out quantity);

			if (product.ProductType == ProductType.BundledProduct && product.BundlePerItemPricing)
			{
				bundleItems = _productService.GetBundleItems(product.Id);
				if (form.Count > 0)
				{
					// may add elements to form if they are preselected by bundle item filter
					foreach (var itemData in bundleItems)
					{
						var unused = _helper.PrepareProductDetailsPageModel(itemData.Item.Product, false, itemData, null, form);
					}
				}
			}

			// get merged model data
			_helper.PrepareProductDetailModel(m, product, isAssociated, bundleItem, bundleItems, form, quantity);

			if (bundleItem != null)	// update bundle item thumbnail
			{
				if (!bundleItem.Item.HideThumbnail)
				{
					var picture = m.GetAssignedPicture(_pictureService, null, bundleItem.Item.ProductId);
					dynamicThumbUrl = _pictureService.GetPictureUrl(picture, _mediaSettings.BundledProductPictureSize, false);
				}
			}
			else if (isAssociated) // update associated product thumbnail
			{
				var picture = m.GetAssignedPicture(_pictureService, null, productId);
				dynamicThumbUrl = _pictureService.GetPictureUrl(picture, _mediaSettings.AssociatedProductPictureSize, false);
			}
			else if (product.ProductType != ProductType.BundledProduct)		// update image gallery
			{
				var pictures = _pictureService.GetPicturesByProductId(productId);

				if (pictures.Count <= _catalogSettings.DisplayAllImagesNumber)	// all pictures rendered... only index is required
				{
					var picture = m.GetAssignedPicture(_pictureService, pictures);
					galleryStartIndex = (picture == null ? 0 : pictures.IndexOf(picture));
				}
				else
				{
					var allCombinationPictureIds = _productAttributeService.GetAllProductVariantAttributeCombinationPictureIds(product.Id);	

					_helper.PrepareProductDetailsPictureModel(
						pictureModel, 
						pictures, 
						product.GetLocalized(x => x.Name), 
						allCombinationPictureIds,
						false, 
						bundleItem, 
						m.SelectedCombination);

					galleryStartIndex = pictureModel.GalleryStartIndex;
					galleryHtml = this.RenderPartialViewToString("_PictureGallery", pictureModel);
				}
			}
 
			#region data object

            object data = new
            {
                Delivery = new
                {
                    Id = 0,
                    Name = m.DeliveryTimeName,
                    Color = m.DeliveryTimeHexValue,
                    DisplayAccordingToStock = m.DisplayDeliveryTimeAccordingToStock
                },
                Measure = new
                {
                    Weight = new { Value = m.WeightValue, Text = m.Weight },
                    Height = new { Value = product.Height, Text = m.Height },
                    Width = new { Value = product.Width, Text = m.Width },
                    Length = new { Value = product.Length, Text = m.Length }
                },
                Number = new
                {
                    Sku = new { Value = m.Sku, Show = m.ShowSku },
                    Gtin = new { Value = m.Gtin, Show = m.ShowGtin },
                    Mpn = new { Value = m.ManufacturerPartNumber, Show = m.ShowManufacturerPartNumber }
                },
                Price = new
                {
                    Base = new
                    {
                        Enabled = m.IsBasePriceEnabled,
                        Info = m.BasePriceInfo
                    },
                    Old = new
                    {
                        Value = decimal.Zero,
                        Text = m.ProductPrice.OldPrice
                    },
                    WithoutDiscount = new
                    {
                        Value = m.ProductPrice.PriceValue,
                        Text = m.ProductPrice.Price
                    },
                    WithDiscount = new
                    {
                        Value = m.ProductPrice.PriceWithDiscountValue,
                        Text = m.ProductPrice.PriceWithDiscount
                    }
                },
                Stock = new
                {
                    Quantity = new
					{ 
                        Value = product.StockQuantity,
						Show = areAllAttributesForCombinationSelected ? product.DisplayStockQuantity : false
                    },
                    Availability = new
					{ 
                        Text = m.StockAvailability,
						Show = areAllAttributesForCombinationSelected ? product.DisplayStockAvailability : false, 
                        Available = m.IsAvailable
					}
                },

                DynamicThumblUrl = dynamicThumbUrl,
                GalleryStartIndex = galleryStartIndex,
                GalleryHtml = galleryHtml
            };

			#endregion

			return new JsonResult { Data = data };
		}

		#endregion


		#region Product tags

		[ChildActionOnly]
		public ActionResult ProductTags(int productId)
		{
			var product = _productService.GetProductById(productId);
			if (product == null)
				throw new ArgumentException(T("Products.NotFound", productId));

			var cacheKey = string.Format(ModelCacheEventConsumer.PRODUCTTAG_BY_PRODUCT_MODEL_KEY, product.Id, _services.WorkContext.WorkingLanguage.Id, _services.StoreContext.CurrentStore.Id);
			var cacheModel = _services.Cache.Get(cacheKey, () =>
			{
				var model = product.ProductTags
					//filter by store
					.Where(x => _productTagService.GetProductCount(x.Id, _services.StoreContext.CurrentStore.Id) > 0)
					.Select(x =>
					{
						var ptModel = new ProductTagModel()
						{
							Id = x.Id,
							Name = x.GetLocalized(y => y.Name),
							SeName = x.GetSeName(),
							ProductCount = _productTagService.GetProductCount(x.Id, _services.StoreContext.CurrentStore.Id)
						};
						return ptModel;
					})
					.ToList();
				return model;
			});

			return PartialView(cacheModel);
		}

		#endregion


		#region Product reviews

		[ActionName("Reviews")]
		[RequireHttpsByConfigAttribute(SslRequirement.No)]
		public ActionResult Reviews(int id)
		{
			var product = _productService.GetProductById(id);
			if (product == null || product.Deleted || !product.Published || !product.AllowCustomerReviews)
				return HttpNotFound();

			var model = new ProductReviewsModel();
			_helper.PrepareProductReviewsModel(model, product);
			//only registered users can leave reviews
			if (_services.WorkContext.CurrentCustomer.IsGuest() && !_catalogSettings.AllowAnonymousUsersToReviewProduct)
				ModelState.AddModelError("", T("Reviews.OnlyRegisteredUsersCanWriteReviews"));
			//default value
			model.AddProductReview.Rating = _catalogSettings.DefaultProductRatingValue;
			return View(model);
		}

		[HttpPost, ActionName("Reviews")]
		[FormValueRequired("add-review")]
		[CaptchaValidator]
		public ActionResult ReviewsAdd(int id, ProductReviewsModel model, bool captchaValid)
		{
			var product = _productService.GetProductById(id);
			if (product == null || product.Deleted || !product.Published || !product.AllowCustomerReviews)
				return HttpNotFound();

			//validate CAPTCHA
			if (_captchaSettings.Enabled && _captchaSettings.ShowOnProductReviewPage && !captchaValid)
			{
				ModelState.AddModelError("", T("Common.WrongCaptcha"));
			}

			if (_services.WorkContext.CurrentCustomer.IsGuest() && !_catalogSettings.AllowAnonymousUsersToReviewProduct)
			{
				ModelState.AddModelError("", T("Reviews.OnlyRegisteredUsersCanWriteReviews"));
			}

			if (ModelState.IsValid)
			{
				//save review
				int rating = model.AddProductReview.Rating;
				if (rating < 1 || rating > 5)
					rating = _catalogSettings.DefaultProductRatingValue;

				bool isApproved = !_catalogSettings.ProductReviewsMustBeApproved;
				var customer = _services.WorkContext.CurrentCustomer;

				var productReview = new ProductReview
				{
					ProductId = product.Id,
					CustomerId = customer.Id,
					IpAddress = _services.WebHelper.GetCurrentIpAddress(),
					Title = model.AddProductReview.Title,
					ReviewText = model.AddProductReview.ReviewText,
					Rating = rating,
					HelpfulYesTotal = 0,
					HelpfulNoTotal = 0,
					IsApproved = isApproved,
				};
				_customerContentService.InsertCustomerContent(productReview);

				//update product totals
				_productService.UpdateProductReviewTotals(product);

				//notify store owner
				if (_catalogSettings.NotifyStoreOwnerAboutNewProductReviews)
					_workflowMessageService.SendProductReviewNotificationMessage(productReview, _localizationSettings.DefaultAdminLanguageId);

				//activity log
				_services.CustomerActivity.InsertActivity("PublicStore.AddProductReview", T("ActivityLog.PublicStore.AddProductReview"), product.Name);

				if (isApproved)
					_customerService.RewardPointsForProductReview(customer, product, true);

				_helper.PrepareProductReviewsModel(model, product);
				model.AddProductReview.Title = null;
				model.AddProductReview.ReviewText = null;

				model.AddProductReview.SuccessfullyAdded = true;
				if (!isApproved)
					model.AddProductReview.Result = T("Reviews.SeeAfterApproving");
				else
					model.AddProductReview.Result = T("Reviews.SuccessfullyAdded");

				return View(model);
			}

			//If we got this far, something failed, redisplay form
			_helper.PrepareProductReviewsModel(model, product);
			return View(model);
		}

		[HttpPost]
		public ActionResult SetReviewHelpfulness(int productReviewId, bool washelpful)
		{
			var productReview = _customerContentService.GetCustomerContentById(productReviewId) as ProductReview;
			if (productReview == null)
				throw new ArgumentException(T("Reviews.NotFound", productReviewId));

			if (_services.WorkContext.CurrentCustomer.IsGuest() && !_catalogSettings.AllowAnonymousUsersToReviewProduct)
			{
				return Json(new
				{
					Success = false,
					Result = T("Reviews.Helpfulness.OnlyRegistered").Text,
					TotalYes = productReview.HelpfulYesTotal,
					TotalNo = productReview.HelpfulNoTotal
				});
			}

			//customers aren't allowed to vote for their own reviews
			if (productReview.CustomerId == _services.WorkContext.CurrentCustomer.Id)
			{
				return Json(new
				{
					Success = false,
					Result = T("Reviews.Helpfulness.YourOwnReview").Text,
					TotalYes = productReview.HelpfulYesTotal,
					TotalNo = productReview.HelpfulNoTotal
				});
			}

			//delete previous helpfulness
			var oldPrh = (from prh in productReview.ProductReviewHelpfulnessEntries
						  where prh.CustomerId == _services.WorkContext.CurrentCustomer.Id
						  select prh).FirstOrDefault();
			if (oldPrh != null)
				_customerContentService.DeleteCustomerContent(oldPrh);

			//insert new helpfulness
			var newPrh = new ProductReviewHelpfulness
			{
				ProductReviewId = productReview.Id,
				CustomerId = _services.WorkContext.CurrentCustomer.Id,
				IpAddress = _services.WebHelper.GetCurrentIpAddress(),
				WasHelpful = washelpful,
				IsApproved = true, //always approved
			};
			_customerContentService.InsertCustomerContent(newPrh);

			//new totals
			int helpfulYesTotal = (from prh in productReview.ProductReviewHelpfulnessEntries
								   where prh.WasHelpful
								   select prh).Count();
			int helpfulNoTotal = (from prh in productReview.ProductReviewHelpfulnessEntries
								  where !prh.WasHelpful
								  select prh).Count();

			productReview.HelpfulYesTotal = helpfulYesTotal;
			productReview.HelpfulNoTotal = helpfulNoTotal;
			_customerContentService.UpdateCustomerContent(productReview);

			return Json(new
			{
				Success = true,
				Result = T("Reviews.Helpfulness.SuccessfullyVoted").Text,
				TotalYes = productReview.HelpfulYesTotal,
				TotalNo = productReview.HelpfulNoTotal
			});
		}

		#endregion


		#region Ask product question

		[ChildActionOnly]
		public ActionResult AskQuestionButton(int id)
		{
			if (!_catalogSettings.AskQuestionEnabled)
				return Content("");
			var model = new ProductAskQuestionModel()
			{
				Id = id
			};

			return PartialView(model);
		}

		[RequireHttpsByConfigAttribute(SslRequirement.No)]
		public ActionResult AskQuestion(int id)
		{
			var product = _productService.GetProductById(id);
			if (product == null || product.Deleted || !product.Published || !_catalogSettings.AskQuestionEnabled)
				return HttpNotFound();

			var customer = _services.WorkContext.CurrentCustomer;

			var model = new ProductAskQuestionModel();
			model.Id = product.Id;
			model.ProductName = product.GetLocalized(x => x.Name);
			model.ProductSeName = product.GetSeName();
			model.SenderEmail = customer.Email;
			model.SenderName = customer.GetFullName();
			model.SenderPhone = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone);
			model.Question = T("Products.AskQuestion.Question.Text").Text.FormatCurrentUI(model.ProductName);
			model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnAskQuestionPage;

			return View(model);
		}

		[HttpPost, ActionName("AskQuestion")]
		[CaptchaValidator]
		public ActionResult AskQuestionSend(ProductAskQuestionModel model, bool captchaValid)
		{
			var product = _productService.GetProductById(model.Id);
			if (product == null || product.Deleted || !product.Published || !_catalogSettings.AskQuestionEnabled)
				return HttpNotFound();

			// validate CAPTCHA
			if (_captchaSettings.Enabled && _captchaSettings.ShowOnAskQuestionPage && !captchaValid)
			{
				ModelState.AddModelError("", T("Common.WrongCaptcha"));
			}

			if (ModelState.IsValid)
			{
				// email
				var result = _workflowMessageService.SendProductQuestionMessage(
					_services.WorkContext.CurrentCustomer,
					_services.WorkContext.WorkingLanguage.Id,
					product,
					model.SenderEmail,
					model.SenderName,
					model.SenderPhone,
					Core.Html.HtmlUtils.FormatText(model.Question, false, true, false, false, false, false));

				if (result > 0)
				{
					this.NotifySuccess(T("Products.AskQuestion.Sent"), true);
					return RedirectToRoute("Product", new { SeName = product.GetSeName() });
				}
				else
				{
					ModelState.AddModelError("", T("Common.Error.SendMail"));
				}
			}

			// If we got this far, something failed, redisplay form
			var customer = _services.WorkContext.CurrentCustomer;
			model.Id = product.Id;
			model.ProductName = product.GetLocalized(x => x.Name);
			model.ProductSeName = product.GetSeName();
			model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnAskQuestionPage;
			return View(model);
		}

		#endregion


		#region Email a friend

		[ChildActionOnly]
		public ActionResult EmailAFriendButton(int id)
		{
			if (!_catalogSettings.EmailAFriendEnabled)
				return Content("");
			var model = new ProductEmailAFriendModel()
			{
				ProductId = id
			};

			return PartialView(model);
		}

		[RequireHttpsByConfigAttribute(SslRequirement.No)]
		public ActionResult EmailAFriend(int id)
		{
			var product = _productService.GetProductById(id);
			if (product == null || product.Deleted || !product.Published || !_catalogSettings.EmailAFriendEnabled)
				return HttpNotFound();

			var model = new ProductEmailAFriendModel();
			model.ProductId = product.Id;
			model.ProductName = product.GetLocalized(x => x.Name);
			model.ProductSeName = product.GetSeName();
			model.YourEmailAddress = _services.WorkContext.CurrentCustomer.Email;
			model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnEmailProductToFriendPage;
			return View(model);
		}

		[HttpPost, ActionName("EmailAFriend")]
		[CaptchaValidator]
		public ActionResult EmailAFriendSend(ProductEmailAFriendModel model, int id, bool captchaValid)
		{
			var product = _productService.GetProductById(id);
			if (product == null || product.Deleted || !product.Published || !_catalogSettings.EmailAFriendEnabled)
				return HttpNotFound();

			//validate CAPTCHA
			if (_captchaSettings.Enabled && _captchaSettings.ShowOnEmailProductToFriendPage && !captchaValid)
			{
				ModelState.AddModelError("", T("Common.WrongCaptcha"));
			}

			//check whether the current customer is guest and ia allowed to email a friend
			if (_services.WorkContext.CurrentCustomer.IsGuest() && !_catalogSettings.AllowAnonymousUsersToEmailAFriend)
			{
				ModelState.AddModelError("", T("Products.EmailAFriend.OnlyRegisteredUsers"));
			}

			if (ModelState.IsValid)
			{
				//email
				_workflowMessageService.SendProductEmailAFriendMessage(_services.WorkContext.CurrentCustomer,
						_services.WorkContext.WorkingLanguage.Id, product,
						model.YourEmailAddress, model.FriendEmail,
						Core.Html.HtmlUtils.FormatText(model.PersonalMessage, false, true, false, false, false, false));

				model.ProductId = product.Id;
				model.ProductName = product.GetLocalized(x => x.Name);
				model.ProductSeName = product.GetSeName();

				//model.SuccessfullySent = true;
				//model.Result = T("Products.EmailAFriend.SuccessfullySent");

				//return View(model);

				NotifySuccess(T("Products.EmailAFriend.SuccessfullySent"));

				return RedirectToRoute("Product", new { SeName = model.ProductSeName });
			}

			//If we got this far, something failed, redisplay form
			model.ProductId = product.Id;
			model.ProductName = product.GetLocalized(x => x.Name);
			model.ProductSeName = product.GetSeName();
			model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnEmailProductToFriendPage;
			return View(model);
		}

		#endregion

	}
}
