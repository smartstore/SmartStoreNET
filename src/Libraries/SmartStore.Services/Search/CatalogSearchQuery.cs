using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Search;

namespace SmartStore.Services.Search
{
	public partial class CatalogSearchQuery : SearchQuery<CatalogSearchQuery>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CatalogSearchQuery"/> class without a search term being set
		/// </summary>
		public CatalogSearchQuery()
			: base((string[])null, null)
		{
		}

		public CatalogSearchQuery(string field, string term, bool escape = true, bool isExactMatch = false, bool isFuzzySearch = false)
			: base(field.HasValue() ? new[] { field } : null, term, escape, isExactMatch, isFuzzySearch)
		{
		}

		public CatalogSearchQuery(string[] fields, string term, bool escape = true, bool isExactMatch = false, bool isFuzzySearch = false)
			: base(fields, term, escape, isExactMatch, isFuzzySearch)
		{
		}

		#region Fluent builder

		public CatalogSearchQuery SortBy(ProductSortingEnum sort)
		{
			switch (sort)
			{
				case ProductSortingEnum.CreatedOnAsc:
				case ProductSortingEnum.CreatedOn:
					return SortBy(SearchSort.ByDateTimeField("createdon", sort == ProductSortingEnum.CreatedOn));

				case ProductSortingEnum.NameAsc:
				case ProductSortingEnum.NameDesc:
					return SortBy(SearchSort.ByStringField("name", sort == ProductSortingEnum.NameDesc));

				case ProductSortingEnum.PriceAsc:
				case ProductSortingEnum.PriceDesc:
					return SortBy(SearchSort.ByDoubleField("price", sort == ProductSortingEnum.PriceDesc));

				case ProductSortingEnum.Relevance:
					return SortBy(SearchSort.ByRelevance());

				default:
					return this;
			}			
		}

		/// <summary>
		/// Only products that are visible in frontend
		/// </summary>
		/// <param name="customer">Customer whose customer roles should be checked, can be <c>null</c></param>
		/// <returns>Catalog search query</returns>
		public CatalogSearchQuery VisibleOnly(Customer customer)
		{
			if (customer != null)
			{
				var allowedCustomerRoleIds = customer.CustomerRoles.Where(x => x.Active).Select(x => x.Id).ToArray();

				return VisibleOnly(allowedCustomerRoleIds);
			}

			return VisibleOnly(new int[0]);
		}

		/// <summary>
		/// Only products that are visible in frontend
		/// </summary>
		/// <param name="allowedCustomerRoleIds">Allowed customer role id, can be <c>null</c></param>
		/// <returns>Catalog search query</returns>
		public CatalogSearchQuery VisibleOnly(params int[] allowedCustomerRoleIds)
		{
			var utcNow = DateTime.UtcNow;

			PublishedOnly(true);

			WithFilter(SearchFilter.ByRange("availablestart", null, utcNow, false, false).Mandatory().NotAnalyzed());
			WithFilter(SearchFilter.ByRange("availableend", utcNow, null, false, false).Mandatory().NotAnalyzed());

			if (allowedCustomerRoleIds != null && allowedCustomerRoleIds.Length > 0)
			{
				var roleIds = allowedCustomerRoleIds.Where(x => x != 0).Distinct().ToList();
				if (roleIds.Any())
				{
					roleIds.Insert(0, 0);
					WithFilter(SearchFilter.Combined(roleIds.Select(x => SearchFilter.ByField("roleid", x).ExactMatch().NotAnalyzed()).ToArray()));
				}
			}

			return this;
		}

		public CatalogSearchQuery PublishedOnly(bool value)
		{
			return WithFilter(SearchFilter.ByField("published", true).Mandatory().ExactMatch().NotAnalyzed());
		}

		public CatalogSearchQuery VisibleIndividuallyOnly(bool value)
		{
			return WithFilter(SearchFilter.ByField("visibleindividually", value).Mandatory().ExactMatch().NotAnalyzed());
		}

		public CatalogSearchQuery HomePageProductsOnly(bool value)
		{
			return WithFilter(SearchFilter.ByField("showonhomepage", value).Mandatory().ExactMatch().NotAnalyzed());
		}

		public CatalogSearchQuery HasParentGroupedProductId(int id)
		{
			return WithFilter(SearchFilter.ByField("parentid", id).Mandatory().ExactMatch().NotAnalyzed());
		}

		public override CatalogSearchQuery HasStoreId(int id)
		{
			base.HasStoreId(id);

			return WithFilter(SearchFilter.Combined(
				SearchFilter.ByField("storeid", 0).ExactMatch().NotAnalyzed(),
				SearchFilter.ByField("storeid", id).ExactMatch().NotAnalyzed())
			);
		}

		public CatalogSearchQuery IsProductType(ProductType type)
		{
			return WithFilter(SearchFilter.ByField("typeid", (int)type).Mandatory().ExactMatch().NotAnalyzed());
		}

		public CatalogSearchQuery WithProductIds(params int[] ids)
		{
			return WithFilter(SearchFilter.Combined(ids.Select(x => SearchFilter.ByField("id", x).ExactMatch().NotAnalyzed()).ToArray()));
		}

		public CatalogSearchQuery WithProductId(int? fromId, int? toId)
		{
			return WithFilter(SearchFilter.ByRange("id", fromId, toId, fromId.HasValue, toId.HasValue).Mandatory().ExactMatch().NotAnalyzed());
		}

		/// <summary>
		/// Category ids filter
		/// </summary>
		/// <param name="featuredOnly">
		/// A value indicating whether loaded products are marked as featured (relates only to categories and manufacturers). 
		/// <c>false</c> to load featured products only, <c>true</c> to load unfeatured products only, <c>null</c> to load all products
		/// </param>
		/// <param name="ids">The category ids</param>
		/// <returns>Query</returns>
		public CatalogSearchQuery WithCategoryIds(bool? featuredOnly, params int[] ids)
		{
			if (ids.Length == 0)
			{
				return this;
			}

			string fieldName = null;

			if (featuredOnly.HasValue)
				fieldName = (featuredOnly.Value ? "featuredcategoryid" : "notfeaturedcategoryid");
			else
				fieldName = "categoryid";

			return WithFilter(SearchFilter.Combined(ids.Select(x => SearchFilter.ByField(fieldName, x).ExactMatch().NotAnalyzed()).ToArray()));
		}

		public CatalogSearchQuery HasAnyCategory(bool value)
		{
			if (value)
			{
				return WithFilter(SearchFilter.ByRange("categoryid", 1, int.MaxValue, true, true).Mandatory().NotAnalyzed());
			}
			else
			{
				return WithFilter(SearchFilter.ByField("categoryid", 0).Mandatory().ExactMatch().NotAnalyzed());
			}
		}

		public CatalogSearchQuery WithManufacturerIds(bool? featuredOnly, params int[] ids)
		{
			string fieldName = null;

			if (featuredOnly.HasValue)
				fieldName = (featuredOnly.Value ? "featuredmanufacturerid" : "notfeaturedmanufacturerid");
			else
				fieldName = "manufacturerid";

			return WithFilter(SearchFilter.Combined(ids.Select(x => SearchFilter.ByField(fieldName, x).ExactMatch().NotAnalyzed()).ToArray()));
		}

		public CatalogSearchQuery HasAnyManufacturer(bool value)
		{
			if (value)
			{
				return WithFilter(SearchFilter.ByRange("manufacturerid", 1, int.MaxValue, true, true).Mandatory().NotAnalyzed());
			}
			else
			{
				return WithFilter(SearchFilter.ByField("manufacturerid", 0).Mandatory().ExactMatch().NotAnalyzed());
			}
		}

		public CatalogSearchQuery WithProductTagIds(params int[] ids)
		{
			return WithFilter(SearchFilter.Combined(ids.Select(x => SearchFilter.ByField("tagid", x).ExactMatch().NotAnalyzed()).ToArray()));
		}

		public CatalogSearchQuery WithStockQuantity(int? fromQuantity, int? toQuantity)
		{
			return WithFilter(SearchFilter.ByRange("stockquantity", fromQuantity, toQuantity, fromQuantity.HasValue, toQuantity.HasValue).Mandatory().ExactMatch().NotAnalyzed());
		}

		public CatalogSearchQuery PriceBetween(decimal? fromPrice, decimal? toPrice, Currency currency)
		{
			Guard.NotNull(currency, nameof(currency));

			var fieldName = "price_c-" + currency.CurrencyCode.EmptyNull().ToLower();

			return WithFilter(SearchFilter.ByRange(fieldName,
				fromPrice.HasValue ? decimal.ToDouble(fromPrice.Value) : (double?)null,
				toPrice.HasValue ? decimal.ToDouble(toPrice.Value) : (double?)null,
				fromPrice.HasValue,
				toPrice.HasValue).Mandatory().ExactMatch().NotAnalyzed());
		}

		public CatalogSearchQuery CreatedBetween(DateTime? fromUtc, DateTime? toUtc)
		{
			return WithFilter(SearchFilter.ByRange("createdon", fromUtc, toUtc, fromUtc.HasValue, toUtc.HasValue).Mandatory().ExactMatch().NotAnalyzed());
		}

		public CatalogSearchQuery WithRating(double? fromRate, double? toRate)
		{
			if (fromRate.HasValue)
			{
				Guard.InRange(fromRate.Value, 0.0, 5.0, nameof(fromRate.Value));
			}
			if (toRate.HasValue)
			{
				Guard.InRange(toRate.Value, 0.0, 5.0, nameof(toRate.Value));
			}

			return WithFilter(SearchFilter.ByRange("rate", fromRate, toRate, fromRate.HasValue, toRate.HasValue).Mandatory().ExactMatch().NotAnalyzed());
		}

		#endregion
	}
}
