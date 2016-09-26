using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

		public CatalogSearchQuery WithManufacturerId(int id)
		{
			WithFilter(SearchFilter.ByField("ManufacturerId", id));
			return this;
		}

		public CatalogSearchQuery WithCategoryIds(params int[] ids)
		{
			// [...]

			return this;
		}

		public CatalogSearchQuery IsProductType(ProductType type)
		{
			// [...]

			return this;
		}

		public CatalogSearchQuery PublishedOnly(bool value)
		{
			// [...]
			
			return this;
		}

		public CatalogSearchQuery FeaturedOnly(bool value)
		{
			// [...]

			return this;
		}

		public CatalogSearchQuery CreatedFromUtc(DateTime fromUtc)
		{
			// [...]

			return this;
		}

		public CatalogSearchQuery CreatedToUtc(DateTime fromUtc)
		{
			// [...]

			return this;
		}

		// [...]

		#endregion
	}
}
