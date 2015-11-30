using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Services.Catalog
{
    public class ProductSearchContext
    {
        public ProductSearchContext()
        {
            CategoryIds = new List<int>();
            FilteredSpecs = new List<int>();
            FilterableSpecificationAttributeOptionIds = new List<int>();
            PageSize = 12;
        }

		/// <summary>
		/// Optional query to use to build the product query. Otherwise the repository of the product service is used (default). 
		/// </summary>
		public IQueryable<Product> Query { get; set; }

        /// <summary>
        /// Category identifiers
        /// </summary>
        public IList<int> CategoryIds { get; set; }

		/// <summary>
		/// Filter by product identifiers
		/// </summary>
		/// <remarks>Only implemented in LINQ mode at the moment</remarks>
		public IList<int> ProductIds { get; set; }

        /// <summary>
        /// A value indicating whether ALL given <see cref="CategoryIds"/> must be assigned to the resulting products (default is ANY)
        /// </summary>
        /// <remarks>Only works in LINQ mode at the moment</remarks>
        public bool MatchAllcategories { get; set; }

		/// <summary>
		/// A value indicating whether to load products without any catgory mapping
		/// </summary>
		public bool? WithoutCategories { get; set; }

        /// <summary>
        /// Manufacturer identifier; 0 to load all records
        /// </summary>
        public int ManufacturerId { get; set; }

		/// <summary>
		/// A value indicating whether to load products without any manufacturer mapping
		/// </summary>
		public bool? WithoutManufacturers { get; set; }

        /// <summary>
        /// A value indicating whether loaded products are marked as featured (relates only to categories and manufacturers). 0 to load featured products only, 1 to load not featured products only, null to load all products
        /// </summary>
        public bool? FeaturedProducts { get; set; }

        /// <summary>
        /// Minimum price; null to load all records
        /// </summary>
        public decimal? PriceMin { get; set; }

        /// <summary>
        /// Maximum price; null to load all records
        /// </summary>
        public decimal? PriceMax { get; set; }

        /// <summary>
        /// Product tag identifier; 0 to load all records
        /// </summary>
        public int ProductTagId { get; set; }

        /// <summary>
        /// Keywords
        /// </summary>
        public string Keywords { get; set; }

        /// <summary>
        /// A value indicating whether to search in descriptions
        /// </summary>
        public bool SearchDescriptions { get; set; }

        /// <summary>
        /// A value indicating whether to search by a specified "keyword" in product tags
        /// </summary>
        public bool SearchProductTags { get; set; }
        
        /// <summary>
        /// Language identifier
        /// </summary>
        public int LanguageId { get; set; }

        /// <summary>
        /// Filtered product specification identifiers
        /// </summary>
        public IList<int> FilteredSpecs { get; set; }

        /// <summary>
        /// Order by
        /// </summary>
        public ProductSortingEnum OrderBy { get; set; }

        /// <summary>
        /// Page index
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// Page size
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// A value indicating whether we should load the specification attribute option identifiers applied to loaded products (all pages)
        /// </summary>
        public bool LoadFilterableSpecificationAttributeOptionIds { get; set; }

        /// <summary>
        /// The specification attribute option identifiers applied to loaded products (all pages)
        /// </summary>
        public IList<int> FilterableSpecificationAttributeOptionIds { get; set; }

        /// <summary>
        /// A value indicating whether to show hidden records
        /// </summary>
        public bool ShowHidden { get; set; }

		/// <summary>
		/// Store identifier; 0 to load all records
		/// </summary>
		public int StoreId { get; set; }

		/// <summary>
		/// Parent product identifier (used with grouped products); 0 to load all records
		/// </summary>
		public int ParentGroupedProductId { get; set; }

		/// <summary>
		/// A values indicating whether to load only products marked as "visible individually"; "false" to load all records; "true" to load "visible individually" only
		/// </summary>
		public bool VisibleIndividuallyOnly { get; set; }

		/// <summary>
		/// Product type; 0 to load all records
		/// </summary>
		public ProductType? ProductType { get; set; }

		/// <summary>
		/// A value indicating whether to search by a specified "Keyword" in product SKU
		/// </summary>
		public bool SearchSku { get; set; }

        /// <summary>
        /// Any value indicating the origin of the search request,
        /// e.g. the category id, if the caller is is a category page.
        /// Can be useful in customization scenarios.
        /// </summary>
        public string Origin { get; set; }

		/// <summary>
		/// A value indicating whether to load only published or non published products
		/// </summary>
		public bool? IsPublished { get; set; }

		/// <summary>
		/// A value indicating whether to load only products displayed on the homepage
		/// </summary>
		public bool? HomePageProducts { get; set; }
    }
}
