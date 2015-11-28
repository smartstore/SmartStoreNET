using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Localization;
using SmartStore.Services;
using SmartStore.Services.Catalog;
using SmartStore.Services.Common;
using SmartStore.Services.Directory;
using SmartStore.Services.Filter;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Orders;
using SmartStore.Services.Security;
using SmartStore.Services.Seo;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Mvc;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.UI;
using SmartStore.Web.Infrastructure.Cache;
using SmartStore.Web.Models.Catalog;
using SmartStore.Web.Models.Media;

namespace SmartStore.Web.Controllers
{
    public partial class CatalogController : PublicControllerBase
    {
        #region Fields

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
        private readonly MediaSettings _mediaSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly IFilterService _filterService;
		private readonly ICompareProductsService _compareProductsService;
		private readonly CatalogHelper _helper;

        #endregion

        #region Constructors

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
            MediaSettings mediaSettings, 
			CatalogSettings catalogSettings,
			IFilterService filterService,
 			CatalogHelper helper)
        {
			this._services = services;
			this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._productService = productService;
            this._categoryTemplateService = categoryTemplateService;
            this._manufacturerTemplateService = manufacturerTemplateService;
            this._currencyService = currencyService;
			this._orderReportService = orderReportService;
			this._productTagService = productTagService;
			this._recentlyViewedProductsService = recentlyViewedProductsService;
			this._compareProductsService = compareProductsService;
            this._pictureService = pictureService;
            this._priceFormatter = priceFormatter;
            this._specificationAttributeService = specificationAttributeService;
            this._genericAttributeService = genericAttributeService;
            this._aclService = aclService;
			this._storeMappingService = storeMappingService;
            this._filterService = filterService;
            this._mediaSettings = mediaSettings;
            this._catalogSettings = catalogSettings;

			this._helper = helper;
        }

        #endregion

        #region Categories

        [RequireHttpsByConfigAttribute(SslRequirement.No)]
        public ActionResult Category(int categoryId, CatalogPagingFilteringModel command, string filter)
        {
			var category = _categoryService.GetCategoryById(categoryId);
            if (category == null || category.Deleted)
				return HttpNotFound();

            //Check whether the current user has a "Manage catalog" permission
            //It allows him to preview a category before publishing
            if (!category.Published && !_services.Permissions.Authorize(StandardPermissionProvider.ManageCatalog))
				return HttpNotFound();

            //ACL (access control list)
            if (!_aclService.Authorize(category))
				return HttpNotFound();

			//Store mapping
			if (!_storeMappingService.Authorize(category))
				return HttpNotFound();            

            //'Continue shopping' URL
			_genericAttributeService.SaveAttribute(_services.WorkContext.CurrentCustomer,
				SystemCustomerAttributeNames.LastContinueShoppingPage,
				_services.WebHelper.GetThisPageUrl(false),
				_services.StoreContext.CurrentStore.Id);

            if (command.PageNumber <= 0)
                command.PageNumber = 1;

            if (command.ViewMode.IsEmpty() && category.DefaultViewMode.HasValue())
            {
                command.ViewMode = category.DefaultViewMode;
            }

            if (command.OrderBy == (int)ProductSortingEnum.Initial)
            {
                command.OrderBy = (int)_catalogSettings.DefaultSortOrder;
            }
            
            var model = category.ToModel();

			_helper.PreparePagingFilteringModel(model.PagingFilteringContext, command, new PageSizeContext
            {
                AllowCustomersToSelectPageSize = category.AllowCustomersToSelectPageSize,
                PageSize = category.PageSize,
                PageSizeOptions = category.PageSizeOptions
            });

            //price ranges
            model.PagingFilteringContext.PriceRangeFilter.LoadPriceRangeFilters(category.PriceRanges, _services.WebHelper, _priceFormatter);
            var selectedPriceRange = model.PagingFilteringContext.PriceRangeFilter.GetSelectedPriceRange(_services.WebHelper, category.PriceRanges);
            decimal? minPriceConverted = null;
            decimal? maxPriceConverted = null;

            if (selectedPriceRange != null)
            {
                if (selectedPriceRange.From.HasValue)
                    minPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(selectedPriceRange.From.Value, _services.WorkContext.WorkingCurrency);

                if (selectedPriceRange.To.HasValue)
                    maxPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(selectedPriceRange.To.Value, _services.WorkContext.WorkingCurrency);
            }

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
					
                    //prepare picture model
                    int pictureSize = _mediaSettings.CategoryThumbPictureSize;
					var categoryPictureCacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_PICTURE_MODEL_KEY, x.Id, pictureSize, true, _services.WorkContext.WorkingLanguage.Id, _services.WebHelper.IsCurrentConnectionSecured(), _services.StoreContext.CurrentStore.Id);
                    subCatModel.PictureModel = _services.Cache.Get(categoryPictureCacheKey, () =>
                    {
						var picture = _pictureService.GetPictureById(x.PictureId.GetValueOrDefault());
						var pictureModel = new PictureModel()
                        {
							PictureId = x.PictureId.GetValueOrDefault(),
							FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
							ImageUrl = _pictureService.GetPictureUrl(picture, targetSize: pictureSize),
                            Title = string.Format(T("Media.Category.ImageLinkTitleFormat"), subCatName),
                            AlternateText = string.Format(T("Media.Category.ImageAlternateTextFormat"), subCatName)
                        };
                        return pictureModel;
                    });

                    return subCatModel;
                })
                .ToList();


            // Featured products
            if (!_catalogSettings.IgnoreFeaturedProducts)
            {
				IPagedList<Product> featuredProducts = null;
				
				string cacheKey = ModelCacheEventConsumer.CATEGORY_HAS_FEATURED_PRODUCTS_KEY.FormatInvariant(categoryId, string.Join(",", customerRolesIds), _services.StoreContext.CurrentStore.Id);
				var hasFeaturedProductsCache = _services.Cache.Get<bool?>(cacheKey);

				var ctx = new ProductSearchContext();
				if (category.Id > 0)
					ctx.CategoryIds.Add(category.Id);
				ctx.FeaturedProducts = true;
				ctx.LanguageId = _services.WorkContext.WorkingLanguage.Id;
				ctx.OrderBy = ProductSortingEnum.Position;
				ctx.PageSize = int.MaxValue;
				ctx.StoreId = _services.StoreContext.CurrentStoreIdIfMultiStoreMode;
				ctx.VisibleIndividuallyOnly = true;
				ctx.Origin = categoryId.ToString();

				if (!hasFeaturedProductsCache.HasValue)
				{
					featuredProducts = _productService.SearchProducts(ctx);
					hasFeaturedProductsCache = featuredProducts.TotalCount > 0;
					_services.Cache.Set(cacheKey, hasFeaturedProductsCache, 240);
				}

				if (hasFeaturedProductsCache.Value && featuredProducts == null)
				{
					featuredProducts = _productService.SearchProducts(ctx);
				}

				if (featuredProducts != null)
				{
					model.FeaturedProducts = _helper.PrepareProductOverviewModels(
						featuredProducts, 
						prepareColorAttributes: true).ToList();
				}
            }

            // Products
            if (filter.HasValue())
            {
                var context = new FilterProductContext
                {
                    ParentCategoryID = category.Id,
                    CategoryIds = new List<int> { category.Id },
                    Criteria = _filterService.Deserialize(filter),
                    OrderBy = command.OrderBy
                };

                if (_catalogSettings.ShowProductsFromSubcategories)
					context.CategoryIds.AddRange(_helper.GetChildCategoryIds(category.Id));

                var filterQuery = _filterService.ProductFilter(context);
                var products = new PagedList<Product>(filterQuery, command.PageIndex, command.PageSize);

				model.Products = _helper.PrepareProductOverviewModels(
					products, 
					prepareColorAttributes: true,
					prepareManufacturers: command.ViewMode.IsCaseInsensitiveEqual("list")).ToList();
                model.PagingFilteringContext.LoadPagedList(products);
            }
            else
            {	// use old filter
                IList<int> alreadyFilteredSpecOptionIds = model.PagingFilteringContext.SpecificationFilter.GetAlreadyFilteredSpecOptionIds(_services.WebHelper);

                var ctx2 = new ProductSearchContext();
                if (category.Id > 0)
                {
                    ctx2.CategoryIds.Add(category.Id);
                    if (_catalogSettings.ShowProductsFromSubcategories)
                    {
                        // include subcategories
						ctx2.CategoryIds.AddRange(_helper.GetChildCategoryIds(category.Id));
                    }
                }
                ctx2.FeaturedProducts = _catalogSettings.IncludeFeaturedProductsInNormalLists ? null : (bool?)false;
                ctx2.PriceMin = minPriceConverted;
                ctx2.PriceMax = maxPriceConverted;
                ctx2.LanguageId = _services.WorkContext.WorkingLanguage.Id;
                ctx2.FilteredSpecs = alreadyFilteredSpecOptionIds;
                ctx2.OrderBy = (ProductSortingEnum)command.OrderBy; // ProductSortingEnum.Position;
                ctx2.PageIndex = command.PageNumber - 1;
                ctx2.PageSize = command.PageSize;
                ctx2.LoadFilterableSpecificationAttributeOptionIds = true;
				ctx2.StoreId = _services.StoreContext.CurrentStoreIdIfMultiStoreMode;
				ctx2.VisibleIndividuallyOnly = true;
                ctx2.Origin = categoryId.ToString();

                var products = _productService.SearchProducts(ctx2);

				model.Products = _helper.PrepareProductOverviewModels(
					products, 
					prepareColorAttributes: true,
					prepareManufacturers: command.ViewMode.IsCaseInsensitiveEqual("list")).ToList();

                model.PagingFilteringContext.LoadPagedList(products);
                //model.PagingFilteringContext.ViewMode = viewMode;

                //specs
                model.PagingFilteringContext.SpecificationFilter.PrepareSpecsFilters(alreadyFilteredSpecOptionIds,
                    ctx2.FilterableSpecificationAttributeOptionIds,
                    _specificationAttributeService, _services.WebHelper, _services.WorkContext);
            }

            // template
            var templateCacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_TEMPLATE_MODEL_KEY, category.CategoryTemplateId);
            var templateViewPath = _services.Cache.Get(templateCacheKey, () =>
            {
                var template = _categoryTemplateService.GetCategoryTemplateById(category.CategoryTemplateId);
                if (template == null)
                    template = _categoryTemplateService.GetAllCategoryTemplates().FirstOrDefault();
                return template.ViewPath;
            });

            // activity log
			_services.CustomerActivity.InsertActivity("PublicStore.ViewCategory", T("ActivityLog.PublicStore.ViewCategory"), category.Name);

            return View(templateViewPath, model);
        }

        [ChildActionOnly]
        public ActionResult CategoryNavigation(int currentCategoryId, int currentProductId)
        {
			var model = _helper.PrepareCategoryNavigationModel(currentCategoryId, currentProductId);
            return PartialView(model);
        }

        [ChildActionOnly]
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
				throw new ArgumentException("No product found with the specified id");

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
						_services.WorkContext.WorkingLanguage.Id, _services.WebHelper.IsCurrentConnectionSecured(), _services.StoreContext.CurrentStore.Id);
                    catModel.PictureModel = _services.Cache.Get(categoryPictureCacheKey, () =>
                    {
                        var pictureModel = new PictureModel()
                        {
							PictureId = x.PictureId.GetValueOrDefault(),
							FullSizeImageUrl = _pictureService.GetPictureUrl(x.PictureId.GetValueOrDefault()),
							ImageUrl = _pictureService.GetPictureUrl(x.PictureId.GetValueOrDefault(), pictureSize),
                            Title = string.Format(T("Media.Category.ImageLinkTitleFormat"), catModel.Name),
							AlternateText = string.Format(T("Media.Category.ImageAlternateTextFormat"), catModel.Name)
                        };
                        return pictureModel;
                    });

                    return catModel;
                })
                .ToList();

			if (listModel.Count == 0)
				return Content("");

            return PartialView(listModel);
        }

        #endregion

        #region Manufacturers

        [RequireHttpsByConfigAttribute(SslRequirement.No)]
        public ActionResult Manufacturer(int manufacturerId, CatalogPagingFilteringModel command)
        {
            var manufacturer = _manufacturerService.GetManufacturerById(manufacturerId);
            if (manufacturer == null || manufacturer.Deleted)
				return HttpNotFound();

            //Check whether the current user has a "Manage catalog" permission
            //It allows him to preview a manufacturer before publishing
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

            if (command.PageNumber <= 0)
                command.PageNumber = 1;

            var model = manufacturer.ToModel();

            // prepare picture model
            model.PictureModel = _helper.PrepareManufacturerPictureModel(manufacturer, model.Name);

            if (command.OrderBy == (int)ProductSortingEnum.Initial)
            {
                command.OrderBy = (int)_catalogSettings.DefaultSortOrder;
            }

			_helper.PreparePagingFilteringModel(model.PagingFilteringContext, command, new PageSizeContext
            {
                AllowCustomersToSelectPageSize = manufacturer.AllowCustomersToSelectPageSize,
                PageSize = manufacturer.PageSize,
                PageSizeOptions = manufacturer.PageSizeOptions
            });

            //price ranges
            model.PagingFilteringContext.PriceRangeFilter.LoadPriceRangeFilters(manufacturer.PriceRanges, _services.WebHelper, _priceFormatter);
            var selectedPriceRange = model.PagingFilteringContext.PriceRangeFilter.GetSelectedPriceRange(_services.WebHelper, manufacturer.PriceRanges);
            decimal? minPriceConverted = null;
            decimal? maxPriceConverted = null;
            if (selectedPriceRange != null)
            {
                if (selectedPriceRange.From.HasValue)
                    minPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(selectedPriceRange.From.Value, _services.WorkContext.WorkingCurrency);

                if (selectedPriceRange.To.HasValue)
                    maxPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(selectedPriceRange.To.Value, _services.WorkContext.WorkingCurrency);
            }

			var customerRolesIds = _services.WorkContext.CurrentCustomer.CustomerRoles.Where(x => x.Active).Select(x => x.Id).ToList();

			// Featured products
			if (!_catalogSettings.IgnoreFeaturedProducts)
			{
				IPagedList<Product> featuredProducts = null;

				string cacheKey = ModelCacheEventConsumer.MANUFACTURER_HAS_FEATURED_PRODUCTS_KEY.FormatInvariant(manufacturerId, string.Join(",", customerRolesIds), _services.StoreContext.CurrentStore.Id);
				var hasFeaturedProductsCache = _services.Cache.Get<bool?>(cacheKey);

				var ctx = new ProductSearchContext();
				ctx.ManufacturerId = manufacturer.Id;
				ctx.FeaturedProducts = true;
				ctx.LanguageId = _services.WorkContext.WorkingLanguage.Id;
				ctx.OrderBy = ProductSortingEnum.Position;
				ctx.PageSize = int.MaxValue;
				ctx.StoreId = _services.StoreContext.CurrentStoreIdIfMultiStoreMode;
				ctx.VisibleIndividuallyOnly = true;

				if (!hasFeaturedProductsCache.HasValue)
				{
					featuredProducts = _productService.SearchProducts(ctx);
					hasFeaturedProductsCache = featuredProducts.TotalCount > 0;
					_services.Cache.Set(cacheKey, hasFeaturedProductsCache, 240);
				}

				if (hasFeaturedProductsCache.Value && featuredProducts == null)
				{
					featuredProducts = _productService.SearchProducts(ctx);
				}

				if (featuredProducts != null)
				{
					model.FeaturedProducts = _helper.PrepareProductOverviewModels(featuredProducts, prepareColorAttributes: true).ToList();
				}
			}

            //products
            var ctx2 = new ProductSearchContext();
            ctx2.ManufacturerId = manufacturer.Id;
            ctx2.FeaturedProducts = _catalogSettings.IncludeFeaturedProductsInNormalLists ? null : (bool?)false;
            ctx2.PriceMin = minPriceConverted;
            ctx2.PriceMax = maxPriceConverted;
            ctx2.LanguageId = _services.WorkContext.WorkingLanguage.Id;
            ctx2.OrderBy = (ProductSortingEnum)command.OrderBy;
            ctx2.PageIndex = command.PageNumber - 1;
            ctx2.PageSize = command.PageSize;
			ctx2.StoreId = _services.StoreContext.CurrentStoreIdIfMultiStoreMode;
			ctx2.VisibleIndividuallyOnly = true;

            var products = _productService.SearchProducts(ctx2);

			model.Products = _helper.PrepareProductOverviewModels(
				products, 
				prepareColorAttributes: false,
				prepareManufacturers: command.ViewMode.IsCaseInsensitiveEqual("list")).ToList();

            model.PagingFilteringContext.LoadPagedList(products);
            //model.PagingFilteringContext.ViewMode = viewMode;


            //template
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

            return View(templateViewPath, model);
        }

        [RequireHttpsByConfigAttribute(SslRequirement.No)]
        public ActionResult ManufacturerAll()
        {
            var model = new List<ManufacturerModel>();
            var manufacturers = _manufacturerService.GetAllManufacturers();
            foreach (var manufacturer in manufacturers)
            {
                var modelMan = manufacturer.ToModel();

                // prepare picture model
                modelMan.PictureModel = _helper.PrepareManufacturerPictureModel(manufacturer, modelMan.Name);
                model.Add(modelMan);
            }

            return View(model);
        }

        [ChildActionOnly]
        public ActionResult ManufacturerNavigation(int currentManufacturerId)
        {
			if (_catalogSettings.ManufacturersBlockItemsToDisplay == 0 || _catalogSettings.ShowManufacturersOnHomepage == false)
				return Content("");

			string cacheKey = string.Format(ModelCacheEventConsumer.MANUFACTURER_NAVIGATION_MODEL_KEY, currentManufacturerId, _services.WorkContext.WorkingLanguage.Id, _services.StoreContext.CurrentStore.Id);
            var cacheModel = _services.Cache.Get(cacheKey, () =>
            {
                var currentManufacturer = _manufacturerService.GetManufacturerById(currentManufacturerId);

                var manufacturers = _manufacturerService.GetAllManufacturers();
                var model = new ManufacturerNavigationModel()
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
                        PictureUrl = _pictureService.GetPictureUrl(manufacturer.PictureId.GetValueOrDefault(), _mediaSettings.ManufacturerThumbPictureSize),
                        IsActive = currentManufacturer != null && currentManufacturer.Id == manufacturer.Id,
                    };
                    model.Manufacturers.Add(modelMan);
                }
                return model;
            });

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
				return Content("");

			// load and cache report
			var report = _services.Cache.Get(string.Format(ModelCacheEventConsumer.HOMEPAGE_BESTSELLERS_IDS_KEY, _services.StoreContext.CurrentStore.Id), () => 
			{
				return _orderReportService.BestSellersReport(_services.StoreContext.CurrentStore.Id, null, null, null, null, null, 0, _catalogSettings.NumberOfBestsellersOnHomepage);
			});

			// load products
			var products = _productService.GetProductsByIds(report.Select(x => x.ProductId).ToArray());

			// ACL and store mapping
			products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();

			// prepare model
			var model = new HomePageBestsellersModel
			{
				UseSmallProductBox = _catalogSettings.UseSmallProductBoxOnHomePage,
				Products = _helper.PrepareProductOverviewModels(products, true, true, productThumbPictureSize).ToList()
			};

			return PartialView(model);
		}

		[ChildActionOnly]
		public ActionResult HomepageProducts(int? productThumbPictureSize)
		{
			var products = _productService.GetAllProductsDisplayedOnHomePage();
			//ACL and store mapping
			products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();

			var model = new HomePageProductsModel()
			{
				UseSmallProductBox = false, //_catalogSettings.UseSmallProductBoxOnHomePage,
				//Products = PrepareProductOverviewModels(products, 
				//    !_catalogSettings.UseSmallProductBoxOnHomePage, true, productThumbPictureSize)
				//    .ToList()
				Products = _helper.PrepareProductOverviewModels(products, true, true, productThumbPictureSize, prepareColorAttributes: true).ToList()
			};

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
					model.Tags.Add(new ProductTagModel()
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
		public ActionResult ProductsByTag(int productTagId, CatalogPagingFilteringModel command)
		{
			var productTag = _productTagService.GetProductTagById(productTagId);
			if (productTag == null)
				return HttpNotFound();

			if (command.PageNumber <= 0)
				command.PageNumber = 1;

			var model = new ProductsByTagModel()
			{
				Id = productTag.Id,
				TagName = productTag.GetLocalized(y => y.Name)
			};

            if (command.OrderBy == (int)ProductSortingEnum.Initial)
            {
                command.OrderBy = (int)_catalogSettings.DefaultSortOrder;
            }

			_helper.PreparePagingFilteringModel(model.PagingFilteringContext, command, new PageSizeContext
			{
				AllowCustomersToSelectPageSize = _catalogSettings.ProductsByTagAllowCustomersToSelectPageSize,
				PageSize = _catalogSettings.ProductsByTagPageSize,
				PageSizeOptions = _catalogSettings.ProductsByTagPageSizeOptions.IsEmpty()
					? _catalogSettings.DefaultPageSizeOptions
					: _catalogSettings.ProductsByTagPageSizeOptions
			});

			//products

			var ctx = new ProductSearchContext();
			ctx.ProductTagId = productTag.Id;
			ctx.LanguageId = _services.WorkContext.WorkingLanguage.Id;
			ctx.OrderBy = (ProductSortingEnum)command.OrderBy;
			ctx.PageIndex = command.PageNumber - 1;
			ctx.PageSize = command.PageSize;
			ctx.StoreId = _services.StoreContext.CurrentStoreIdIfMultiStoreMode;
			ctx.VisibleIndividuallyOnly = true;

			var products = _productService.SearchProducts(ctx);

			model.Products = _helper.PrepareProductOverviewModels(
				products, 
				prepareColorAttributes: true,
				prepareManufacturers: command.ViewMode.IsCaseInsensitiveEqual("list")).ToList();

			model.PagingFilteringContext.LoadPagedList(products);
			//model.PagingFilteringContext.ViewMode = viewMode;
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

		//recently viewed products
		[RequireHttpsByConfigAttribute(SslRequirement.No)]
		public ActionResult RecentlyViewedProducts()
		{
			var model = new List<ProductOverviewModel>();
			if (_catalogSettings.RecentlyViewedProductsEnabled)
			{
				var products = _recentlyViewedProductsService.GetRecentlyViewedProducts(_catalogSettings.RecentlyViewedProductsNumber);
				model.AddRange(_helper.PrepareProductOverviewModels(products));
			}
			return View(model);
		}

		[ChildActionOnly]
		public ActionResult RecentlyViewedProductsBlock(int? productThumbPictureSize)
		{
			var model = new List<ProductOverviewModel>();
			if (_catalogSettings.RecentlyViewedProductsEnabled)
			{
				var products = _recentlyViewedProductsService.GetRecentlyViewedProducts(_catalogSettings.RecentlyViewedProductsNumber);
				model.AddRange(_helper.PrepareProductOverviewModels(products, false, true, productThumbPictureSize));
			}
			return PartialView(model);
		}

		[RequireHttpsByConfigAttribute(SslRequirement.No)]
		public ActionResult RecentlyAddedProducts(CatalogPagingFilteringModel command)
		{
			var model = new RecentlyAddedProductsModel();

			if (_catalogSettings.RecentlyAddedProductsEnabled)
			{
				IList<int> filterableSpecificationAttributeOptionIds = null;

				var ctx = new ProductSearchContext();
				ctx.LanguageId = _services.WorkContext.WorkingLanguage.Id;
				//ctx.OrderBy = (ProductSortingEnum)command.OrderBy;
				ctx.OrderBy = ProductSortingEnum.CreatedOn;
				//ctx.PageSize = command.PageSize;
				ctx.PageSize = _catalogSettings.RecentlyAddedProductsNumber;
				//ctx.PageIndex = command.PageNumber - 1;
				ctx.FilterableSpecificationAttributeOptionIds = filterableSpecificationAttributeOptionIds;
				ctx.StoreId = _services.StoreContext.CurrentStoreIdIfMultiStoreMode;
				ctx.VisibleIndividuallyOnly = true;

				var products = _productService.SearchProducts(ctx);

				//var products = _productService.SearchProducts(ctx).Take(_catalogSettings.RecentlyAddedProductsNumber).OrderBy((ProductSortingEnum)command.OrderBy);

				model.Products.AddRange(_helper.PrepareProductOverviewModels(products));
				//model.PagingFilteringContext.LoadPagedList(products);
			}
			return View(model);
		}

		[Compress]
		public ActionResult RecentlyAddedProductsRss()
		{
			var feed = _productService.CreateRecentlyAddedProductsRssFeed(Url);

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
				return HttpNotFound();

			var model = new CompareProductsModel()
			{
				IncludeShortDescriptionInCompareProducts = _catalogSettings.IncludeShortDescriptionInCompareProducts,
				IncludeFullDescriptionInCompareProducts = _catalogSettings.IncludeFullDescriptionInCompareProducts,
			};

			var products = _compareProductsService.GetComparedProducts();

			_helper.PrepareProductOverviewModels(products, prepareSpecificationAttributes: true, prepareFullDescription: true)
				.ToList()
				.ForEach(model.Products.Add);

			return View(model);
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

		public ActionResult FlyoutCompare()
		{
			var model = new CompareProductsModel
			{
				IncludeShortDescriptionInCompareProducts = _catalogSettings.IncludeShortDescriptionInCompareProducts,
				IncludeFullDescriptionInCompareProducts = _catalogSettings.IncludeFullDescriptionInCompareProducts,
			};

			var products = _compareProductsService.GetComparedProducts();

			_helper.PrepareProductOverviewModels(products, prepareSpecificationAttributes: true)
				.ToList()
				.ForEach(model.Products.Add);

			return PartialView(model);
		}

		#endregion

		#region Searching

		[RequireHttpsByConfigAttribute(SslRequirement.No)]
		[ValidateInput(false)]
		public ActionResult Search(SearchModel model, SearchPagingFilteringModel command)
		{
			if (model == null)
				model = new SearchModel();

			// 'Continue shopping' URL
			_genericAttributeService.SaveAttribute(_services.WorkContext.CurrentCustomer,
				SystemCustomerAttributeNames.LastContinueShoppingPage,
				_services.WebHelper.GetThisPageUrl(false),
				_services.StoreContext.CurrentStore.Id);

			if (command.PageSize <= 0)
				command.PageSize = _catalogSettings.SearchPageProductsPerPage;
			if (command.PageNumber <= 0)
				command.PageNumber = 1;

            if (command.OrderBy == (int)ProductSortingEnum.Initial)
            {
                command.OrderBy = (int)_catalogSettings.DefaultSortOrder;
            }

			_helper.PreparePagingFilteringModel(model.PagingFilteringContext, command, new PageSizeContext
			{
				AllowCustomersToSelectPageSize = _catalogSettings.ProductSearchAllowCustomersToSelectPageSize,
				PageSize = _catalogSettings.SearchPageProductsPerPage,
				PageSizeOptions = _catalogSettings.ProductSearchPageSizeOptions
			});

			if (model.Q == null)
				model.Q = "";
			model.Q = model.Q.Trim();

			// Build AvailableCategories
			// first empty entry
			model.AvailableCategories.Add(new SelectListItem
			{
				Value = "0",
				Text = T("Common.All")
			});

			var navModel = _helper.PrepareCategoryNavigationModel(0, 0);

			navModel.Root.TraverseTree((node) =>
			{
				if (node.IsRoot)
					return;

				int id = node.Value.EntityId;
				var breadcrumb = node.GetBreadcrumb().Select(x => x.Text).ToArray();

				model.AvailableCategories.Add(new SelectListItem
				{
					Value = id.ToString(),
					Text = String.Join(" > ", breadcrumb),
					Selected = model.Cid == id
				});
			});

			var manufacturers = _manufacturerService.GetAllManufacturers();
			if (manufacturers.Count > 0)
			{
				model.AvailableManufacturers.Add(new SelectListItem
				{
					Value = "0",
					Text = T("Common.All")
				});
				foreach (var m in manufacturers)
					model.AvailableManufacturers.Add(new SelectListItem
					{
						Value = m.Id.ToString(),
						Text = m.GetLocalized(x => x.Name),
						Selected = model.Mid == m.Id
					});
			}

			IPagedList<Product> products = new PagedList<Product>(new List<Product>(), 0, 1);
			// only search if query string search keyword is set (used to avoid searching or displaying search term min length error message on /search page load)
			if (Request.Params["Q"] != null)
			{
				if (model.Q.Length < _catalogSettings.ProductSearchTermMinimumLength)
				{
					model.Warning = string.Format(T("Search.SearchTermMinimumLengthIsNCharacters"), _catalogSettings.ProductSearchTermMinimumLength);
				}
				else
				{
					var categoryIds = new List<int>();
					int manufacturerId = 0;
					decimal? minPriceConverted = null;
					decimal? maxPriceConverted = null;
					bool searchInDescriptions = false;
					if (model.As)
					{
						// advanced search
						var categoryId = model.Cid;
						if (categoryId > 0)
						{
							categoryIds.Add(categoryId);
							if (model.Isc)
							{
								// include subcategories
								categoryIds.AddRange(_helper.GetChildCategoryIds(categoryId));
							}
						}

						manufacturerId = model.Mid;

						// min price
						if (!string.IsNullOrEmpty(model.Pf))
						{
							decimal minPrice = decimal.Zero;
							if (decimal.TryParse(model.Pf, out minPrice))
								minPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(minPrice, _services.WorkContext.WorkingCurrency);
						}
						// max price
						if (!string.IsNullOrEmpty(model.Pt))
						{
							decimal maxPrice = decimal.Zero;
							if (decimal.TryParse(model.Pt, out maxPrice))
								maxPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(maxPrice, _services.WorkContext.WorkingCurrency);
						}

						searchInDescriptions = model.Sid;
					}

					//var searchInProductTags = false;
					var searchInProductTags = searchInDescriptions;

					//products

					var ctx = new ProductSearchContext();
					ctx.CategoryIds = categoryIds;
					ctx.ManufacturerId = manufacturerId;
					ctx.PriceMin = minPriceConverted;
					ctx.PriceMax = maxPriceConverted;
					ctx.Keywords = model.Q;
					ctx.SearchDescriptions = searchInDescriptions;
					ctx.SearchSku = !_catalogSettings.SuppressSkuSearch;
					ctx.SearchProductTags = searchInProductTags;
					ctx.LanguageId = _services.WorkContext.WorkingLanguage.Id;
					ctx.OrderBy = (ProductSortingEnum)command.OrderBy; // ProductSortingEnum.Position;
					ctx.PageIndex = command.PageNumber - 1;
					ctx.PageSize = command.PageSize;
					ctx.StoreId = _services.StoreContext.CurrentStoreIdIfMultiStoreMode;
					ctx.VisibleIndividuallyOnly = true;

					products = _productService.SearchProducts(ctx);

					model.Products = _helper.PrepareProductOverviewModels(
						products, 
						prepareColorAttributes: true, 
						prepareManufacturers: command.ViewMode.IsCaseInsensitiveEqual("list")).ToList();

					model.NoResults = !model.Products.Any();
				}
			}

			model.PagingFilteringContext.LoadPagedList(products);
			return View(model);
		}

		[ChildActionOnly]
		public ActionResult SearchBox()
		{
			var model = new SearchBoxModel
			{
				AutoCompleteEnabled = _catalogSettings.ProductSearchAutoCompleteEnabled,
				ShowProductImagesInSearchAutoComplete = _catalogSettings.ShowProductImagesInSearchAutoComplete,
				SearchTermMinimumLength = _catalogSettings.ProductSearchTermMinimumLength
			};
			return PartialView(model);
		}

		public ActionResult SearchTermAutoComplete(string term)
		{
			if (String.IsNullOrWhiteSpace(term) || term.Length < _catalogSettings.ProductSearchTermMinimumLength)
				return Content("");

			// products
			var pageSize = _catalogSettings.ProductSearchAutoCompleteNumberOfProducts > 0 ? _catalogSettings.ProductSearchAutoCompleteNumberOfProducts : 10;

			var ctx = new ProductSearchContext();
			ctx.LanguageId = _services.WorkContext.WorkingLanguage.Id;
			ctx.Keywords = term;
			ctx.SearchSku = !_catalogSettings.SuppressSkuSearch;
			ctx.OrderBy = ProductSortingEnum.Position;
			ctx.PageSize = pageSize;
			ctx.StoreId = _services.StoreContext.CurrentStoreIdIfMultiStoreMode;
			ctx.VisibleIndividuallyOnly = true;

			var products = _productService.SearchProducts(ctx);

			var models = _helper.PrepareProductOverviewModels(
				products, 
				false, 
				_catalogSettings.ShowProductImagesInSearchAutoComplete, 
				_mediaSettings.ProductThumbPictureSizeOnProductDetailsPage).ToList();
			
			var result = (from p in models
						  select new
						  {
							  label = p.Name,
							  secondary = p.ShortDescription.Truncate(70, "...") ?? "",
							  producturl = Url.RouteUrl("Product", new { SeName = p.SeName }),
							  productpictureurl = p.DefaultPictureModel.ImageUrl
						  })
						  .ToList();
			return Json(result, JsonRequestBehavior.AllowGet);
		}

		#endregion
    }
}
