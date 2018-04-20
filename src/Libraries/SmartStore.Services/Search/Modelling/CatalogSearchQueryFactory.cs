using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using SmartStore.Collections;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Search;
using SmartStore.Core.Search.Facets;
using SmartStore.Services.Catalog;
using SmartStore.Utilities;
using SmartStore.Services.Security;

namespace SmartStore.Services.Search.Modelling
{
	/*
		TOKENS:
		===============================
		q	-	Search term
		i	-	Page index
		s	-	Page size
		o	-	Order by
		p	-   Price range (from~to || from(~) || ~to)
		c	-	Categories
		m	-	Manufacturers
		r	-	Min Rating
		a	-	Availability
		n	-	New Arrivals
		d	-	Delivery Time
		v	-	View Mode
		
		*	-	Variants & attributes
	*/

	public class CatalogSearchQueryFactory : ICatalogSearchQueryFactory
	{
		protected static readonly string[] _tokens = new string[] { "q", "i", "s", "o", "p", "c", "m", "r", "a", "n", "d", "v" };
        protected static readonly string[] _instantSearchFields = new string[] { "manufacturer", "sku", "gtin", "mpn", "attrname", "variantname" };

        protected readonly HttpContextBase _httpContext;
		protected readonly CatalogSettings _catalogSettings;
		protected readonly SearchSettings _searchSettings;
		protected readonly ICommonServices _services;
		protected readonly ICatalogSearchQueryAliasMapper _catalogSearchQueryAliasMapper;
		private Multimap<string, string> _aliases;

		public CatalogSearchQueryFactory(
			HttpContextBase httpContext,
			CatalogSettings catalogSettings,
			SearchSettings searchSettings,
			ICommonServices services,
			ICatalogSearchQueryAliasMapper catalogSearchQueryAliasMapper)
		{
			_httpContext = httpContext;
			_catalogSettings = catalogSettings;
			_searchSettings = searchSettings;
			_services = services;
			_catalogSearchQueryAliasMapper = catalogSearchQueryAliasMapper;

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
			var fields = new List<string> { "name" };
			var term = GetValueFor<string>("q");
			var isInstantSearch = origin.IsCaseInsensitiveEqual("Search/InstantSearch");

			if (isInstantSearch)
			{
				fields.Add("shortdescription");
				fields.Add("tagname");

                foreach (var fieldName in _instantSearchFields)
                {
                    if (_searchSettings.SearchFields.Contains(fieldName))
                        fields.Add(fieldName);
                }
            }
			else
			{
				fields.AddRange(_searchSettings.SearchFields);
			}

			var query = new CatalogSearchQuery(fields.ToArray(), term, _searchSettings.SearchMode)
				.OriginatesFrom(origin)
				.WithLanguage(_services.WorkContext.WorkingLanguage)
				.WithCurrency(_services.WorkContext.WorkingCurrency)
				.VisibleIndividuallyOnly(true)
				.BuildFacetMap(!isInstantSearch);

			// Visibility
			query.VisibleOnly(!QuerySettings.IgnoreAcl ? _services.WorkContext.CurrentCustomer : null);

			// Store
			if (!QuerySettings.IgnoreMultiStore)
			{
				query.HasStoreId(_services.StoreContext.CurrentStore.Id);
			}

			// Availability
			ConvertAvailability(query, routeData, origin);

			// Instant-Search never uses these filter parameters
			if (!isInstantSearch)
			{
				ConvertPagingSorting(query, routeData, origin);
				ConvertPrice(query, routeData, origin);
				ConvertCategory(query, routeData, origin);
				ConvertManufacturer(query, routeData, origin);
				ConvertRating(query, routeData, origin);
				ConvertNewArrivals(query, routeData, origin);
				ConvertDeliveryTime(query, routeData, origin);
			}

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
                if(origin.Equals("Search/Search"))
                {
                    orderBy = _searchSettings.DefaultSortOrder;
                }
                else
                {
                    orderBy = _catalogSettings.DefaultSortOrder;
                }
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
			FacetGroupKind kind,
			bool isMultiSelect,
			FacetSorting sorting,
			Action<FacetDescriptor> addValues)
		{
			string fieldName;
			var displayOrder = 0;

			switch (kind)
			{
				case FacetGroupKind.Category:
					fieldName = _catalogSettings.IncludeFeaturedProductsInNormalLists ? "categoryid" : "notfeaturedcategoryid";
					break;
				case FacetGroupKind.Brand:
					if (_searchSettings.BrandDisabled)
						return;
					fieldName = "manufacturerid";
					displayOrder = _searchSettings.BrandDisplayOrder;
					break;
				case FacetGroupKind.Price:
					if (_searchSettings.PriceDisabled || !_services.Permissions.Authorize(StandardPermissionProvider.DisplayPrices))
						return;
					fieldName = "price";
					displayOrder = _searchSettings.PriceDisplayOrder;
					break;
				case FacetGroupKind.Rating:
					if (_searchSettings.RatingDisabled)
						return;
					fieldName = "rating";
					displayOrder = _searchSettings.RatingDisplayOrder;
					break;
				case FacetGroupKind.DeliveryTime:
					if (_searchSettings.DeliveryTimeDisabled)
						return;
					fieldName = "deliveryid";
					displayOrder = _searchSettings.DeliveryTimeDisplayOrder;
					break;
				case FacetGroupKind.Availability:
					if (_searchSettings.AvailabilityDisabled)
						return;
					fieldName = "available";
					displayOrder = _searchSettings.AvailabilityDisplayOrder;
					break;
				case FacetGroupKind.NewArrivals:
					if (_searchSettings.NewArrivalsDisabled)
						return;
					fieldName = "createdon";
					displayOrder = _searchSettings.NewArrivalsDisplayOrder;
					break;
				default:
					throw new SmartException($"Unknown field name for facet group '{kind.ToString()}'");
			}

			var descriptor = new FacetDescriptor(fieldName);
			descriptor.Label = _services.Localization.GetResource(FacetDescriptor.GetLabelResourceKey(kind) ?? kind.ToString());
			descriptor.IsMultiSelect = isMultiSelect;
			descriptor.DisplayOrder = displayOrder;
			descriptor.OrderBy = sorting;
			descriptor.MinHitCount = _searchSettings.FilterMinHitCount;
			descriptor.MaxChoicesCount = _searchSettings.FilterMaxChoicesCount;

			addValues(descriptor);
			query.WithFacet(descriptor);
		}

		protected virtual void ConvertCategory(CatalogSearchQuery query, RouteData routeData, string origin)
		{
			if (origin == "Catalog/Category")
			{
				// we don't need category facetting in category pages
				return;
			}

			List<int> ids;

			if (GetValueFor(query, "c", FacetGroupKind.Category, out ids) && ids != null && ids.Any())
			{
				// TODO; (mc) Get deep ids (???) Make a low-level version of CatalogHelper.GetChildCategoryIds()
				query.WithCategoryIds(_catalogSettings.IncludeFeaturedProductsInNormalLists ? null : (bool?)false, ids.ToArray());
			}

			AddFacet(query, FacetGroupKind.Category, true, FacetSorting.HitsDesc, descriptor =>
			{
				if (ids != null)
				{
					foreach (var id in ids)
					{
						descriptor.AddValue(new FacetValue(id, IndexTypeCode.Int32)
						{
							IsSelected = true
						});
					}
				}
			});
		}

		protected virtual void ConvertManufacturer(CatalogSearchQuery query, RouteData routeData, string origin)
		{
			//List<int> ids = null;
			//int? minHitCount = null;

			//GetValueFor(query, "m", FacetGroupKind.Brand, out ids);

			//// Preselect manufacturer on manufacturer page.... and then?
			//if (origin.IsCaseInsensitiveEqual("Catalog/Manufacturer"))
			//{
			//	minHitCount = 0;

			//	var manufacturerId = routeData.Values["manufacturerid"].ToString().ToInt();
			//	if (manufacturerId != 0)
			//	{
			//		if (ids == null)
			//			ids = new List<int> { manufacturerId };
			//		else if (!ids.Contains(manufacturerId))
			//			ids.Add(manufacturerId);
			//	}
			//}

			if (origin.IsCaseInsensitiveEqual("Catalog/Manufacturer"))
			{
				// we don't need brand facetting in brand pages
				return;
			}

			List<int> ids;

			if (GetValueFor(query, "m", FacetGroupKind.Brand, out ids) && ids != null && ids.Any())
			{
				query.WithManufacturerIds(null, ids.ToArray());
			}

			AddFacet(query, FacetGroupKind.Brand, true, FacetSorting.LabelAsc, descriptor =>
			{
				if (ids != null)
				{
					foreach (var id in ids)
					{
						descriptor.AddValue(new FacetValue(id, IndexTypeCode.Int32)
						{
							IsSelected = true
						});
					}
				}
			});
		}

		protected virtual void ConvertPrice(CatalogSearchQuery query, RouteData routeData, string origin)
		{
			string price;
			double? minPrice = null;
			double? maxPrice = null;

			if (GetValueFor(query, "p", FacetGroupKind.Price, out price) && TryParseRange(price, out minPrice, out maxPrice))
			{
				var currency = _services.WorkContext.WorkingCurrency;

				if (minPrice.HasValue)
				{
					// TODO: (mc) Why the heck did I convert this??!!
					//minPrice = _currencyService.ConvertToPrimaryStoreCurrency(minPrice.Value, currency);
				}

				if (maxPrice.HasValue)
				{
					//maxPrice = _currencyService.ConvertToPrimaryStoreCurrency(maxPrice.Value, currency);
				}

				if (minPrice.HasValue && maxPrice.HasValue && minPrice > maxPrice)
				{
					var tmp = minPrice;
					minPrice = maxPrice;
					maxPrice = tmp;
				}

				if (minPrice.HasValue || maxPrice.HasValue)
				{
					query.PriceBetween((decimal?)minPrice, (decimal?)maxPrice);
				}
			}

			AddFacet(query, FacetGroupKind.Price, false, FacetSorting.DisplayOrder, descriptor =>
			{
				if (minPrice.HasValue || maxPrice.HasValue)
				{
					descriptor.AddValue(new FacetValue(
						minPrice,
						maxPrice,
						IndexTypeCode.Double,
						minPrice.HasValue,
						maxPrice.HasValue)
					{
						IsSelected = true
					});
				}
			});
		}

		protected virtual bool TryParseRange(string query, out double? min, out double? max)
		{
			min = max = null;

			if (query.IsEmpty())
			{
				return false;
			}

			// Format: from~to || from[~] || ~to
			var arr = query.Split('~').Select(x => x.Trim()).Take(2).ToArray();

			CommonHelper.TryConvert(arr[0], out min);
			if (arr.Length == 2)
			{
				CommonHelper.TryConvert(arr[1], out max);
			}

			return min != null || max != null;
		}

		protected virtual void ConvertRating(CatalogSearchQuery query, RouteData routeData, string origin)
		{
			double? fromRate;

			if (GetValueFor(query, "r", FacetGroupKind.Rating, out fromRate) && fromRate.HasValue)
			{
				query.WithRating(fromRate, null);
			}

			AddFacet(query, FacetGroupKind.Rating, false, FacetSorting.DisplayOrder, descriptor =>
			{
				descriptor.MinHitCount = 0;
				descriptor.MaxChoicesCount = 5;

				if (fromRate.HasValue)
				{
					descriptor.AddValue(new FacetValue(fromRate.Value, IndexTypeCode.Double)
					{
						IsSelected = true
					});
				}
			});
		}

		protected virtual void ConvertAvailability(CatalogSearchQuery query, RouteData routeData, string origin)
		{
			bool availability;
			GetValueFor(query, "a", FacetGroupKind.Availability, out availability);

			// Setting specifies the logical direction of the filter. That's smarter than just to specify a default value.
			if (_searchSettings.IncludeNotAvailable)
			{
				// False = show, True = hide unavailable products.
				if (availability)
				{
					query.AvailableOnly(true);
				}
			}
			else
			{
				// False = hide, True = show unavailable products.
				if (!availability)
				{
					query.AvailableOnly(true);
				}
			}

			AddFacet(query, FacetGroupKind.Availability, true, FacetSorting.LabelAsc, descriptor =>
			{
				descriptor.MinHitCount = 0;

				var newValue = availability
					? new FacetValue(true, IndexTypeCode.Boolean)
					: new FacetValue(null, IndexTypeCode.Empty);

				newValue.IsSelected = availability;
				newValue.Label = _services.Localization.GetResource(_searchSettings.IncludeNotAvailable ? "Search.Facet.ExcludeOutOfStock" : "Search.Facet.IncludeOutOfStock");

				descriptor.AddValue(newValue);
			});
		}

		protected virtual void ConvertNewArrivals(CatalogSearchQuery query, RouteData routeData, string origin)
		{
			var newForMaxDays = _catalogSettings.LabelAsNewForMaxDays ?? 0;
			if (newForMaxDays <= 0)
			{
				// we cannot filter without it
				return;
			}

			bool newArrivalsOnly;
			var fromUtc = DateTime.UtcNow.Subtract(TimeSpan.FromDays(newForMaxDays));

			if (GetValueFor(query, "n", FacetGroupKind.NewArrivals, out newArrivalsOnly) && newArrivalsOnly)
			{
				query.CreatedBetween(fromUtc, null);
			}

			AddFacet(query, FacetGroupKind.NewArrivals, true, FacetSorting.LabelAsc, descriptor =>
			{
				descriptor.AddValue(new FacetValue(fromUtc, null, IndexTypeCode.DateTime, true, false)
				{
					IsSelected = newArrivalsOnly,
					Label = _services.Localization.GetResource("Search.Facet.LastDays").FormatInvariant(newForMaxDays)
				});
			});
		}

		protected virtual void ConvertDeliveryTime(CatalogSearchQuery query, RouteData routeData, string origin)
		{
			List<int> ids;

			if (GetValueFor(query, "d", FacetGroupKind.DeliveryTime, out ids) && ids != null && ids.Any())
			{
				query.WithDeliveryTimeIds(ids.ToArray());
			}

			AddFacet(query, FacetGroupKind.DeliveryTime, true, FacetSorting.DisplayOrder, descriptor =>
			{
				if (ids != null)
				{
					foreach (var id in ids)
					{
						descriptor.AddValue(new FacetValue(id, IndexTypeCode.Int32)
						{
							IsSelected = true
						});
					}
				}
			});
		}

		protected virtual void OnConverted(CatalogSearchQuery query, RouteData routeData, string origin)
		{
		}

		protected T GetValueFor<T>(string key)
		{
			T value;
			return GetValueFor(key, out value) ? value : default(T);
		}

		protected bool GetValueFor<T>(string key, out T value)
		{
			var strValue = _httpContext.Request?.Unvalidated.Form?[key] ?? _httpContext.Request?.Unvalidated.QueryString?[key];

			if (strValue != null)
			{
				value = strValue.Convert<T>();
				return true;
			}

			value = default(T);
			return false;
		}

		protected bool GetValueFor<T>(CatalogSearchQuery query, string key, FacetGroupKind kind, out T value)
		{
			return GetValueFor(_catalogSearchQueryAliasMapper.GetCommonFacetAliasByGroupKind(kind, query.LanguageId ?? 0) ?? key, out value);
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
							foreach (var key in form.AllKeys)
							{
								if (key.HasValue() && !_tokens.Contains(key))
								{
									_aliases.AddRange(key, form[key].SplitSafe(","));
								}
							}
						}

						if (query != null)
						{
							foreach (var key in query.AllKeys)
							{
								if (key.HasValue() && !_tokens.Contains(key))
								{
									_aliases.AddRange(key, query[key].SplitSafe(","));
								}
							}
						}
					}
				}

				return _aliases;
			}
		}
	}
}
