using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Search;
using SmartStore.Services.Catalog;
using SmartStore.Services.Directory;

namespace SmartStore.Services.Search.Modelling
{
	/*
		TOKENS:
		===============================
		q	-	Search term
		i	-	Page index
		s	-	Page size
		o	-	Order by
		pf	-	Price from
		pt	-	Price to
		c	-	Categories
		m	-	Manufacturers
		r	-	Min Rating
		a	-	Stock
		d	-	Delivery Time

		v	-	View Mode
		
		*	-	Specification attributes & variants 
	*/

	public class CatalogSearchQueryFactory : ICatalogSearchQueryFactory
	{
		private readonly HttpContextBase _httpContext;
		private readonly CatalogSettings _catalogSettings;
		private readonly SearchSettings _searchSettings;
		private readonly ICurrencyService _currencyService;
		private readonly ICommonServices _services;

		public CatalogSearchQueryFactory(
			HttpContextBase httpContext,
			CatalogSettings catalogSettings,
			SearchSettings searchSettings,
			ICurrencyService currencyService,
			ICommonServices services)
		{
			_httpContext = httpContext;
			_catalogSettings = catalogSettings;
			_searchSettings = searchSettings;
			_currencyService = currencyService;
			_services = services;

			QuerySettings = DbQuerySettings.Default;
		}

		public DbQuerySettings QuerySettings { get; set; }

		public CatalogSearchQuery Current { get; private set; }

		public CatalogSearchQuery CreateFromQuery()
		{
			var ctx = _httpContext;

			if (ctx.Request == null)
				return null;

			var routeData = ctx.Request.RequestContext.RouteData;
			var area = routeData.GetAreaName();
			var controller = routeData.GetRequiredString("controller");
			var action = routeData.GetRequiredString("action");
			string origin = "{0}{1}/{2}".FormatInvariant(area == null ? "" : area + "/", controller, action);

			var term = GetValueFor<string>("q");

			var fields = new List<string> { "name" };
			fields.AddRange(_searchSettings.SearchFields);

			var query = new CatalogSearchQuery(fields.ToArray(), term, _searchSettings.SearchMode)
				.OriginatesFrom(origin)
				.WithLanguage(_services.WorkContext.WorkingLanguage)
				.VisibleIndividuallyOnly(true);

			// Visibility
			query.VisibleOnly(!QuerySettings.IgnoreAcl ? _services.WorkContext.CurrentCustomer : null);

			// Store
			if (!QuerySettings.IgnoreMultiStore)
			{
				query.HasStoreId(_services.StoreContext.CurrentStore.Id);
			}

			ConvertPagingSorting(query, routeData, origin);
			ConvertPrice(query, routeData, origin);
			ConvertCategory(query, routeData, origin);
			ConvertManufacturer(query, routeData, origin);
			ConvertRating(query, routeData, origin);

			OnConverted(query, routeData, origin);

			this.Current = query;

			return query;
		}

		protected virtual void ConvertPagingSorting(CatalogSearchQuery query, RouteData routeData, string origin)
		{
			var index = Math.Max(1, GetValueFor<int?>("i") ?? 1);
			var size = GetPageSize(query, routeData, origin);

			query.Slice((index - 1) * size, size);

			var orderBy = GetValueFor<ProductSortingEnum?>("o");
			if (orderBy == null || orderBy == ProductSortingEnum.Initial)
			{
				orderBy = _catalogSettings.DefaultSortOrder;
			}

			query.CustomData["CurrentSortOrder"] = orderBy.Value;

			query.SortBy(orderBy.Value);
		}

		private int GetPageSize(CatalogSearchQuery query, RouteData routeData, string origin)
		{
			string entityViewMode = null;

			// Determine entity id if possible
			IPagingOptions entity = null;
			int? entityId;
			if (origin.IsCaseInsensitiveEqual("Catalog/Category"))
			{
				entityId = (int?)routeData.Values["categoryId"];
				if (entityId.HasValue)
				{
					entity = _services.Resolve<ICategoryService>().GetCategoryById(entityId.Value) as IPagingOptions;
					entityViewMode = ((Category)entity)?.DefaultViewMode;
				}
			}
			else if (origin.IsCaseInsensitiveEqual("Catalog/Manufacturer"))
			{
				entityId = (int?)routeData.Values["manufacturerId"];
				if (entityId.HasValue)
				{
					entity = _services.Resolve<IManufacturerService>().GetManufacturerById(entityId.Value) as IPagingOptions;
				}
			}
			
			var entitySize = entity?.PageSize;

			var sessionKey = origin;
			if (entitySize.HasValue)
			{
				sessionKey += "/" + entitySize.Value;
			}

			DetectViewMode(query, sessionKey, entityViewMode);

			var allowChange = entity?.AllowCustomersToSelectPageSize ?? _catalogSettings.AllowCustomersToSelectPageSize;
			if (!allowChange)
			{
				return entitySize ?? _catalogSettings.DefaultProductListPageSize;
			}

			sessionKey = "PageSize:" + sessionKey;

			// Get from form or query
			var selectedSize = GetValueFor<int?>("s");

			if (selectedSize.HasValue)
			{
				// Save the selection in session. We'll fetch this session value
				// on subsequent requests for this route.
				_httpContext.Session[sessionKey] = selectedSize.Value;
				return selectedSize.Value;
			}

			// Return user size from session
			var sessionSize = _httpContext.Session[sessionKey].Convert<int?>();
			if (sessionSize.HasValue)
			{
				return sessionSize.Value;
			}

			// Return default size for entity (IPagingOptions)
			if (entitySize.HasValue)
			{
				return entitySize.Value;
			}

			// Return default page size
			return _catalogSettings.DefaultProductListPageSize;
		}

		private void DetectViewMode(CatalogSearchQuery query, string sessionKey, string entityViewMode = null)
		{
			if (!_catalogSettings.AllowProductViewModeChanging)
			{
				query.CustomData["ViewMode"] = entityViewMode.NullEmpty() ?? _catalogSettings.DefaultViewMode;
				return;
			}

			var selectedViewMode = GetValueFor<string>("v");

			sessionKey = "ViewMode:" + sessionKey;

			if (selectedViewMode != null)
			{
				// Save the view mode selection in session. We'll fetch this session value
				// on subsequent requests for this route.
				_httpContext.Session[sessionKey] = selectedViewMode;
				query.CustomData["ViewMode"] = selectedViewMode;
				return;
			}

			// Set view mode from session
			var sessionViewMode = _httpContext.Session[sessionKey].Convert<string>();
			if (sessionViewMode != null)
			{
				query.CustomData["ViewMode"] = sessionViewMode;
				return;
			}

			// Set default view mode for entity
			if (entityViewMode != null)
			{
				query.CustomData["ViewMode"] = entityViewMode;
				return;
			}

			// Set default view mode
			query.CustomData["ViewMode"] = _catalogSettings.DefaultViewMode;
		}

		protected virtual void ConvertCategory(CatalogSearchQuery query, RouteData routeData, string origin)
		{
			var ids = GetValueFor<List<int>>("c");
			if (ids != null && ids.Any())
			{
				// TODO; (mc) Get deep ids (???) Make a low-level version of CatalogHelper.GetChildCategoryIds()
				query.WithCategoryIds(_catalogSettings.IncludeFeaturedProductsInNormalLists ? null : (bool?)false, ids.ToArray());
			}
		}

		protected virtual void ConvertManufacturer(CatalogSearchQuery query, RouteData routeData, string origin)
		{
			var ids = GetValueFor<List<int>>("m");
			if (ids != null && ids.Any())
			{
				query.WithManufacturerIds(null, ids.ToArray());
			}
		}

		protected virtual void ConvertPrice(CatalogSearchQuery query, RouteData routeData, string origin)
		{
			var currency = _services.WorkContext.WorkingCurrency;
			var minPrice = GetValueFor<decimal?>("pf");
			var maxPrice = GetValueFor<decimal?>("pt");

			if (minPrice.HasValue)
			{
				minPrice = _currencyService.ConvertToPrimaryStoreCurrency(minPrice.Value, currency);
			}

			if (maxPrice.HasValue)
			{
				maxPrice = _currencyService.ConvertToPrimaryStoreCurrency(maxPrice.Value, currency);
			}

			if (minPrice.HasValue || maxPrice.HasValue)
			{
				query.PriceBetween(minPrice, maxPrice, currency);
			}
		}

		protected virtual void ConvertRating(CatalogSearchQuery query, RouteData routeData, string origin)
		{
		}

		protected virtual void ConvertStock(CatalogSearchQuery query, RouteData routeData, string origin)
		{
		}

		protected virtual void ConvertDeliveryTime(CatalogSearchQuery query, RouteData routeData, string origin)
		{
		}

		protected virtual void OnConverted(CatalogSearchQuery query, RouteData routeData, string origin)
		{
		}

		public string ToQueryString(CatalogSearchQuery query)
		{
			return query.ToString();
		}

		protected T GetValueFor<T>(string key)
		{
			var value = _httpContext.Request?.Form?[key] ?? _httpContext.Request?.QueryString?[key];

			if (value != null)
			{
				return value.Convert<T>();
			}

			return default(T);
		}
	}
}
