using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Search;
using SmartStore.Core.Search.Facets;
using SmartStore.Core.Security;
using SmartStore.Services.Catalog;
using SmartStore.Services.Catalog.Extensions;
using SmartStore.Services.Common;
using SmartStore.Services.Localization;
using SmartStore.Services.Search;
using SmartStore.Services.Search.Modelling;
using SmartStore.Services.Search.Rendering;
using SmartStore.Services.Seo;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Seo;
using SmartStore.Web.Models.Catalog;
using SmartStore.Web.Models.Search;

namespace SmartStore.Web.Controllers
{
    public partial class SearchController : PublicControllerBase
    {
        private readonly CatalogSettings _catalogSettings;
        private readonly MediaSettings _mediaSettings;
        private readonly SearchSettings _searchSettings;
        private readonly ICatalogSearchService _catalogSearchService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly CatalogHelper _catalogHelper;
        private readonly ICatalogSearchQueryFactory _queryFactory;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly Lazy<IFacetTemplateProvider> _templateProvider;
        private readonly Lazy<IProductService> _productService;
        private readonly ProductUrlHelper _productUrlHelper;

        public SearchController(
            ICatalogSearchQueryFactory queryFactory,
            ICatalogSearchService catalogSearchService,
            CatalogSettings catalogSettings,
            MediaSettings mediaSettings,
            SearchSettings searchSettings,
            IGenericAttributeService genericAttributeService,
            CatalogHelper catalogHelper,
            ILocalizedEntityService localizedEntityService,
            IUrlRecordService urlRecordService,
            Lazy<IFacetTemplateProvider> templateProvider,
            Lazy<IProductService> productService,
            ProductUrlHelper productUrlHelper)
        {
            _queryFactory = queryFactory;
            _catalogSearchService = catalogSearchService;
            _catalogSettings = catalogSettings;
            _mediaSettings = mediaSettings;
            _searchSettings = searchSettings;
            _genericAttributeService = genericAttributeService;
            _catalogHelper = catalogHelper;
            _localizedEntityService = localizedEntityService;
            _urlRecordService = urlRecordService;
            _templateProvider = templateProvider;
            _productService = productService;
            _productUrlHelper = productUrlHelper;
        }

        [ChildActionOnly]
        public ActionResult SearchBox()
        {
            var model = new SearchBoxModel
            {
                Origin = "Search/Search",
                SearchUrl = Url.RouteUrl("Search"),
                InstantSearchUrl = Url.RouteUrl("InstantSearch"),
                InputPlaceholder = T("Search.SearchBox.Tooltip"),
                InstantSearchEnabled = _searchSettings.InstantSearchEnabled && Services.Permissions.Authorize(Permissions.System.AccessShop),
                ShowThumbsInInstantSearch = _searchSettings.ShowProductImagesInInstantSearch,
                SearchTermMinimumLength = _searchSettings.InstantSearchTermMinLength,
                CurrentQuery = _queryFactory.Current?.Term
            };

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult InstantSearch(CatalogSearchQuery query)
        {
            if (string.IsNullOrWhiteSpace(query.Term) || query.Term.Length < _searchSettings.InstantSearchTermMinLength)
            {
                return Content(string.Empty);
            }

            query
                .BuildFacetMap(false)
                .Slice(0, Math.Min(16, _searchSettings.InstantSearchNumberOfProducts))
                .SortBy(ProductSortingEnum.Relevance);

            var result = _catalogSearchService.Search(query);

            var model = new SearchResultModel(query)
            {
                SearchResult = result,
                Term = query.Term,
                TotalProductsCount = result.TotalHitsCount
            };

            var mappingSettings = _catalogHelper.GetBestFitProductSummaryMappingSettings(ProductSummaryViewMode.Mini, x =>
            {
                x.MapPrices = false;
                x.MapShortDescription = true;
                x.MapPictures = _searchSettings.ShowProductImagesInInstantSearch;
                x.ThumbnailSize = _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage;
                x.PrefetchTranslations = true;
                x.PrefetchUrlSlugs = true;
            });

            using (_urlRecordService.BeginScope(false))
            using (_localizedEntityService.BeginScope(false))
            {
                // InstantSearch should be REALLY very fast! No time for smart caching stuff.
                if (result.Hits.Count > 0)
                {
                    _localizedEntityService.PrefetchLocalizedProperties(
                        nameof(Product),
                        Services.WorkContext.WorkingLanguage.Id,
                        result.Hits.Select(x => x.Id).ToArray());
                }

                // Add product hits.
                model.TopProducts = _catalogHelper.MapProductSummaryModel(result.Hits, mappingSettings);

                // Add spell checker suggestions (if any).
                model.AddSpellCheckerSuggestions(result.SpellCheckerSuggestions, T, x => Url.RouteUrl("Search", new { q = x }));
            }

            return PartialView(model);
        }

        [RewriteUrl(SslRequirement.No)]
        public ActionResult Search(CatalogSearchQuery query)
        {
            var model = new SearchResultModel(query);
            CatalogSearchResult result = null;

            if (query.Term == null || query.Term.Length < _searchSettings.InstantSearchTermMinLength)
            {
                model.SearchResult = new CatalogSearchResult(query);
                model.Error = T("Search.SearchTermMinimumLengthIsNCharacters", _searchSettings.InstantSearchTermMinLength);
                return View(model);
            }

            // 'Continue shopping' URL.
            _genericAttributeService.SaveAttribute(Services.WorkContext.CurrentCustomer,
                SystemCustomerAttributeNames.LastContinueShoppingPage,
                Services.WebHelper.GetThisPageUrl(false),
                Services.StoreContext.CurrentStore.Id);

            try
            {
                if (_searchSettings.SearchProductByIdentificationNumber)
                {
                    var product = _productService.Value.GetProductByIdentificationNumber(query.Term, out var attributeCombination);
                    if (product != null)
                    {
                        if (attributeCombination != null)
                        {
                            return Redirect(_productUrlHelper.GetProductUrl(product.Id, product.GetSeName(), attributeCombination.AttributesXml));
                        }

                        return RedirectToRoute("Product", new { SeName = product.GetSeName() });
                    }
                }

                result = _catalogSearchService.Search(query);
            }
            catch (Exception ex)
            {
                model.Error = ex.ToString();
                result = new CatalogSearchResult(query);
            }

            if (result.TotalHitsCount == 0 && result.SpellCheckerSuggestions.Any())
            {
                // No matches, but spell checker made a suggestion.
                // We implicitly search again with the first suggested term.
                var oldSuggestions = result.SpellCheckerSuggestions;
                var oldTerm = query.Term;
                query.Term = oldSuggestions[0];

                result = _catalogSearchService.Search(query);

                if (result.TotalHitsCount > 0)
                {
                    model.AttemptedTerm = oldTerm;
                    // Restore the original suggestions.
                    result.SpellCheckerSuggestions = oldSuggestions.Where(x => x != query.Term).ToArray();
                }
                else
                {
                    query.Term = oldTerm;
                }
            }

            model.SearchResult = result;
            model.Term = query.Term;
            model.TotalProductsCount = result.TotalHitsCount;

            var mappingSettings = _catalogHelper.GetBestFitProductSummaryMappingSettings(query.GetViewMode());
            var summaryModel = _catalogHelper.MapProductSummaryModel(result.Hits, mappingSettings);

            // Prepare paging/sorting/mode stuff.
            _catalogHelper.MapListActions(summaryModel, null, _catalogSettings.DefaultPageSizeOptions);

            // Add product hits.
            model.TopProducts = summaryModel;

            return View(model);
        }

        [ChildActionOnly]
        public ActionResult Filters(ISearchResultModel model)
        {
            if (model == null)
            {
                return Content("");
            }

            #region Obsolete
            //// TODO: (mc) really necessary?
            //if (excludedFacets != null && excludedFacets.Length > 0)
            //{
            //	foreach (var exclude in excludedFacets.Where(x => x.HasValue()))
            //	{
            //		var facets = searchResultModel.SearchResult.Facets;
            //		if (facets.ContainsKey(exclude))
            //		{
            //			facets.Remove(exclude);
            //		}
            //	} 
            //}
            #endregion

            ViewBag.TemplateProvider = _templateProvider.Value;

            return PartialView(model);
        }

        [ChildActionOnly]
        public ActionResult ActiveFilters(ISearchResultModel model)
        {
            if (model == null || (ControllerContext.ParentActionViewContext != null && ControllerContext.ParentActionViewContext.IsChildAction))
            {
                return Content("");
            }

            return PartialView("Filters.Active", model);
        }

        [ChildActionOnly]
        public ActionResult FacetGroup(FacetGroup facetGroup, string templateName)
        {
            // Just a "proxy" for our "DefaultFacetTemplateSelector"
            return PartialView(templateName, facetGroup);
        }
    }
}