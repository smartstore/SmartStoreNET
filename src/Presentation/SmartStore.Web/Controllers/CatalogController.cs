using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Media;
using SmartStore.Services;
using SmartStore.Services.Catalog;
using SmartStore.Services.Common;
using SmartStore.Services.Directory;
using SmartStore.Services.Filter;
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
using SmartStore.Web.Infrastructure.Cache;
using SmartStore.Web.Models.Catalog;
using SmartStore.Web.Models.Media;

namespace SmartStore.Web.Controllers
{
	public partial class CatalogController : PublicControllerBase
    {
		private readonly ICommonServices _services;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IProductService _productService;
        private readonly ICategoryTemplateService _categoryTemplateService;
        private readonly IManufacturerTemplateService _manufacturerTemplateService;
        private readonly ICurrencyService _currencyService;
        private readonly IPictureService _pictureService;
        private readonly IPriceFormatter _priceFormatter;
		private readonly IOrderReportService _orderReportService;
		private readonly IProductTagService _productTagService;
		private readonly IRecentlyViewedProductsService _recentlyViewedProductsService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IAclService _aclService;
		private readonly IStoreMappingService _storeMappingService;
		private readonly ICatalogSearchService _catalogSearchService;
		private readonly MediaSettings _mediaSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly IFilterService _filterService;
		private readonly ICompareProductsService _compareProductsService;
		private readonly CatalogHelper _helper;

        public CatalogController(
			ICommonServices services,
			ICategoryService categoryService,
            IManufacturerService manufacturerService, 
			IProductService productService,
            ICategoryTemplateService categoryTemplateService,
            IManufacturerTemplateService manufacturerTemplateService,
			ICurrencyService currencyService,
			IOrderReportService orderReportService,
			IProductTagService productTagService,
			IRecentlyViewedProductsService recentlyViewedProductsService,
            IPictureService pictureService,
            IPriceFormatter priceFormatter,
            ISpecificationAttributeService specificationAttributeService,
			ICompareProductsService compareProductsService,
			IGenericAttributeService genericAttributeService,
			IAclService aclService,
			IStoreMappingService storeMappingService,
			ICatalogSearchService catalogSearchService,
			MediaSettings mediaSettings, 
			CatalogSettings catalogSettings,
			IFilterService filterService,
 			CatalogHelper helper)
        {
			_services = services;
			_categoryService = categoryService;
            _manufacturerService = manufacturerService;
            _productService = productService;
            _categoryTemplateService = categoryTemplateService;
            _manufacturerTemplateService = manufacturerTemplateService;
            _currencyService = currencyService;
			_orderReportService = orderReportService;
			_productTagService = productTagService;
			_recentlyViewedProductsService = recentlyViewedProductsService;
			_compareProductsService = compareProductsService;
            _pictureService = pictureService;
            _priceFormatter = priceFormatter;
            _specificationAttributeService = specificationAttributeService;
            _genericAttributeService = genericAttributeService;
            _aclService = aclService;
			_storeMappingService = storeMappingService;
			_catalogSearchService = catalogSearchService;
            _filterService = filterService;
            _mediaSettings = mediaSettings;
            _catalogSettings = catalogSettings;

			_helper = helper;
        }

        #region Categories

        [RequireHttpsByConfigAttribute(SslRequirement.No)]
        public ActionResult Category(int categoryId, CatalogSearchQuery query)
        {
			var category = _categoryService.GetCategoryById(categoryId);
            if (category == null || category.Deleted)
				return HttpNotFound();

            // Check whether the current user has a "Manage catalog" permission
            // It allows him to preview a category before publishing
            if (!category.Published && !_services.Permissions.Authorize(StandardPermissionProvider.ManageCatalog))
				return HttpNotFound();

            // ACL (access control list)
            if (!_aclService.Authorize(category))
				return HttpNotFound();

			// Store mapping
			if (!_storeMappingService.Authorize(category))
				return HttpNotFound();            

            // 'Continue shopping' URL
			_genericAttributeService.SaveAttribute(_services.WorkContext.CurrentCustomer,
				SystemCustomerAttributeNames.LastContinueShoppingPage,
				_services.WebHelper.GetThisPageUrl(false),
				_services.StoreContext.CurrentStore.Id);

            var model = category.ToModel();

			_services.DisplayControl.Announce(category);

            //category breadcrumb
            model.DisplayCategoryBreadcrumb = _catalogSettings.CategoryBreadcrumbEnabled;
            if (model.DisplayCategoryBreadcrumb)
            {
				model.CategoryBreadcrumb = _helper.GetCategoryBreadCrumb(category.Id, 0);
            }

			model.DisplayFilter = _catalogSettings.FilterEnabled;
			model.SubCategoryDisplayType = _catalogSettings.SubCategoryDisplayType;

			var customerRolesIds = _services.WorkContext.CurrentCustomer.CustomerRoles.Where(x => x.Active).Select(x => x.Id).ToList();

            // subcategories
            model.SubCategories = _categoryService
                .GetAllCategoriesByParentCategoryId(categoryId)
                .Select(x =>
                {
                    var subCatName = x.GetLocalized(y => y.Name);
                    var subCatModel = new CategoryModel.SubCategoryModel
                    {
                        Id = x.Id,
                        Name = subCatName,
                        SeName = x.GetSeName(),
                    };

					_services.DisplayControl.Announce(x);
					
                    // prepare picture model
                    int pictureSize = _mediaSettings.CategoryThumbPictureSize;
					var categoryPictureCacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_PICTURE_MODEL_KEY, x.Id, pictureSize, true, _services.WorkContext.WorkingLanguage.Id, _services.StoreContext.CurrentStore.Id);
                    subCatModel.PictureModel = _services.Cache.Get(categoryPictureCacheKey, () =>
                    {
						var picture = _pictureService.GetPictureById(x.PictureId.GetValueOrDefault());
						var pictureModel = new PictureModel
                        {
							PictureId = x.PictureId.GetValueOrDefault(),
							Size = pictureSize,
							FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
							ImageUrl = _pictureService.GetPictureUrl(picture, pictureSize, !_catalogSettings.HideCategoryDefaultPictures),
                            Title = string.Format(T("Media.Category.ImageLinkTitleFormat"), subCatName),
                            AlternateText = string.Format(T("Media.Category.ImageAlternateTextFormat"), subCatName)
                        };

                        return pictureModel;
                    }, TimeSpan.FromHours(6));

                    return subCatModel;
                })
                .ToList();

			// Featured Products
			if (!_catalogSettings.IgnoreFeaturedProducts)
			{
				CatalogSearchResult featuredProductsResult = null;

				string cacheKey = ModelCacheEventConsumer.CATEGORY_HAS_FEATURED_PRODUCTS_KEY.FormatInvariant(categoryId, string.Join(",", customerRolesIds), _services.StoreContext.CurrentStore.Id);
				var hasFeaturedProductsCache = _services.Cache.Get<bool?>(cacheKey);

				var featuredProductsQuery = new CatalogSearchQuery()
					.WithCategoryIds(true, new int[] { categoryId })
					.VisibleIndividuallyOnly(true)
					.HasStoreId(_services.StoreContext.CurrentStore.Id)
					.WithLanguage(_services.WorkContext.WorkingLanguage);

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

			// Products
			int[] catIds = new int[] { categoryId };
			if (_catalogSettings.ShowProductsFromSubcategories)
			{
				// Include subcategories
				catIds = catIds.Concat(_helper.GetChildCategoryIds(categoryId)).ToArray();
			}

			query.WithCategoryIds(_catalogSettings.IncludeFeaturedProductsInNormalLists ? null : (bool?)false, catIds);

			var productsResult = _catalogSearchService.Search(query);

			var mappingSettings = _helper.GetBestFitProductSummaryMappingSettings(query.GetViewMode());
			model.Products = _helper.MapProductSummaryModel(productsResult.Hits, mappingSettings);

			// Prepare paging/sorting/mode stuff
			_helper.MapListActions(model.Products, category, _catalogSettings.DefaultPageSizeOptions);

			// template
			var templateCacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_TEMPLATE_MODEL_KEY, category.CategoryTemplateId);
			var templateViewPath = _services.Cache.Get(templateCacheKey, () =>
			{
				var template = _categoryTemplateService.GetCategoryTemplateById(category.CategoryTemplateId);
				if (template == null)
					template = _categoryTemplateService.GetAllCategoryTemplates().FirstOrDefault();
				return template.ViewPath;
			});

			// Activity log
			_services.CustomerActivity.InsertActivity("PublicStore.ViewCategory", T("ActivityLog.PublicStore.ViewCategory"), category.Name);

			return View(templateViewPath, model);
		}

        [ChildActionOnly]
        public ActionResult CategoryNavigation(int currentCategoryId, int currentProductId)
        {
			var model = _helper.PrepareCategoryNavigationModel(currentCategoryId, currentProductId);
            return PartialView(model);
        }

        //[ChildActionOnly]
        public ActionResult Megamenu(int currentCategoryId, int currentProductId)
        {
			var model = _helper.PrepareCategoryNavigationModel(currentCategoryId, currentProductId);
            return PartialView(model);
        }

		[ChildActionOnly]
		public ActionResult ProductBreadcrumb(int productId)
		{
			var product = _productService.GetProductById(productId);
			if (product == null)
				throw new ArgumentException(T("Products.NotFound", productId));

			if (!_catalogSettings.CategoryBreadcrumbEnabled)
				return Content("");

			var model = new ProductDetailsModel.ProductBreadcrumbModel
			{
				ProductId = product.Id,
				ProductName = product.GetLocalized(x => x.Name),
				ProductSeName = product.GetSeName()
			};

			var breadcrumb = _helper.GetCategoryBreadCrumb(0, productId);
			model.CategoryBreadcrumb = breadcrumb;

			return PartialView(model);
		}

        [ChildActionOnly]
        public ActionResult HomepageCategories()
        {
			var categories = _categoryService.GetAllCategoriesDisplayedOnHomePage()
				.Where(c => _aclService.Authorize(c) && _storeMappingService.Authorize(c))
				.ToList();

            var listModel = categories
                .Select(x =>
                {
                    var catModel = x.ToModel();

                    //prepare picture model
                    int pictureSize = _mediaSettings.CategoryThumbPictureSize;
					var categoryPictureCacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_PICTURE_MODEL_KEY, x.Id, pictureSize, true, 
						_services.WorkContext.WorkingLanguage.Id, 
						_services.StoreContext.CurrentStore.Id);
                    catModel.PictureModel = _services.Cache.Get(categoryPictureCacheKey, () =>
                    {
                        var pictureModel = new PictureModel
                        {
							PictureId = x.PictureId.GetValueOrDefault(),
							Size = pictureSize,
							FullSizeImageUrl = _pictureService.GetPictureUrl(x.PictureId.GetValueOrDefault()),
							ImageUrl = _pictureService.GetPictureUrl(x.PictureId.GetValueOrDefault(), pictureSize, !_catalogSettings.HideCategoryDefaultPictures),
                            Title = string.Format(T("Media.Category.ImageLinkTitleFormat"), catModel.Name),
							AlternateText = string.Format(T("Media.Category.ImageAlternateTextFormat"), catModel.Name)
                        };
                        return pictureModel;
                    }, TimeSpan.FromHours(6));

                    return catModel;
                })
                .ToList();

			if (listModel.Count == 0)
				return Content("");

			_services.DisplayControl.AnnounceRange(categories);

            return PartialView(listModel);
        }

        #endregion

        #region Manufacturers

        [RequireHttpsByConfigAttribute(SslRequirement.No)]
        public ActionResult Manufacturer(int manufacturerId, CatalogSearchQuery query)
        {
            var manufacturer = _manufacturerService.GetManufacturerById(manufacturerId);
            if (manufacturer == null || manufacturer.Deleted)
				return HttpNotFound();

            // Check whether the current user has a "Manage catalog" permission
            // It allows him to preview a manufacturer before publishing
            if (!manufacturer.Published && !_services.Permissions.Authorize(StandardPermissionProvider.ManageCatalog))
				return HttpNotFound();

			//Store mapping
			if (!_storeMappingService.Authorize(manufacturer))
				return HttpNotFound();

            //'Continue shopping' URL
			_genericAttributeService.SaveAttribute(_services.WorkContext.CurrentCustomer,
				SystemCustomerAttributeNames.LastContinueShoppingPage,
				_services.WebHelper.GetThisPageUrl(false),
				_services.StoreContext.CurrentStore.Id);

            var model = manufacturer.ToModel();

            // prepare picture model
            model.PictureModel = _helper.PrepareManufacturerPictureModel(manufacturer, model.Name);

			var customerRolesIds = _services.WorkContext.CurrentCustomer.CustomerRoles.Where(x => x.Active).Select(x => x.Id).ToList();

			// Featured products
			if (!_catalogSettings.IgnoreFeaturedProducts)
			{
				CatalogSearchResult featuredProductsResult = null;

				string cacheKey = ModelCacheEventConsumer.MANUFACTURER_HAS_FEATURED_PRODUCTS_KEY.FormatInvariant(manufacturerId, string.Join(",", customerRolesIds), _services.StoreContext.CurrentStore.Id);
				var hasFeaturedProductsCache = _services.Cache.Get<bool?>(cacheKey);

				var featuredProductsQuery = new CatalogSearchQuery()
					.WithManufacturerIds(true, new int[] { manufacturerId })
					.VisibleIndividuallyOnly(true)
					.HasStoreId(_services.StoreContext.CurrentStore.Id)
					.WithLanguage(_services.WorkContext.WorkingLanguage);

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
			query.WithManufacturerIds(_catalogSettings.IncludeFeaturedProductsInNormalLists ? null : (bool?)false, new int[] { manufacturerId });

			var productsResult = _catalogSearchService.Search(query);

			var mappingSettings = _helper.GetBestFitProductSummaryMappingSettings(query.GetViewMode());
			model.Products = _helper.MapProductSummaryModel(productsResult.Hits, mappingSettings);

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

        [RequireHttpsByConfigAttribute(SslRequirement.No)]
        public ActionResult ManufacturerAll()
        {
            var model = new List<ManufacturerModel>();
            var manufacturers = _manufacturerService.GetAllManufacturers(null, _services.StoreContext.CurrentStore.Id);
            foreach (var manufacturer in manufacturers)
            {
                var modelMan = manufacturer.ToModel();

                // prepare picture model
                modelMan.PictureModel = _helper.PrepareManufacturerPictureModel(manufacturer, modelMan.Name);
                model.Add(modelMan);
            }

			_services.DisplayControl.AnnounceRange(manufacturers);

            return View(model);
        }

        [ChildActionOnly]
        public ActionResult ManufacturerNavigation(int currentManufacturerId)
        {
			if (_catalogSettings.ManufacturersBlockItemsToDisplay == 0 || _catalogSettings.ShowManufacturersOnHomepage == false)
				return Content("");

			var cacheKey = string.Format(ModelCacheEventConsumer.MANUFACTURER_NAVIGATION_MODEL_KEY,
				currentManufacturerId,
				!_catalogSettings.HideManufacturerDefaultPictures,
				_services.WorkContext.WorkingLanguage.Id,
				_services.StoreContext.CurrentStore.Id);

            var cacheModel = _services.Cache.Get(cacheKey, () =>
            {
                var currentManufacturer = _manufacturerService.GetManufacturerById(currentManufacturerId);

                var manufacturers = _manufacturerService.GetAllManufacturers(null, _services.StoreContext.CurrentStore.Id);

                var model = new ManufacturerNavigationModel
                {
                    TotalManufacturers = manufacturers.Count,
                    DisplayManufacturers = _catalogSettings.ShowManufacturersOnHomepage,
                    DisplayImages = _catalogSettings.ShowManufacturerPictures
                };

                foreach (var manufacturer in manufacturers.Take(_catalogSettings.ManufacturersBlockItemsToDisplay))
                {
                    var modelMan = new ManufacturerBriefInfoModel
                    {
                        Id = manufacturer.Id,
                        Name = manufacturer.GetLocalized(x => x.Name),
                        SeName = manufacturer.GetSeName(),
                        PictureUrl = _pictureService.GetPictureUrl(manufacturer.PictureId.GetValueOrDefault(), _mediaSettings.ManufacturerThumbPictureSize, !_catalogSettings.HideManufacturerDefaultPictures),
                        IsActive = currentManufacturer != null && currentManufacturer.Id == manufacturer.Id,
                    };
                    model.Manufacturers.Add(modelMan);
                }
                return model;
            }, TimeSpan.FromHours(6));

			if (cacheModel.Manufacturers.Count == 0)
				return Content("");

            return PartialView(cacheModel);
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

			return PartialView(model);
		}

		#endregion

		#region Products by Tag

		[ChildActionOnly]
		public ActionResult PopularProductTags()
		{
			var cacheKey = string.Format(ModelCacheEventConsumer.PRODUCTTAG_POPULAR_MODEL_KEY, _services.WorkContext.WorkingLanguage.Id, _services.StoreContext.CurrentStore.Id);
			var cacheModel = _services.Cache.Get(cacheKey, () =>
			{
				var model = new PopularProductTagsModel();

				//get all tags
				var allTags = _productTagService
					.GetAllProductTags()
					//filter by current store
					.Where(x => _productTagService.GetProductCount(x.Id, _services.StoreContext.CurrentStore.Id) > 0)
					//order by product count
					.OrderByDescending(x => _productTagService.GetProductCount(x.Id, _services.StoreContext.CurrentStore.Id))
					.ToList();

				var tags = allTags
					.Take(_catalogSettings.NumberOfProductTags)
					.ToList();
				//sorting
				tags = tags.OrderBy(x => x.GetLocalized(y => y.Name)).ToList();

				model.TotalTags = allTags.Count;

				foreach (var tag in tags)
					model.Tags.Add(new ProductTagModel
					{
						Id = tag.Id,
						Name = tag.GetLocalized(y => y.Name),
						SeName = tag.GetSeName(),
						ProductCount = _productTagService.GetProductCount(tag.Id, _services.StoreContext.CurrentStore.Id)
					});
				return model;
			});

			return PartialView(cacheModel);
		}

		[RequireHttpsByConfigAttribute(SslRequirement.No)]
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

			var productsResult = _catalogSearchService.Search(query);

			var mappingSettings = _helper.GetBestFitProductSummaryMappingSettings(query.GetViewMode());
			model.Products = _helper.MapProductSummaryModel(productsResult.Hits, mappingSettings);

			// Prepare paging/sorting/mode stuff
			_helper.MapListActions(model.Products, null, _catalogSettings.DefaultPageSizeOptions);

			return View(model);
		}

		[RequireHttpsByConfigAttribute(SslRequirement.No)]
		public ActionResult ProductTagsAll()
		{
			var model = new PopularProductTagsModel();
			model.Tags = _productTagService
				.GetAllProductTags()
				//filter by current store
				.Where(x => _productTagService.GetProductCount(x.Id, _services.StoreContext.CurrentStore.Id) > 0)
				//sort by name
				.OrderBy(x => x.GetLocalized(y => y.Name))
				.Select(x =>
				{
					var ptModel = new ProductTagModel
					{
						Id = x.Id,
						Name = x.GetLocalized(y => y.Name),
						SeName = x.GetSeName(),
						ProductCount = _productTagService.GetProductCount(x.Id, _services.StoreContext.CurrentStore.Id)
					};
					return ptModel;
				})
				.ToList();
			return View(model);
		}

		#endregion

		#region Recently[...]Products

		[RequireHttpsByConfigAttribute(SslRequirement.No)]
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
				x.MapManufacturers = true;
			});

			var model = _helper.MapProductSummaryModel(products, settings);

			return PartialView(model);
		}

		[RequireHttpsByConfigAttribute(SslRequirement.No)]
		public ActionResult RecentlyAddedProducts(CatalogSearchQuery query)
		{
			if (!_catalogSettings.RecentlyAddedProductsEnabled || _catalogSettings.RecentlyAddedProductsNumber <= 0)
			{
				return View(ProductSummaryModel.Empty);
			}

			query = query.SortBy(ProductSortingEnum.CreatedOn).Slice(0, _catalogSettings.RecentlyAddedProductsNumber);
			var result = _catalogSearchService.Search(query);

			var settings = _helper.GetBestFitProductSummaryMappingSettings(query.GetViewMode());
			var model = _helper.MapProductSummaryModel(result.Hits, settings);

			return View(model);
		}

		[Compress]
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

			query = query.SortBy(ProductSortingEnum.CreatedOn).Slice(0, _catalogSettings.RecentlyAddedProductsNumber);
			var result = _catalogSearchService.Search(query);

			var storeUrl = _services.StoreContext.CurrentStore.Url;

			foreach (var product in result.Hits)
			{
				string productUrl = Url.RouteUrl("Product", new { SeName = product.GetSeName() }, "http");
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
						var picture = product.ProductPictures.OrderBy(x => x.DisplayOrder).Select(x => x.Picture).FirstOrDefault();

						if (picture != null)
						{
							feed.AddEnclosue(item, picture, _pictureService.GetPictureUrl(picture, _mediaSettings.ProductDetailsPictureSize, false, storeUrl));
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
			if (product == null || product.Deleted || !product.Published)
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
			if (product == null || product.Deleted || !product.Published || !_catalogSettings.CompareProductsEnabled)
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

		[RequireHttpsByConfigAttribute(SslRequirement.No)]
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

		[ChildActionOnly]
		public ActionResult CompareProductsButton(int productId)
		{
			if (!_catalogSettings.CompareProductsEnabled)
				return Content("");

			var model = new AddToCompareListModel()
			{
				ProductId = productId
			};

			return PartialView("CompareProductsButton", model);
		}

		public ActionResult CompareSummary()
		{
			return Json(new
			{
				Count = _compareProductsService.GetComparedProducts().Count
			},
			JsonRequestBehavior.AllowGet);
		}

		#endregion
    }
}
