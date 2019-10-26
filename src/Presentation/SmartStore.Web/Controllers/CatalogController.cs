﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Web.Mvc;
using SmartStore.Core.Caching;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Security;
using SmartStore.Services;
using SmartStore.Services.Catalog;
using SmartStore.Services.Common;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Orders;
using SmartStore.Services.Search;
using SmartStore.Services.Security;
using SmartStore.Services.Seo;
using SmartStore.Services.Stores;
using SmartStore.Utilities;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.UI;
using SmartStore.Web.Infrastructure.Cache;
using SmartStore.Web.Models.Catalog;
using SmartStore.Web.Models.Media;

namespace SmartStore.Web.Controllers
{
    public partial class CatalogController : PublicControllerBase
    {
		private readonly ICommonServices _services;
        private readonly IMenuService _menuService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IProductService _productService;
        private readonly ICategoryTemplateService _categoryTemplateService;
        private readonly IManufacturerTemplateService _manufacturerTemplateService;
        private readonly IPictureService _pictureService;
		private readonly IOrderReportService _orderReportService;
		private readonly IProductTagService _productTagService;
		private readonly IRecentlyViewedProductsService _recentlyViewedProductsService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IAclService _aclService;
		private readonly IStoreMappingService _storeMappingService;
		private readonly ICatalogSearchService _catalogSearchService;
		private readonly MediaSettings _mediaSettings;
        private readonly CatalogSettings _catalogSettings;
		private readonly ICompareProductsService _compareProductsService;
        private readonly CatalogHelper _helper;
		private readonly IBreadcrumb _breadcrumb;

		public CatalogController(
			ICommonServices services,
            IMenuService menuService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService, 
			IProductService productService,
            ICategoryTemplateService categoryTemplateService,
            IManufacturerTemplateService manufacturerTemplateService,
			IOrderReportService orderReportService,
			IProductTagService productTagService,
			IRecentlyViewedProductsService recentlyViewedProductsService,
            IPictureService pictureService,
			ICompareProductsService compareProductsService,
			IGenericAttributeService genericAttributeService,
			IAclService aclService,
			IStoreMappingService storeMappingService,
			ICatalogSearchService catalogSearchService,
			MediaSettings mediaSettings, 
			CatalogSettings catalogSettings,
            CatalogHelper helper,
			IBreadcrumb breadcrumb)
        {
			_services = services;
            _menuService = menuService;
			_categoryService = categoryService;
            _manufacturerService = manufacturerService;
            _productService = productService;
            _categoryTemplateService = categoryTemplateService;
            _manufacturerTemplateService = manufacturerTemplateService;
			_orderReportService = orderReportService;
			_productTagService = productTagService;
			_recentlyViewedProductsService = recentlyViewedProductsService;
			_compareProductsService = compareProductsService;
            _pictureService = pictureService;
            _genericAttributeService = genericAttributeService;
            _aclService = aclService;
			_storeMappingService = storeMappingService;
			_catalogSearchService = catalogSearchService;
            _mediaSettings = mediaSettings;
            _catalogSettings = catalogSettings;
            _helper = helper;
			_breadcrumb = breadcrumb;
        }

        #region Categories

        [RewriteUrl(SslRequirement.No)]
        public ActionResult Category(int categoryId, CatalogSearchQuery query)
        {
			var category = _categoryService.GetCategoryById(categoryId);
            if (category == null || category.Deleted)
				return HttpNotFound();

            // Check whether the current user has a "Manage catalog" permission.
            // It allows him to preview a category before publishing.
            if (!category.Published && !_services.Permissions.Authorize(Permissions.Catalog.Category.Read))
				return HttpNotFound();

            // ACL (access control list).
            if (!_aclService.Authorize(category))
				return HttpNotFound();

			// Store mapping.
			if (!_storeMappingService.Authorize(category))
				return HttpNotFound();

            var store = _services.StoreContext.CurrentStore;
            var customer = _services.WorkContext.CurrentCustomer;

            // 'Continue shopping' URL.
            if (!_services.WorkContext.CurrentCustomer.IsSystemAccount)
			{
				_genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.LastContinueShoppingPage,	_services.WebHelper.GetThisPageUrl(false), store.Id);
			}

            var model = category.ToModel();
			if (query.IsSubPage && !_catalogSettings.ShowDescriptionInSubPages)
			{
				model.Description.ChangeValue(string.Empty);
			}

			_services.DisplayControl.Announce(category);

            // Category breadcrumb.
			if (_catalogSettings.CategoryBreadcrumbEnabled)
			{
                _helper.GetCategoryBreadcrumb(_breadcrumb, ControllerContext);
			}

			// Products.
			var catIds = new int[] { categoryId };
			if (_catalogSettings.ShowProductsFromSubcategories)
			{
				// Include subcategories.
				catIds = catIds.Concat(_helper.GetChildCategoryIds(categoryId)).ToArray();
			}

			query.WithCategoryIds(_catalogSettings.IncludeFeaturedProductsInNormalLists ? (bool?)null : false, catIds);

			var searchResult = _catalogSearchService.Search(query);
			model.SearchResult = searchResult;

			var mappingSettings = _helper.GetBestFitProductSummaryMappingSettings(query.GetViewMode());
			model.Products = _helper.MapProductSummaryModel(searchResult.Hits, mappingSettings);

			model.SubCategoryDisplayType = _catalogSettings.SubCategoryDisplayType;

			var customerRolesIds = customer.CustomerRoles.Where(x => x.Active).Select(x => x.Id).ToList();
			var pictureSize = _mediaSettings.CategoryThumbPictureSize;
			var fallbackType = _catalogSettings.HideCategoryDefaultPictures ? FallbackPictureType.NoFallback : FallbackPictureType.Entity;

			var hideSubCategories = _catalogSettings.SubCategoryDisplayType == SubCategoryDisplayType.Hide 
				|| (_catalogSettings.SubCategoryDisplayType == SubCategoryDisplayType.AboveProductList && query.IsSubPage && !_catalogSettings.ShowSubCategoriesInSubPages);
			var hideFeaturedProducts = _catalogSettings.IgnoreFeaturedProducts || (query.IsSubPage && !_catalogSettings.IncludeFeaturedProductsInSubPages);

			// Subcategories.
			if (!hideSubCategories)
			{
				var subCategories = _categoryService.GetAllCategoriesByParentCategoryId(categoryId);
                model.SubCategories = _helper.MapCategorySummaryModel(subCategories, pictureSize);
			}

			// Featured Products.
			if (!hideFeaturedProducts)
			{
				CatalogSearchResult featuredProductsResult = null;

				string cacheKey = ModelCacheEventConsumer.CATEGORY_HAS_FEATURED_PRODUCTS_KEY.FormatInvariant(categoryId, string.Join(",", customerRolesIds), store.Id);
				var hasFeaturedProductsCache = _services.Cache.Get<bool?>(cacheKey);

				var featuredProductsQuery = new CatalogSearchQuery()
					.VisibleOnly(customer)
					.VisibleIndividuallyOnly(true)
					.WithCategoryIds(true, categoryId)
					.HasStoreId(_services.StoreContext.CurrentStore.Id)
					.WithLanguage(_services.WorkContext.WorkingLanguage)
					.WithCurrency(_services.WorkContext.WorkingCurrency);

				if (!hasFeaturedProductsCache.HasValue)
				{
					featuredProductsResult = _catalogSearchService.Search(featuredProductsQuery);
					hasFeaturedProductsCache = featuredProductsResult.TotalHitsCount > 0;
					_services.Cache.Put(cacheKey, hasFeaturedProductsCache, TimeSpan.FromHours(6));
				}

				if (hasFeaturedProductsCache.Value && featuredProductsResult == null)
				{
					featuredProductsResult = _catalogSearchService.Search(featuredProductsQuery);
				}

				if (featuredProductsResult != null)
				{
					var featuredProductsmappingSettings = _helper.GetBestFitProductSummaryMappingSettings(ProductSummaryViewMode.Grid);
					model.FeaturedProducts = _helper.MapProductSummaryModel(featuredProductsResult.Hits, featuredProductsmappingSettings);
				}
			}

			// Prepare paging/sorting/mode stuff.
			_helper.MapListActions(model.Products, category, _catalogSettings.DefaultPageSizeOptions);

			// Template.
			var templateCacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_TEMPLATE_MODEL_KEY, category.CategoryTemplateId);
			var templateViewPath = _services.Cache.Get(templateCacheKey, () =>
			{
				var template = _categoryTemplateService.GetCategoryTemplateById(category.CategoryTemplateId);
				if (template == null)
					template = _categoryTemplateService.GetAllCategoryTemplates().FirstOrDefault();
				return template.ViewPath;
			});

			// Activity log.
			_services.CustomerActivity.InsertActivity("PublicStore.ViewCategory", T("ActivityLog.PublicStore.ViewCategory"), category.Name);

			return View(templateViewPath, model);
		}

        [ChildActionOnly]
        public ActionResult HomepageCategories()
        {
			var categories = _categoryService.GetAllCategoriesDisplayedOnHomePage()
				.Where(c => _aclService.Authorize(c) && _storeMappingService.Authorize(c))
				.ToList();

            var model = _helper.MapCategorySummaryModel(categories, _mediaSettings.CategoryThumbPictureSize);

			if (model.Count == 0)
            {
                return new EmptyResult();
            }			

            return PartialView(model);
        }

        #endregion

        #region Manufacturers

        [RewriteUrl(SslRequirement.No)]
        public ActionResult Manufacturer(int manufacturerId, CatalogSearchQuery query)
        {
            var manufacturer = _manufacturerService.GetManufacturerById(manufacturerId);
            if (manufacturer == null || manufacturer.Deleted)
				return HttpNotFound();

            // Check whether the current user has a "Manage catalog" permission.
            // It allows him to preview a manufacturer before publishing.
            if (!manufacturer.Published && !_services.Permissions.Authorize(Permissions.Catalog.Manufacturer.Read))
				return HttpNotFound();

			// Store mapping.
			if (!_storeMappingService.Authorize(manufacturer))
				return HttpNotFound();

			// 'Continue shopping' URL.
			if (!_services.WorkContext.CurrentCustomer.IsSystemAccount)
			{
				_genericAttributeService.SaveAttribute(_services.WorkContext.CurrentCustomer,
					SystemCustomerAttributeNames.LastContinueShoppingPage,
					_services.WebHelper.GetThisPageUrl(false),
					_services.StoreContext.CurrentStore.Id);
			}

            var model = manufacturer.ToModel();
			if (query.IsSubPage && !_catalogSettings.ShowDescriptionInSubPages)
			{
				model.Description.ChangeValue(string.Empty);
			}

			model.PictureModel = _helper.PrepareManufacturerPictureModel(manufacturer, model.Name);

			var customerRolesIds = _services.WorkContext.CurrentCustomer.CustomerRoles.Where(x => x.Active).Select(x => x.Id).ToList();

			// Featured products.
			var hideFeaturedProducts = _catalogSettings.IgnoreFeaturedProducts || (query.IsSubPage && !_catalogSettings.IncludeFeaturedProductsInSubPages);
			if (!hideFeaturedProducts)
			{
				CatalogSearchResult featuredProductsResult = null;

				string cacheKey = ModelCacheEventConsumer.MANUFACTURER_HAS_FEATURED_PRODUCTS_KEY.FormatInvariant(
					manufacturerId, string.Join(",", customerRolesIds), _services.StoreContext.CurrentStore.Id);
				var hasFeaturedProductsCache = _services.Cache.Get<bool?>(cacheKey);

				var featuredProductsQuery = new CatalogSearchQuery()
					.VisibleOnly(_services.WorkContext.CurrentCustomer)
					.VisibleIndividuallyOnly(true)
					.WithManufacturerIds(true, manufacturerId)
					.HasStoreId(_services.StoreContext.CurrentStore.Id)
					.WithLanguage(_services.WorkContext.WorkingLanguage)
					.WithCurrency(_services.WorkContext.WorkingCurrency);

				if (!hasFeaturedProductsCache.HasValue)
				{
					featuredProductsResult = _catalogSearchService.Search(featuredProductsQuery);
					hasFeaturedProductsCache = featuredProductsResult.TotalHitsCount > 0;
					_services.Cache.Put(cacheKey, hasFeaturedProductsCache, TimeSpan.FromHours(6));
				}

				if (hasFeaturedProductsCache.Value && featuredProductsResult == null)
				{
					featuredProductsResult = _catalogSearchService.Search(featuredProductsQuery);
				}

				if (featuredProductsResult != null)
				{
					// TODO: (mc) determine settings properly
					var featuredProductsmappingSettings = _helper.GetBestFitProductSummaryMappingSettings(ProductSummaryViewMode.Grid);
					model.FeaturedProducts = _helper.MapProductSummaryModel(featuredProductsResult.Hits, featuredProductsmappingSettings);
				}
			}

			// Products
			query.WithManufacturerIds(_catalogSettings.IncludeFeaturedProductsInNormalLists ? null : (bool?)false, manufacturerId);

			var searchResult = _catalogSearchService.Search(query);
			model.SearchResult = searchResult;

			var mappingSettings = _helper.GetBestFitProductSummaryMappingSettings(query.GetViewMode());
			model.Products = _helper.MapProductSummaryModel(searchResult.Hits, mappingSettings);

			// Prepare paging/sorting/mode stuff
			_helper.MapListActions(model.Products, manufacturer, _catalogSettings.DefaultPageSizeOptions);

			// Template
			var templateCacheKey = string.Format(ModelCacheEventConsumer.MANUFACTURER_TEMPLATE_MODEL_KEY, manufacturer.ManufacturerTemplateId);
            var templateViewPath = _services.Cache.Get(templateCacheKey, () =>
            {
                var template = _manufacturerTemplateService.GetManufacturerTemplateById(manufacturer.ManufacturerTemplateId);
                if (template == null)
                    template = _manufacturerTemplateService.GetAllManufacturerTemplates().FirstOrDefault();
                return template.ViewPath;
            });

            //activity log
			_services.CustomerActivity.InsertActivity("PublicStore.ViewManufacturer", T("ActivityLog.PublicStore.ViewManufacturer"), manufacturer.Name);

			_services.DisplayControl.Announce(manufacturer);

            return View(templateViewPath, model);
        }

        [RewriteUrl(SslRequirement.No)]
        public ActionResult ManufacturerAll()
        {
            var model = new List<ManufacturerModel>();

            // TODO: result isn't cached, DO IT!
            var manufacturers = _manufacturerService.GetAllManufacturers(null, _services.StoreContext.CurrentStore.Id);
            foreach (var manufacturer in manufacturers)
            {
                var manuModel = manufacturer.ToModel();
                manuModel.PictureModel = _helper.PrepareManufacturerPictureModel(manufacturer, manuModel.Name);
                model.Add(manuModel);
            }

			_services.DisplayControl.AnnounceRange(manufacturers);

            ViewBag.SortManufacturersAlphabetically = _catalogSettings.SortManufacturersAlphabetically;

            return View(model);
        }

        [ChildActionOnly]
        public ActionResult HomepageManufacturers()
        {
            if (_catalogSettings.ManufacturerItemsToDisplayOnHomepage > 0 && _catalogSettings.ShowManufacturersOnHomepage)
            {
                var model = _helper.PrepareManufacturerNavigationModel(_catalogSettings.ManufacturerItemsToDisplayOnHomepage);
                if (model.Manufacturers.Any())
                {
                    return PartialView(model);
                }
            }

            return new EmptyResult();
        }

        #endregion

		#region HomePage

		[ChildActionOnly]
		public ActionResult HomepageBestSellers(int? productThumbPictureSize)
		{
			if (!_catalogSettings.ShowBestsellersOnHomepage || _catalogSettings.NumberOfBestsellersOnHomepage == 0)
			{
				return Content("");
			}

			// Load report from cache
			var report = _services.Cache.Get(string.Format(ModelCacheEventConsumer.HOMEPAGE_BESTSELLERS_IDS_KEY, _services.StoreContext.CurrentStore.Id), () => 
			{
				return _orderReportService.BestSellersReport(_services.StoreContext.CurrentStore.Id, null, null, null, null, null, 0, _catalogSettings.NumberOfBestsellersOnHomepage);
			});

			// Load products
			var products = _productService.GetProductsByIds(report.Select(x => x.ProductId).ToArray());

			// ACL and store mapping
			products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();


			var viewMode = _catalogSettings.UseSmallProductBoxOnHomePage ? ProductSummaryViewMode.Mini : ProductSummaryViewMode.Grid;

			var settings = _helper.GetBestFitProductSummaryMappingSettings(viewMode, x => 
			{
				x.ThumbnailSize = productThumbPictureSize;
			});

			var model = _helper.MapProductSummaryModel(products, settings);
			model.GridColumnSpan = GridColumnSpan.Max6Cols;

			return PartialView(model);
		}

		[ChildActionOnly]
		public ActionResult HomepageProducts(int? productThumbPictureSize)
		{
			var products = _productService.GetAllProductsDisplayedOnHomePage();

			// ACL and store mapping
			products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();

			var viewMode = _catalogSettings.UseSmallProductBoxOnHomePage ? ProductSummaryViewMode.Mini : ProductSummaryViewMode.Grid;

			var settings = _helper.GetBestFitProductSummaryMappingSettings(viewMode, x => 
			{
				x.ThumbnailSize = productThumbPictureSize;
			});			

			var model = _helper.MapProductSummaryModel(products, settings);
			model.GridColumnSpan = GridColumnSpan.Max6Cols;

			return PartialView(model);
		}

		#endregion

		#region Products by Tag

		[ChildActionOnly]
		public ActionResult PopularProductTags()
		{
            if (!_catalogSettings.ShowPopularProductTagsOnHomepage)
            {
                return new EmptyResult();
            }

            var store = _services.StoreContext.CurrentStore;

            var cacheKey = string.Format(ModelCacheEventConsumer.PRODUCTTAG_POPULAR_MODEL_KEY, _services.WorkContext.WorkingLanguage.Id, store.Id);
			var cacheModel = _services.Cache.Get(cacheKey, () =>
			{
				var model = new PopularProductTagsModel();

				var allTags = _productTagService
					.GetAllProductTags()
					.Where(x => _productTagService.GetProductCount(x.Id, store.Id) > 0)
					.OrderByDescending(x => _productTagService.GetProductCount(x.Id, store.Id))
					.ToList();

				var tags = allTags
					.Take(_catalogSettings.NumberOfProductTags)
					.ToList();

				tags = tags.OrderBy(x => x.GetLocalized(y => y.Name)).ToList();

				model.TotalTags = allTags.Count;

                foreach (var tag in tags)
                {
                    model.Tags.Add(new ProductTagModel
                    {
                        Id = tag.Id,
                        Name = tag.GetLocalized(y => y.Name),
                        SeName = tag.GetSeName(),
                        ProductCount = _productTagService.GetProductCount(tag.Id, store.Id)
                    });
                }

				return model;
			});

			return PartialView(cacheModel);
		}

		[RewriteUrl(SslRequirement.No)]
		public ActionResult ProductsByTag(int productTagId, CatalogSearchQuery query)
		{
			var productTag = _productTagService.GetProductTagById(productTagId);
			if (productTag == null)
				return HttpNotFound();

			var model = new ProductsByTagModel()
			{
				Id = productTag.Id,
				TagName = productTag.GetLocalized(y => y.Name)
			};

			// Products
			query.WithProductTagIds(new int[] { productTagId });

			var searchResult = _catalogSearchService.Search(query);
			model.SearchResult = searchResult;

			var mappingSettings = _helper.GetBestFitProductSummaryMappingSettings(query.GetViewMode());
			model.Products = _helper.MapProductSummaryModel(searchResult.Hits, mappingSettings);

			// Prepare paging/sorting/mode stuff
			_helper.MapListActions(model.Products, null, _catalogSettings.DefaultPageSizeOptions);

			return View(model);
		}

		[RewriteUrl(SslRequirement.No)]
		public ActionResult ProductTagsAll()
		{
            var store = _services.StoreContext.CurrentStore;
			var model = new PopularProductTagsModel();

			model.Tags = _productTagService
				.GetAllProductTags()
				.Where(x => _productTagService.GetProductCount(x.Id, store.Id) > 0)
				.OrderBy(x => x.GetLocalized(y => y.Name))
				.Select(x =>
				{
					var ptModel = new ProductTagModel
					{
						Id = x.Id,
						Name = x.GetLocalized(y => y.Name),
						SeName = x.GetSeName(),
						ProductCount = _productTagService.GetProductCount(x.Id, store.Id)
					};
					return ptModel;
				})
				.ToList();

			return View(model);
		}

		#endregion

		#region Recently[...]Products

		[RewriteUrl(SslRequirement.No)]
		public ActionResult RecentlyViewedProducts(CatalogSearchQuery query)
		{
			if (!_catalogSettings.RecentlyViewedProductsEnabled || _catalogSettings.RecentlyViewedProductsNumber <= 0)
			{
				return View(ProductSummaryModel.Empty);
			}

			var products = _recentlyViewedProductsService.GetRecentlyViewedProducts(_catalogSettings.RecentlyViewedProductsNumber);
			var settings = _helper.GetBestFitProductSummaryMappingSettings(ProductSummaryViewMode.List);
			var model = _helper.MapProductSummaryModel(products, settings);

			return View(model);
		}

		[ChildActionOnly]
		public ActionResult RecentlyViewedProductsBlock()
		{
			if (!_catalogSettings.RecentlyViewedProductsEnabled)
			{
				return Content("");
			}

			var products = _recentlyViewedProductsService.GetRecentlyViewedProducts(_catalogSettings.RecentlyViewedProductsNumber);
			if (products.Count == 0)
			{
				return Content("");
			}

			var settings = _helper.GetBestFitProductSummaryMappingSettings(ProductSummaryViewMode.Mini, x => 
			{
				x.MapManufacturers = _catalogSettings.ShowManufacturerInGridStyleLists;
			});

			var model = _helper.MapProductSummaryModel(products, settings);

			return PartialView(model);
		}

		[RewriteUrl(SslRequirement.No)]
		public ActionResult RecentlyAddedProducts(CatalogSearchQuery query)
		{
			if (!_catalogSettings.RecentlyAddedProductsEnabled || _catalogSettings.RecentlyAddedProductsNumber <= 0)
			{
				return View(ProductSummaryModel.Empty);
			}

			query.Sorting.Clear();
			query = query
				.BuildFacetMap(false)
				.SortBy(ProductSortingEnum.CreatedOn)
				.Slice(0, _catalogSettings.RecentlyAddedProductsNumber);

			var result = _catalogSearchService.Search(query);

			var settings = _helper.GetBestFitProductSummaryMappingSettings(query.GetViewMode());
			var model = _helper.MapProductSummaryModel(result.Hits.ToList(), settings);
			model.GridColumnSpan = GridColumnSpan.Max5Cols;

			return View(model);
		}

		public ActionResult RecentlyAddedProductsRss(CatalogSearchQuery query)
		{
			// TODO: (mc) find a more prominent place for the "NewProducts" link (may be in main menu?)
			var protocol = _services.WebHelper.IsCurrentConnectionSecured() ? "https" : "http";
			var selfLink = Url.RouteUrl("RecentlyAddedProductsRSS", null, protocol);
			var recentProductsLink = Url.RouteUrl("RecentlyAddedProducts", null, protocol);

			var title = "{0} - {1}".FormatInvariant(_services.StoreContext.CurrentStore.Name, T("RSS.RecentlyAddedProducts"));

			var feed = new SmartSyndicationFeed(new Uri(recentProductsLink), title, T("RSS.InformationAboutProducts"));

			feed.AddNamespaces(true);
			feed.Init(selfLink, _services.WorkContext.WorkingLanguage);

			if (!_catalogSettings.RecentlyAddedProductsEnabled || _catalogSettings.RecentlyAddedProductsNumber <= 0)
			{
				return new RssActionResult { Feed = feed };
			}
				
			var items = new List<SyndicationItem>();

			query.Sorting.Clear();
			query = query
				.BuildFacetMap(false)
				.SortBy(ProductSortingEnum.CreatedOn)
				.Slice(0, _catalogSettings.RecentlyAddedProductsNumber);

			var result = _catalogSearchService.Search(query);

			var storeUrl = _services.StoreService.GetHost(_services.StoreContext.CurrentStore);

			// Prefecthing
			var allPictureInfos = _pictureService.GetPictureInfos(result.Hits);

			//_mediaSettings.ProductDetailsPictureSize, false, storeUrl

			foreach (var product in result.Hits)
			{
				string productUrl = Url.RouteUrl("Product", new { SeName = product.GetSeName() }, protocol);
				if (productUrl.HasValue())
				{
					var item = feed.CreateItem(
						product.GetLocalized(x => x.Name),
						product.GetLocalized(x => x.ShortDescription),
						productUrl,
						product.CreatedOnUtc,
						product.FullDescription);

					try
					{
						// we add only the first picture
						var picture = _pictureService.GetPictureById(product.MainPictureId.GetValueOrDefault());
						if (picture != null)
						{
							feed.AddEnclosure(item, picture, _pictureService.GetUrl(picture, _mediaSettings.ProductDetailsPictureSize, FallbackPictureType.NoFallback, storeUrl));
						}
					}
					catch { }

					items.Add(item);
				}
			}

			feed.Items = items;

			_services.DisplayControl.AnnounceRange(result.Hits);

			return new RssActionResult { Feed = feed };
		}

		#endregion

		#region Comparing products

		[ActionName("AddProductToCompare")]
		public ActionResult AddProductToCompareList(int id)
		{
			var product = _productService.GetProductById(id);
			if (product == null || product.Deleted || product.IsSystemProduct || !product.Published)
				return HttpNotFound();

			if (!_catalogSettings.CompareProductsEnabled)
				return HttpNotFound();

			_compareProductsService.AddProductToCompareList(id);

			//activity log
			_services.CustomerActivity.InsertActivity("PublicStore.AddToCompareList", T("ActivityLog.PublicStore.AddToCompareList"), product.Name);

			return RedirectToRoute("CompareProducts");
		}

		// ajax
		[HttpPost]
		[ActionName("AddProductToCompare")]
		public ActionResult AddProductToCompareListAjax(int id)
		{
			var product = _productService.GetProductById(id);
			if (product == null || product.Deleted || product.IsSystemProduct || !product.Published || !_catalogSettings.CompareProductsEnabled)
			{
				return Json(new
				{
					success = false,
					message = T("AddProductToCompareList.CouldNotBeAdded")
				});
			}

			_compareProductsService.AddProductToCompareList(id);

			//activity log
			_services.CustomerActivity.InsertActivity("PublicStore.AddToCompareList", T("ActivityLog.PublicStore.AddToCompareList"), product.Name);

			return Json(new
			{
				success = true,
				message = string.Format(T("AddProductToCompareList.ProductWasAdded"), product.Name)
			});
		}

		[ActionName("RemoveProductFromCompare")]
		public ActionResult RemoveProductFromCompareList(int id)
		{
			var product = _productService.GetProductById(id);
			if (product == null)
				return HttpNotFound();

			if (!_catalogSettings.CompareProductsEnabled)
				return HttpNotFound();

			_compareProductsService.RemoveProductFromCompareList(id);

			return RedirectToRoute("CompareProducts");
		}

		// ajax
		[HttpPost]
		[ActionName("RemoveProductFromCompare")]
		public ActionResult RemoveProductFromCompareListAjax(int id)
		{
			var product = _productService.GetProductById(id);
			if (product == null || !_catalogSettings.CompareProductsEnabled)
			{
				return Json(new
				{
					success = false,
					message = T("AddProductToCompareList.CouldNotBeRemoved")
				});
			}

			_compareProductsService.RemoveProductFromCompareList(id);

			return Json(new
			{
				success = true,
				message = string.Format(T("AddProductToCompareList.ProductWasDeleted"), product.Name)
			});
		}

        [RewriteUrl(SslRequirement.No)]
		public ActionResult CompareProducts()
		{
			if (!_catalogSettings.CompareProductsEnabled)
			{
				return HttpNotFound();
			}

			var products = _compareProductsService.GetComparedProducts();
			var settings = _helper.GetBestFitProductSummaryMappingSettings(ProductSummaryViewMode.Compare);
			var model = _helper.MapProductSummaryModel(products, settings);

			return View(model);
		}

		public ActionResult OffCanvasCompare()
		{
			if (!_catalogSettings.CompareProductsEnabled)
			{
				return PartialView(ProductSummaryModel.Empty);
			}

			var products = _compareProductsService.GetComparedProducts();
			var settings = _helper.GetBestFitProductSummaryMappingSettings(ProductSummaryViewMode.Grid, x => 
			{
				x.MapAttributes = false;
				x.MapColorAttributes = false;
				x.MapManufacturers = false;
			});
			var model = _helper.MapProductSummaryModel(products, settings);

			return PartialView(model);
		}

		public ActionResult ClearCompareList()
		{
			if (!_catalogSettings.CompareProductsEnabled)
				return RedirectToRoute("HomePage");

			_compareProductsService.ClearCompareProducts();

			return RedirectToRoute("CompareProducts");
		}

        // ajax
        [HttpPost]
        [ActionName("ClearCompareList")]
        public ActionResult ClearCompareListAjax()
        {
            _compareProductsService.ClearCompareProducts();

            return Json(new
            {
                success = true,
                message = T("CompareList.ListWasCleared")
            });
        }

        #endregion

        #region OffCanvasMenu 

        /// <summary>
        /// Called by ajax, to get a partial catalog menu to display in OffCanvasMenu.
        /// </summary>
        /// <param name="categoryId">EntityId of the category to which should be navigated in the OffCanvasMenu.</param>
        /// <param name="currentCategoryId">EntityId of the category that is currently displayed in the shop (WebViewPage.CurrentCategoryId).</param>
        /// <param name="currentProductId">EntityId of the product that is currently displayed in the shop (WebViewPage.CurrentProductId).</param>
        [HttpPost]
        public ActionResult OffCanvasMenuCategories(int categoryId, int currentCategoryId, int currentProductId)
        {
            var menu = _menuService.GetMenu("Main");
            if (menu == null)
            {
                return new EmptyResult();
            }

            var model = menu.CreateModel("offcanvas", ControllerContext);

            ViewBag.SelectedNode = categoryId == 0
                ? model.Root
                : ViewBag.SelectedNode = model.Root.SelectNode(x => x.Value.EntityId == categoryId) ?? model.Root.SelectNodeById(categoryId);

            return PartialView(model);
        }

        // ajax
        [HttpPost]
        public ActionResult OffCanvasMenuManufacturers()
        {
            var model = _helper.PrepareManufacturerNavigationModel(_catalogSettings.ManufacturerItemsToDisplayInOffcanvasMenu);

            return PartialView("OffCanvasMenuManufacturers", model);
        }

        [HttpPost]
        public ActionResult OffCanvasMenu()
        {
            ViewBag.ShowManufacturers = false;
			ViewBag.ShowCategories = false;

			if (_catalogSettings.ShowManufacturersInOffCanvas == true && 
				_catalogSettings.ManufacturerItemsToDisplayInOffcanvasMenu > 0 &&
				_services.Permissions.Authorize(Permissions.System.AccessShop))
            {
                ViewBag.ShowManufacturers = true;
            }

			if (_services.Permissions.Authorize(Permissions.System.AccessShop))
			{
				ViewBag.ShowCategories = true;
			}
			
			return PartialView();
        }
        
        #endregion
    }
}
