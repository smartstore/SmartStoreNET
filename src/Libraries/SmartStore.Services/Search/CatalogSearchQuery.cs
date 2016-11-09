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

		public CatalogSearchQuery(string field, string term, bool escape = false, bool isExactMatch = false, bool isFuzzySearch = false)
			: base(field.HasValue() ? new[] { field } : null, term, escape, isExactMatch, isFuzzySearch)
		{
		}

		public CatalogSearchQuery(string[] fields, string term, bool escape = false, bool isExactMatch = false, bool isFuzzySearch = false)
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

				case ProductSortingEnum.Position:
					return SortBy(SearchSort.ByRelevance());

				default:
					return this;
			}			
		}

		/// <summary>
		/// Only products that are visible in frontend
		/// </summary>
		/// <param name="customer">Customer whose customer roles should be checked</param>
		/// <returns>Catalog search query</returns>
		public CatalogSearchQuery VisibleOnly(Customer customer)
		{
			var allowedCustomerRoleIds = customer.CustomerRoles.Where(x => x.Active).Select(x => x.Id).ToArray();

			return VisibleOnly(allowedCustomerRoleIds);
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

			var roleIds = (allowedCustomerRoleIds != null ? allowedCustomerRoleIds.Where(x => x != 0).Distinct().ToList() : new List<int>());
			var roleIdCount = roleIds.Count;

			if (roleIdCount > 0)
			{
				if (roleIdCount == 1)
				{
					WithFilter(SearchFilter.ByField("roleid", roleIds.First()).Mandatory().ExactMatch().NotAnalyzed());
				}
				else
				{
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

		public CatalogSearchQuery HasStoreId(int id)
		{
			return WithFilter(SearchFilter.ByField("storeid", id).Mandatory().ExactMatch().NotAnalyzed());
		}

		public CatalogSearchQuery IsProductType(ProductType type)
		{
			return WithFilter(SearchFilter.ByField("typeid", (int)type).Mandatory().ExactMatch().NotAnalyzed());
		}

		public CatalogSearchQuery WithProductIds(params int[] ids)
		{
			ids.Each(x => WithFilter(SearchFilter.ByField("id", x).ExactMatch().NotAnalyzed()));
			return this;
		}

		public CatalogSearchQuery WithProductId(int? fromId, int? toId)
		{
			return WithFilter(SearchFilter.ByRange("id", fromId, toId, fromId.HasValue, toId.HasValue).Mandatory().ExactMatch().NotAnalyzed());
		}

		public CatalogSearchQuery WithCategoryIds(bool? featuredOnly, params int[] ids)
		{
			string fieldName = null;

			if (featuredOnly.HasValue)
				fieldName = (featuredOnly.Value ? "featuredcategoryid" : "notfeaturedcategoryid");
			else
				fieldName = "categoryid";

			ids.Each(x => WithFilter(SearchFilter.ByField(fieldName, x).ExactMatch().NotAnalyzed()));
			return this;
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

			ids.Each(x => WithFilter(SearchFilter.ByField(fieldName, x).ExactMatch().NotAnalyzed()));
			return this;
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
			ids.Each(x => WithFilter(SearchFilter.ByField("tagid", x).ExactMatch().NotAnalyzed()));
			return this;
		}

		public CatalogSearchQuery WithStockQuantity(int? fromQuantity, int? toQuantity)
		{
			return WithFilter(SearchFilter.ByRange("stockquantity", fromQuantity, toQuantity, fromQuantity.HasValue, toQuantity.HasValue).Mandatory().ExactMatch().NotAnalyzed());
		}

		public CatalogSearchQuery WithPrice(Currency currency, decimal? fromPrice, decimal? toPrice)
		{
			Guard.NotNull(currency, nameof(currency));

			var fieldName = "price_c-" + currency.CurrencyCode.EmptyNull().ToLower();

			return WithFilter(SearchFilter.ByRange(fieldName,
				fromPrice.HasValue ? decimal.ToDouble(fromPrice.Value) : (double?)null,
				toPrice.HasValue ? decimal.ToDouble(toPrice.Value) : (double?)null,
				fromPrice.HasValue,
				toPrice.HasValue).Mandatory().ExactMatch().NotAnalyzed());
		}

		public CatalogSearchQuery WithCreatedUtc(DateTime? fromUtc, DateTime? toUtc)
		{
			return WithFilter(SearchFilter.ByRange("createdon", fromUtc, toUtc, fromUtc.HasValue, toUtc.HasValue).Mandatory().ExactMatch().NotAnalyzed());
		}

		#endregion
	}
}
