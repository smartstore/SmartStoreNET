using System;
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
					return SortBy(SearchSort.ByDateTimeField("CreatedOnUtc", sort == ProductSortingEnum.CreatedOn));

				case ProductSortingEnum.NameAsc:
				case ProductSortingEnum.NameDesc:
					return SortBy(SearchSort.ByStringField("Name", sort == ProductSortingEnum.NameDesc));

				case ProductSortingEnum.PriceAsc:
				case ProductSortingEnum.PriceDesc:
					return SortBy(SearchSort.ByDoubleField("Price", sort == ProductSortingEnum.PriceDesc));

				case ProductSortingEnum.Position:
					return SortBy(SearchSort.ByRelevance());

				default:
					return this;
			}			
		}

		public CatalogSearchQuery ShowHidden(bool value)
		{
			return WithFilter(SearchFilter.ByField("_ShowHidden", value));
		}

		public CatalogSearchQuery PublishedOnly(bool value)
		{
			return WithFilter(SearchFilter.ByField("Published", value));
		}

		public CatalogSearchQuery FeaturedOnly(bool value)
		{
			return WithFilter(SearchFilter.ByField("_FeaturedOnly", value));
		}

		public CatalogSearchQuery VisibleIndividuallyOnly(bool value)
		{
			return WithFilter(SearchFilter.ByField("VisibleIndividually", value));
		}

		public CatalogSearchQuery HomePageProductsOnly(bool value)
		{
			return WithFilter(SearchFilter.ByField("ShowOnHomePage", value));
		}

		public CatalogSearchQuery IsParentGroupedProductId(int id)
		{
			return WithFilter(SearchFilter.ByField("ParentGroupedProductId", id));
		}

		public CatalogSearchQuery IsStoreId(int id)
		{
			return WithFilter(SearchFilter.ByField("_StoreId", id));
		}

		public CatalogSearchQuery IsProductType(ProductType type)
		{
			return WithFilter(SearchFilter.ByField("ProductTypeId", (int)type));
		}

		public CatalogSearchQuery WithProductIds(params int[] ids)
		{
			ids.Each(x => WithFilter(SearchFilter.ByField("id", x)));
			return this;
		}

		public CatalogSearchQuery WithProductId(int? fromId, int? toId)
		{
			return WithFilter(SearchFilter.ByRange("Id", fromId, toId, fromId.HasValue, toId.HasValue));
		}

		public CatalogSearchQuery WithCategoryIds(params int[] ids)
		{
			ids.Each(x => WithFilter(SearchFilter.ByField("ProductCategories.CategoryId", x)));
			return this;
		}

		public CatalogSearchQuery HasAnyCategories(bool value)
		{
			return WithFilter(SearchFilter.ByField("ProductCategories._Any", value));
		}

		public CatalogSearchQuery WithManufacturerIds(params int[] ids)
		{
			ids.Each(x => WithFilter(SearchFilter.ByField("ProductManufacturers.ManufacturerId", x)));
			return this;
		}

		public CatalogSearchQuery HasAnyManufacturers(bool value)
		{
			return WithFilter(SearchFilter.ByField("ProductManufacturers._Any", value));
		}

		public CatalogSearchQuery WithProductTagIds(params int[] ids)
		{
			ids.Each(x => WithFilter(SearchFilter.ByField("ProductTags.Id", x)));
			return this;
		}

		public CatalogSearchQuery WithStockQuantity(int? fromQuantity, int? toQuantity)
		{
			return WithFilter(SearchFilter.ByRange("StockQuantity", fromQuantity, toQuantity, fromQuantity.HasValue, toQuantity.HasValue));
		}

		public CatalogSearchQuery WithPrice(decimal? fromPrice, decimal? toPrice)
		{
			return WithFilter(SearchFilter.ByRange("Price",
				fromPrice.HasValue ? decimal.ToDouble(fromPrice.Value) : (double?)null,
				toPrice.HasValue ? decimal.ToDouble(toPrice.Value) : (double?)null,
				fromPrice.HasValue,
				toPrice.HasValue));
		}

		public CatalogSearchQuery WithCreatedUtc(DateTime? fromUtc, DateTime? toUtc)
		{
			return WithFilter(SearchFilter.ByRange("CreatedOnUtc", fromUtc, toUtc, fromUtc.HasValue, toUtc.HasValue));
		}

		#endregion
	}
}
