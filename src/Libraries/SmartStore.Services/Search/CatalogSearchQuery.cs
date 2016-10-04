using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Domain.Catalog;
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

		public CatalogSearchQuery(string field, string term, bool escape = false, bool isFuzzySearch = false)
			: base(field.HasValue() ? new[] { field } : null, term, escape, isFuzzySearch)
		{
		}

		public CatalogSearchQuery(string[] fields, string term, bool escape = false, bool isFuzzySearch = false)
			: base(fields, term, escape, isFuzzySearch)
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

		public CatalogSearchQuery NotShowHidden(List<int> allowedCustomerRolesIds)
		{
			var utcNow = DateTime.UtcNow;

			WithFilter(SearchFilter.ByField("published", true).Mandatory());

			WithFilter(SearchFilter.ByRange("availablestart", null, utcNow, false, false).Mandatory());
			WithFilter(SearchFilter.ByRange("availableend", utcNow, null, false, false).Mandatory());

			WithFilter(SearchFilter.ByField("subjecttoacl", false));

			if (allowedCustomerRolesIds != null)
			{
				allowedCustomerRolesIds.Each(x => WithFilter(SearchFilter.ByField("roleid", x)));
			}

			return this;
		}

		public CatalogSearchQuery PublishedOnly(bool value)
		{
			return WithFilter(SearchFilter.ByField("published", value).Mandatory());
		}

		public CatalogSearchQuery FeaturedOnly(bool value)
		{
			return WithFilter(SearchFilter.ByField("featured", value).Mandatory());
		}

		public CatalogSearchQuery VisibleIndividuallyOnly(bool value)
		{
			return WithFilter(SearchFilter.ByField("visibleindividually", value).Mandatory());
		}

		public CatalogSearchQuery HomePageProductsOnly(bool value)
		{
			return WithFilter(SearchFilter.ByField("showonhomepage", value).Mandatory());
		}

		public CatalogSearchQuery IsParentGroupedProductId(int id)
		{
			return WithFilter(SearchFilter.ByField("parentid", id).Mandatory());
		}

		public CatalogSearchQuery IsStoreId(int id)
		{
			return WithFilter(SearchFilter.ByField("storeid", id).Mandatory());
		}

		public CatalogSearchQuery IsProductType(ProductType type)
		{
			return WithFilter(SearchFilter.ByField("typeid", (int)type).Mandatory());
		}

		public CatalogSearchQuery WithProductIds(params int[] ids)
		{
			ids.Each(x => WithFilter(SearchFilter.ByField("id", x)));
			return this;
		}

		public CatalogSearchQuery WithProductId(int? fromId, int? toId)
		{
			return WithFilter(SearchFilter.ByRange("id", fromId, toId, fromId.HasValue, toId.HasValue).Mandatory());
		}

		public CatalogSearchQuery WithCategoryIds(params int[] ids)
		{
			ids.Each(x => WithFilter(SearchFilter.ByField("categoryid", x)));
			return this;
		}

		public CatalogSearchQuery HasAnyCategories(bool value)
		{
			if (value)
			{
				WithFilter(SearchFilter.ByRange("categoryid", 1, int.MaxValue, true, true).Mandatory());
			}
			else
			{
				// TODO: how, index id 0?
				//WithFilter(SearchFilter.ByRange("categoryid", 1, int.MaxValue, true, true).Forbidden());
			}

			return this;
		}

		public CatalogSearchQuery WithManufacturerIds(params int[] ids)
		{
			ids.Each(x => WithFilter(SearchFilter.ByField("manufacturerid", x)));
			return this;
		}

		public CatalogSearchQuery HasAnyManufacturers(bool value)
		{
			if (value)
			{
				WithFilter(SearchFilter.ByRange("manufacturerid", 1, int.MaxValue, true, true).Mandatory());
			}
			else
			{
				// TODO: how, index id 0?
				//WithFilter(SearchFilter.ByRange("manufacturerid", 1, int.MaxValue, true, true).Forbidden());
			}

			return this;
		}

		public CatalogSearchQuery WithProductTagIds(params int[] ids)
		{
			ids.Each(x => WithFilter(SearchFilter.ByField("tagid", x)));
			return this;
		}

		public CatalogSearchQuery WithStockQuantity(int? fromQuantity, int? toQuantity)
		{
			return WithFilter(SearchFilter.ByRange("stockquantity", fromQuantity, toQuantity, fromQuantity.HasValue, toQuantity.HasValue).Mandatory());
		}

		public CatalogSearchQuery WithPrice(decimal? fromPrice, decimal? toPrice)
		{
			// TODO: how?
			WithFilter(SearchFilter.ByRange("price",
				fromPrice.HasValue ? decimal.ToDouble(fromPrice.Value) : (double?)null,
				toPrice.HasValue ? decimal.ToDouble(toPrice.Value) : (double?)null,
				fromPrice.HasValue,
				toPrice.HasValue).Mandatory());

			return this;
		}

		public CatalogSearchQuery WithCreatedUtc(DateTime? fromUtc, DateTime? toUtc)
		{
			return WithFilter(SearchFilter.ByRange("createdon", fromUtc, toUtc, fromUtc.HasValue, toUtc.HasValue).Mandatory());
		}

		#endregion
	}
}
