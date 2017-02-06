using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Routing;
using Newtonsoft.Json;
using SmartStore.Collections;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Search;
using SmartStore.Core.Search.Facets;
using SmartStore.Core.Search.Filter;
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
		sq	-	Stock Quantity
		d	-	Delivery Time
		v	-	View Mode
		
		*	-	Variants 
	*/

	public class CatalogSearchQueryFactory : ICatalogSearchQueryFactory
	{
		protected static readonly string[] _tokens = new string[] { "q", "i", "s", "o", "pf", "pt", "c", "m", "r", "sq", "d", "v" };
		protected readonly HttpContextBase _httpContext;
		protected readonly CatalogSettings _catalogSettings;
		protected readonly SearchSettings _searchSettings;
		protected readonly ICurrencyService _currencyService;
		protected readonly ICommonServices _services;
		// field name to display order
		protected Dictionary<string, int> _globalFilters;
		private Multimap<string, string> _aliases;

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

			_globalFilters = new Dictionary<string, int>();

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
			var origin = "{0}{1}/{2}".FormatInvariant(area == null ? "" : area + "/", controller, action);

			if (!origin.IsCaseInsensitiveEqual("Search/InstantSearch"))
			{
				_globalFilters.Add(_catalogSettings.IncludeFeaturedProductsInNormalLists ? "categoryid" : "notfeaturedcategoryid", 0);

				if (_searchSettings.GlobalFilters.HasValue())
				{
					var globalFilters = JsonConvert.DeserializeObject<List<GlobalSearchFilterDescriptor>>(_searchSettings.GlobalFilters);

					globalFilters.Where(x => !x.Disabled).Each(x =>	_globalFilters.Add(x.FieldName, x.DisplayOrder));
				}
				else
				{				
					_globalFilters.Add("manufacturerid", 1);
					_globalFilters.Add("price", 2);
					_globalFilters.Add("rate", 3);
					_globalFilters.Add("deliveryid", 4);
				}
			}

			var term = GetValueFor<string>("q");

			var fields = new List<string> { "name" };
			fields.AddRange(_searchSettings.SearchFields);

			var query = new CatalogSearchQuery(fields.ToArray(), term, _searchSettings.SearchMode)
				.OriginatesFrom(origin)
				.WithLanguage(_services.WorkContext.WorkingLanguage)
				.WithCurrency(_services.WorkContext.WorkingCurrency)
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
			ConvertStock(query, routeData, origin);
			ConvertDeliveryTime(query, routeData, origin);

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
				if (_httpContext.Session != null)
				{
					_httpContext.Session[sessionKey] = selectedSize.Value;
				}
				return selectedSize.Value;
			}

			// Return user size from session
			if (_httpContext.Session != null)
			{
				var sessionSize = _httpContext.Session[sessionKey].Convert<int?>();
				if (sessionSize.HasValue)
				{
					return sessionSize.Value;
				}
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
				if (_httpContext.Session != null)
				{
					_httpContext.Session[sessionKey] = selectedViewMode;
				}
				query.CustomData["ViewMode"] = selectedViewMode;
				return;
			}

			// Set view mode from session
			if (_httpContext.Session != null)
			{
				var sessionViewMode = _httpContext.Session[sessionKey].Convert<string>();
				if (sessionViewMode != null)
				{
					query.CustomData["ViewMode"] = sessionViewMode;
					return;
				}
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

		private void AddFacet(
			CatalogSearchQuery query,
			string fieldName,
			bool isMultiSelect,
			FacetSorting sorting,
			Action<FacetDescriptor> addValues)
		{
			if (!_globalFilters.ContainsKey(fieldName))
				return;

			var facet = new FacetDescriptor(fieldName);
			facet.Label = _services.Localization.GetResource(FacetDescriptor.GetLabelResourceKey(fieldName) ?? fieldName);
			facet.IsMultiSelect = isMultiSelect;
			facet.DisplayOrder = _globalFilters[fieldName];
			facet.OrderBy = sorting;

			if (fieldName != "rate")
			{
				facet.MinHitCount = _searchSettings.FilterMinHitCount;
				facet.MaxChoicesCount = _searchSettings.FilterMaxChoicesCount;
			}

			addValues(facet);
			query.WithFacet(facet);
		}

		protected virtual void ConvertCategory(CatalogSearchQuery query, RouteData routeData, string origin)
		{
			var ids = GetValueFor<List<int>>("c");
			if (ids != null && ids.Any())
			{
				// TODO; (mc) Get deep ids (???) Make a low-level version of CatalogHelper.GetChildCategoryIds()
				query.WithCategoryIds(_catalogSettings.IncludeFeaturedProductsInNormalLists ? null : (bool?)false, ids.ToArray());
			}

			var fieldName = (_catalogSettings.IncludeFeaturedProductsInNormalLists ? "categoryid" : "notfeaturedcategoryid");

			AddFacet(query, fieldName, true, FacetSorting.HitsDesc, descriptor =>
			{
				if (ids != null && ids.Any())
				{
					ids.Select(x => new FacetValue(x) { IsSelected = true })
						.Each(x => descriptor.AddValue(x));
				}
			});
		}

		protected virtual void ConvertManufacturer(CatalogSearchQuery query, RouteData routeData, string origin)
		{
			var ids = GetValueFor<List<int>>("m");
			if (ids != null && ids.Any())
			{
				query.WithManufacturerIds(null, ids.ToArray());
			}

			AddFacet(query, "manufacturerid", true, FacetSorting.ValueAsc, descriptor =>
			{
				if (ids != null && ids.Any())
				{
					ids.Select(x => new FacetValue(x) { IsSelected = true })
						.Each(x => descriptor.AddValue(x));
				}
			});
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

			if (minPrice.HasValue && maxPrice.HasValue && minPrice > maxPrice)
			{
				var tmp = minPrice;
				minPrice = maxPrice;
				maxPrice = tmp;
			}

			if (minPrice.HasValue || maxPrice.HasValue)
			{
				query.PriceBetween(minPrice, maxPrice);
			}

			AddFacet(query, "price", false, FacetSorting.DisplayOrder, descriptor =>
			{
				if (minPrice.HasValue || maxPrice.HasValue)
				{
					descriptor.AddValue(new FacetValue(
						minPrice.HasValue ? decimal.ToDouble(minPrice.Value) : (double?)null,
						maxPrice.HasValue ? decimal.ToDouble(maxPrice.Value) : (double?)null,
						IndexTypeCode.Double,
						minPrice.HasValue,
						maxPrice.HasValue)
					{
						IsSelected = true
					});
				}
			});
		}

		protected virtual void ConvertRating(CatalogSearchQuery query, RouteData routeData, string origin)
		{
			var fromRate = GetValueFor<double?>("r");

			if (fromRate.HasValue)
			{
				query.WithRating(fromRate, null);
			}

			AddFacet(query, "rate", false, FacetSorting.DisplayOrder, descriptor =>
			{
				if (fromRate.HasValue)
				{
					descriptor.AddValue(new FacetValue(fromRate.Value) { IsSelected = true });
				}
			});
		}

		protected virtual void ConvertStock(CatalogSearchQuery query, RouteData routeData, string origin)
		{
			var fromQuantity = GetValueFor<int?>("sq");

			if (fromQuantity.HasValue)
			{
				query.WithStockQuantity(fromQuantity, null);
			}
		}

		protected virtual void ConvertDeliveryTime(CatalogSearchQuery query, RouteData routeData, string origin)
		{
			var ids = GetValueFor<List<int>>("d");
			if (ids != null && ids.Any())
			{
				query.WithDeliveryTimeIds(ids.ToArray());
			}

			AddFacet(query, "deliveryid", true, FacetSorting.DisplayOrder, descriptor =>
			{
				if (ids != null && ids.Any())
				{
					ids.Select(x => new FacetValue(x) { IsSelected = true })
						.Each(x => descriptor.AddValue(x));
				}
			});
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

		protected Multimap<string, string> Aliases
		{
			get
			{
				if (_aliases == null)
				{
					_aliases = new Multimap<string, string>();

					if (_httpContext.Request != null)
					{
						var form = _httpContext.Request.Form;
						var query = _httpContext.Request.QueryString;

						if (form != null)
						{
							form.AllKeys.Where(x => !_tokens.Contains(x)).Each(key => _aliases.AddRange(key, form[key].SplitSafe(",")));
						}

						if (query != null)
						{
							query.AllKeys.Where(x => !_tokens.Contains(x)).Each(key => _aliases.AddRange(key, query[key].SplitSafe(",")));
						}
					}
				}

				return _aliases;
			}
		}
	}
}
