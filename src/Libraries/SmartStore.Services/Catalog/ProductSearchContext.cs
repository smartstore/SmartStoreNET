using System;
using System.Collections.Generic;
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
        /// Category identifiers
        /// </summary>
        public IList<int> CategoryIds { get; set; }

        /// <summary>
        /// Manufacturer identifier; 0 to load all records
        /// </summary>
        public int ManufacturerId { get; set; }

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
		/// Current store id
		/// </summary>
		public int CurrentStoreId { get; set; }
    }
}
