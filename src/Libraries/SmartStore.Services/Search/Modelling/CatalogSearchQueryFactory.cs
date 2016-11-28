using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Search;
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

			var area = ctx.Request.RequestContext.RouteData.GetAreaName();
			var controller = ctx.Request.RequestContext.RouteData.GetRequiredString("controller");
			var action = ctx.Request.RequestContext.RouteData.GetRequiredString("action");
			string origin = "{0}{1}/{2}".FormatInvariant(area == null ? "" : area + "/", controller, action);

			var term = GetValueFor<string>("q");

			var fields = new List<string> { "name" };
			fields.AddRange(_searchSettings.SearchFields);

			var query = new CatalogSearchQuery(fields.ToArray(), term)
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

			ConvertPagingSorting(query);
			ConvertPrice(query);
			ConvertCategory(query);
			ConvertManufacturer(query);
			ConvertRating(query);

			OnConverted(query);

			this.Current = query;

			return query;
		}

		protected virtual void ConvertPagingSorting(CatalogSearchQuery query)
		{
			var size = GetValueFor<int?>("s");
			var index = Math.Max(1, GetValueFor<int?>("i") ?? 1);

			if (size == null)
			{
				// TODO: (mc) In category pages, get current category default page size
				size = _catalogSettings.DefaultProductListPageSize;
			}

			query.Slice((index - 1) * size.Value, size.Value);

			var orderBy = GetValueFor<ProductSortingEnum?>("o");
			if (orderBy == null || orderBy == ProductSortingEnum.Initial)
			{
				orderBy = _catalogSettings.DefaultSortOrder;
			}

			query.SortBy(orderBy.Value);
		}

		protected virtual void ConvertCategory(CatalogSearchQuery query)
		{
			var ids = GetValueFor<List<int>>("c");
			if (ids != null && ids.Any())
			{
				// TODO; (mc) Get deep ids (???) Make a low-level version of CatalogHelper.GetChildCategoryIds()
				query.WithCategoryIds(null, ids.ToArray());
			}
		}

		protected virtual void ConvertManufacturer(CatalogSearchQuery query)
		{
			var ids = GetValueFor<List<int>>("m");
			if (ids != null && ids.Any())
			{
				query.WithManufacturerIds(null, ids.ToArray());
			}
		}

		protected virtual void ConvertPrice(CatalogSearchQuery query)
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

		protected virtual void ConvertRating(CatalogSearchQuery query)
		{
		}

		protected virtual void ConvertStock(CatalogSearchQuery query)
		{
		}

		protected virtual void ConvertDeliveryTime(CatalogSearchQuery query)
		{
		}

		protected virtual void OnConverted(CatalogSearchQuery query)
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
