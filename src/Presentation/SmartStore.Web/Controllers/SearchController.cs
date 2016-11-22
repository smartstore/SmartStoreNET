using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Services.Catalog;
using SmartStore.Services.Common;
using SmartStore.Services.Directory;
using SmartStore.Services.Search;
using SmartStore.Services.Localization;
using SmartStore.Services.Seo;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Models.Catalog;
using SmartStore.Web.Framework.UI;
using SmartStore.Web.Models.Search;
using SmartStore.Core.Domain.Media;

namespace SmartStore.Web.Controllers
{
	public partial class SearchController : PublicControllerBase
	{
		private readonly CatalogSettings _catalogSettings;
		private readonly MediaSettings _mediaSettings;
		private readonly ICatalogSearchService _catalogSearchService;
		private readonly ICurrencyService _currencyService;
		private readonly IManufacturerService _manufacturerService;
		private readonly IGenericAttributeService _genericAttributeService;
		private readonly CatalogHelper _catalogHelper;

		public SearchController(
			CatalogSettings catalogSettings,
			MediaSettings mediaSettings,
			ICatalogSearchService catalogSearchService,
			ICurrencyService currencyService,
			IManufacturerService manufacturerService,
			IGenericAttributeService genericAttributeService,
			CatalogHelper catalogHelper)
		{
			_catalogSettings = catalogSettings;
			_mediaSettings = mediaSettings;
			_catalogSearchService = catalogSearchService;
			_currencyService = currencyService;
			_manufacturerService = manufacturerService;
			_genericAttributeService = genericAttributeService;
			_catalogHelper = catalogHelper;
		}

		[ChildActionOnly]
		public ActionResult SearchBox()
		{
			var model = new SearchBoxModel
			{
				InstantSearchEnabled = _catalogSettings.ProductSearchAutoCompleteEnabled,
				ShowProductImagesInInstantSearch = _catalogSettings.ShowProductImagesInSearchAutoComplete,
				SearchTermMinimumLength = _catalogSettings.ProductSearchTermMinimumLength
			};
			return PartialView(model);
		}

		//[HttpPost]
		//public ActionResult InstantSearch(string term)
		//{
		//	if (string.IsNullOrWhiteSpace(term) || term.Length < _catalogSettings.ProductSearchTermMinimumLength)
		//		return Content("");

		//	var numberOfSuggestions = (_catalogSettings.ProductSearchAutoCompleteNumberOfProducts > 0 ? _catalogSettings.ProductSearchAutoCompleteNumberOfProducts : 10);
		//	var searchQuery = new CatalogSearchQuery("name", term)
		//		.WithSuggestions(numberOfSuggestions)
		//		.Slice(0, 0)
		//		.HasStoreId(Services.StoreContext.CurrentStore.Id)
		//		.WithLanguage(Services.WorkContext.WorkingLanguage);

		//	var result = _catalogSearchService.Search(searchQuery);

		//	ViewBag.Term = term;

		//	return PartialView(result.Suggestions);
		//}

		[HttpPost]
		public ActionResult InstantSearch(string term)
		{
			if (string.IsNullOrWhiteSpace(term) || term.Length < _catalogSettings.ProductSearchTermMinimumLength)
				return Content("");

			var maxItems = Math.Min(16, _catalogSettings.ProductSearchAutoCompleteNumberOfProducts);
			var searchQuery = new CatalogSearchQuery("name", term, isExactMatch: false)
				.Slice(0, maxItems)
				.SortBy(ProductSortingEnum.Position)
				.HasStoreId(Services.StoreContext.CurrentStore.Id)
				.WithLanguage(Services.WorkContext.WorkingLanguage);

			var result = _catalogSearchService.Search(searchQuery);

			var overviewModels = _catalogHelper.PrepareProductOverviewModels(
				result.Hits, 
				false, 
				_catalogSettings.ShowProductImagesInSearchAutoComplete,
				_mediaSettings.ProductThumbPictureSizeOnProductDetailsPage);

			var model = new InstantSearchResultModel
			{
				SearchResult = result,
				Term = term,
				TotalProductsCount = result.Hits.TotalCount
			};
			model.TopProducts.AddRange(overviewModels);

			return PartialView(model);
		}

		[RequireHttpsByConfigAttribute(SslRequirement.No)]
		[ValidateInput(false)]
		public ActionResult Search(SearchModel model, SearchPagingFilteringModel command)
		{
			// TODO: // TODO: find this in views

			if (model == null)
				model = new SearchModel();

			// 'Continue shopping' URL
			_genericAttributeService.SaveAttribute(Services.WorkContext.CurrentCustomer,
				SystemCustomerAttributeNames.LastContinueShoppingPage,
				Services.WebHelper.GetThisPageUrl(false),
				Services.StoreContext.CurrentStore.Id);

			if (command.PageSize <= 0)
				command.PageSize = _catalogSettings.SearchPageProductsPerPage;
			if (command.PageNumber <= 0)
				command.PageNumber = 1;

			if (command.OrderBy == (int)ProductSortingEnum.Initial)
				command.OrderBy = (int)_catalogSettings.DefaultSortOrder;

			_catalogHelper.PreparePagingFilteringModel(model.PagingFilteringContext, command, new PageSizeContext
			{
				AllowCustomersToSelectPageSize = _catalogSettings.ProductSearchAllowCustomersToSelectPageSize,
				PageSize = _catalogSettings.SearchPageProductsPerPage,
				PageSizeOptions = _catalogSettings.ProductSearchPageSizeOptions
			});

			model.Q = model.Q.EmptyNull().Trim();

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
				if (model.Q.Length < _catalogSettings.ProductSearchTermMinimumLength)
				{
					model.Warning = string.Format(T("Search.SearchTermMinimumLengthIsNCharacters"), _catalogSettings.ProductSearchTermMinimumLength);
				}
				else
				{
					var fields = new List<string> { "name", "shortdescription" };
					if (!_catalogSettings.SuppressSkuSearch)
					{
						fields.Add("sku");
					}
					if (model.Sid)
					{
						fields.Add("tagname");
						fields.Add("fulldescription");
					}

					var searchQuery = new CatalogSearchQuery(fields.ToArray(), model.Q, true)
						.Slice((command.PageNumber - 1) * command.PageSize, command.PageSize)
						.HasStoreId(Services.StoreContext.CurrentStore.Id)
						.WithLanguage(Services.WorkContext.WorkingLanguage)
						.VisibleIndividuallyOnly(true)
						.SortBy((ProductSortingEnum)command.OrderBy);

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
							searchQuery = searchQuery.WithPrice(currency, fromPrice, toPrice);
						}
					}

					var hits = _catalogSearchService.Search(searchQuery);
					products = hits.Hits;

					model.Products = _catalogHelper.PrepareProductOverviewModels(
						products,
						prepareColorAttributes: true,
						prepareManufacturers: command.ViewMode.IsCaseInsensitiveEqual("list")).ToList();

					model.NoResults = !model.Products.Any();
				}
			}
			else
			{
				model.Sid = _catalogSettings.SearchDescriptions;
			}

			model.PagingFilteringContext.LoadPagedList(products);
			return View(model);
		}
	}
}