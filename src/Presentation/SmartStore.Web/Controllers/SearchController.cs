using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Search;
using SmartStore.Services.Catalog;
using SmartStore.Services.Common;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;
using SmartStore.Services.Search;
using SmartStore.Services.Search.Modelling;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.UI;
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
		private readonly ICurrencyService _currencyService;
		private readonly IManufacturerService _manufacturerService;
		private readonly IGenericAttributeService _genericAttributeService;
		private readonly CatalogHelper _catalogHelper;
		private readonly ICatalogSearchQueryFactory _queryFactory;

		public SearchController(
			ICatalogSearchQueryFactory queryFactory,
			ICatalogSearchService catalogSearchService,
			CatalogSettings catalogSettings,
			MediaSettings mediaSettings,
			SearchSettings searchSettings,
			ICurrencyService currencyService,
			IManufacturerService manufacturerService,
			IGenericAttributeService genericAttributeService,
			CatalogHelper catalogHelper)
		{
			_queryFactory = queryFactory;
			_catalogSearchService = catalogSearchService;
			_catalogSettings = catalogSettings;
			_mediaSettings = mediaSettings;
			_searchSettings = searchSettings;
			_currencyService = currencyService;
			_manufacturerService = manufacturerService;
			_genericAttributeService = genericAttributeService;
			_catalogHelper = catalogHelper;

			QuerySettings = DbQuerySettings.Default;
		}

		public DbQuerySettings QuerySettings { get; set; }

		[ChildActionOnly]
		public ActionResult SearchBox()
		{
			var currentTerm = _queryFactory.Current?.Term;

			var model = new SearchBoxModel
			{
				InstantSearchEnabled = _searchSettings.InstantSearchEnabled,
				ShowProductImagesInInstantSearch = _searchSettings.ShowProductImagesInInstantSearch,
				SearchTermMinimumLength = _searchSettings.InstantSearchTermMinLength,
				CurrentQuery = currentTerm
			};

			return PartialView(model);
		}

		[HttpPost]
		public ActionResult InstantSearch(CatalogSearchQuery query)
		{
			if (string.IsNullOrWhiteSpace(query.Term) || query.Term.Length < _searchSettings.InstantSearchTermMinLength)
				return Content(string.Empty);

			// Overwrite search fields
			var searchFields = new List<string> { "name" };
			searchFields.AddRange(_searchSettings.SearchFields);

			query.Fields = searchFields.ToArray();

			query
				.Slice(0, Math.Min(16, _searchSettings.InstantSearchNumberOfProducts))
				.SortBy(ProductSortingEnum.Relevance);

			var result = _catalogSearchService.Search(query);

			var overviewModels = _catalogHelper.PrepareProductOverviewModels(
				result.Hits, 
				false, 
				_searchSettings.ShowProductImagesInInstantSearch,
				_mediaSettings.ProductThumbPictureSizeOnProductDetailsPage);

			var model = new SearchResultModel(query)
			{
				SearchResult = result,
				Term = query.Term,
				TotalProductsCount = result.Hits.TotalCount
			};

			// Add product hits
			model.TopProducts.AddRange(overviewModels);

			// Add spell checker suggestions (if any)
			AddSpellCheckerSuggestionsToModel(result.SpellCheckerSuggestions, model);

			return PartialView(model);
		}

		[RequireHttpsByConfigAttribute(SslRequirement.No)]
		[ValidateInput(false)]
		public ActionResult Search(CatalogSearchQuery query)
		{
			var model = new SearchResultModel(query);

			if (query.Term == null || query.Term.Length < _searchSettings.InstantSearchTermMinLength)
			{
				model.Error = T("Search.SearchTermMinimumLengthIsNCharacters", _searchSettings.InstantSearchTermMinLength);
				return View(model);
			}
			
			// 'Continue shopping' URL
			_genericAttributeService.SaveAttribute(Services.WorkContext.CurrentCustomer,
				SystemCustomerAttributeNames.LastContinueShoppingPage,
				Services.WebHelper.GetThisPageUrl(false),
				Services.StoreContext.CurrentStore.Id);
			
			var result = _catalogSearchService.Search(query);

			var overviewModels = _catalogHelper.PrepareProductOverviewModels(
				result.Hits,
				prepareColorAttributes: true,
				prepareManufacturers: false /* TODO: (mc) ViewModes */).ToList();

			model.SearchResult = result;
			model.Term = query.Term;
			model.TotalProductsCount = result.Hits.TotalCount;

			// Add product hits
			model.TopProducts.AddRange(overviewModels);

			// Add spell checker suggestions (if any)
			AddSpellCheckerSuggestionsToModel(result.SpellCheckerSuggestions, model);

			return View(model);
		}

		[RequireHttpsByConfigAttribute(SslRequirement.No)]
		[ValidateInput(false)]
		public ActionResult Search2(SearchModel model, SearchPagingFilteringModel command)
		{
			if (model == null)
				model = new SearchModel();

			var resultModel = new SearchResultModel(new CatalogSearchQuery());

			// 'Continue shopping' URL
			_genericAttributeService.SaveAttribute(Services.WorkContext.CurrentCustomer,
				SystemCustomerAttributeNames.LastContinueShoppingPage,
				Services.WebHelper.GetThisPageUrl(false),
				Services.StoreContext.CurrentStore.Id);

			if (command.PageSize <= 0)
				command.PageSize = _catalogSettings.DefaultProductListPageSize;
			if (command.PageNumber <= 0)
				command.PageNumber = 1;

			if (command.OrderBy == (int)ProductSortingEnum.Initial)
				command.OrderBy = (int)_catalogSettings.DefaultSortOrder;

			_catalogHelper.PreparePagingFilteringModel(model.PagingFilteringContext, command, new PageSizeContext
			{
				AllowCustomersToSelectPageSize = _catalogSettings.ProductsByTagAllowCustomersToSelectPageSize,
				PageSize = _catalogSettings.DefaultProductListPageSize,
				PageSizeOptions = _catalogSettings.DefaultPageSizeOptions
			});

			model.Q = model.Q.EmptyNull().Trim();
			resultModel.Term = model.Q;

			// Build AvailableCategories
			model.AvailableCategories.Add(new SelectListItem { Value = "0", Text = T("Common.All") });

			var navModel = _catalogHelper.PrepareCategoryNavigationModel(0, 0);

			navModel.Root.Traverse((node) =>
			{
				if (node.IsRoot)
					return;

				var id = node.Value.EntityId;
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
				model.AvailableManufacturers.Add(new SelectListItem { Value = "0", Text = T("Common.All") });

				foreach (var m in manufacturers)
				{
					model.AvailableManufacturers.Add(new SelectListItem
					{
						Value = m.Id.ToString(),
						Text = m.GetLocalized(x => x.Name),
						Selected = model.Mid == m.Id
					});
				}
			}

			IPagedList<Product> products = new PagedList<Product>(new List<Product>(), 0, 1);

			// only search if query string search keyword is set (used to avoid searching or displaying search term min length error message on /search page load)
			if (Request.Params["Q"] != null)
			{
				if (model.Q.Length < _searchSettings.InstantSearchTermMinLength)
				{
					model.Warning = string.Format(T("Search.SearchTermMinimumLengthIsNCharacters"), _searchSettings.InstantSearchTermMinLength);
				}
				else
				{
					var fields = new List<string> { "name" };
					fields.AddRange(_searchSettings.SearchFields);

					var searchQuery = new CatalogSearchQuery(fields.ToArray(), model.Q)
						.OriginatesFrom("Search")
						.Slice((command.PageNumber - 1) * command.PageSize, command.PageSize)
						.WithLanguage(Services.WorkContext.WorkingLanguage)
						.VisibleIndividuallyOnly(true)
						.SortBy((ProductSortingEnum)command.OrderBy);

					// Visibility
					searchQuery.VisibleOnly(!QuerySettings.IgnoreAcl ? Services.WorkContext.CurrentCustomer : null);

					// Store
					if (!QuerySettings.IgnoreMultiStore)
					{
						searchQuery.HasStoreId(Services.StoreContext.CurrentStore.Id);
					}

					if (model.As)
					{
						if (model.Cid > 0)
						{
							var categoryIds = new List<int> { model.Cid };
							if (model.Isc)
							{
								categoryIds.AddRange(_catalogHelper.GetChildCategoryIds(model.Cid));
							}

							searchQuery = searchQuery.WithCategoryIds(null, categoryIds.ToArray());
						}

						if (model.Mid > 0)
						{
							searchQuery = searchQuery.WithManufacturerIds(null, model.Mid);
						}

						decimal? fromPrice = null;
						decimal? toPrice = null;
						var currency = Services.WorkContext.WorkingCurrency;

						if (model.Pf.HasValue())
						{
							var minPrice = decimal.Zero;
							if (decimal.TryParse(model.Pf, out minPrice))
								fromPrice = _currencyService.ConvertToPrimaryStoreCurrency(minPrice, currency);
						}
						if (model.Pt.HasValue())
						{
							var maxPrice = decimal.Zero;
							if (decimal.TryParse(model.Pt, out maxPrice))
								toPrice = _currencyService.ConvertToPrimaryStoreCurrency(maxPrice, currency);
						}

						if (fromPrice.HasValue || toPrice.HasValue)
						{
							searchQuery = searchQuery.PriceBetween(fromPrice, toPrice, currency);
						}
					}

					var searchResult = _catalogSearchService.Search(searchQuery);
					products = searchResult.Hits;

					model.Products = _catalogHelper.PrepareProductOverviewModels(
						products,
						prepareColorAttributes: true,
						prepareManufacturers: command.ViewMode.IsCaseInsensitiveEqual("list")).ToList();

					model.NoResults = !model.Products.Any();

					resultModel.TotalProductsCount = searchResult.Hits.TotalCount;
					resultModel.TopProducts.AddRange(model.Products);
					resultModel.SearchResult = searchResult;

					// Add spell checker suggestions (if any)
					AddSpellCheckerSuggestionsToModel(searchResult.SpellCheckerSuggestions, resultModel);
				}
			}
			else
			{
				model.Warning = string.Format(T("Search.SearchTermMinimumLengthIsNCharacters"), _searchSettings.InstantSearchTermMinLength);
				model.Sid = _searchSettings.SearchFields.Contains("fulldescription");
			}

			// TODO: (mc) Temp only
			ViewBag.ResultModel = resultModel;

			model.PagingFilteringContext.LoadPagedList(products);
			return View(model);
		}

		private void AddSpellCheckerSuggestionsToModel(string[] suggestions, SearchResultModel model)
		{
			if (suggestions.Length == 0)
				return;

			var hitGroup = new SearchResultModel.HitGroup(model)
			{
				Name = "SpellChecker",
				DisplayName = T("Search.DidYouMean"),
				Ordinal = -100
			};

			hitGroup.Hits.AddRange(suggestions.Select(x => new SearchResultModel.HitItem
			{
				Label = x,
				Url = Url.RouteUrl("Search", new { q = x })
			}));

			model.HitGroups.Add(hitGroup);
		}
	}
}