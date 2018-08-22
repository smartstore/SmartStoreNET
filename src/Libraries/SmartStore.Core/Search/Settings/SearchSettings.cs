using System.Collections.Generic;
using SmartStore.Core.Configuration;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Core.Search
{
	public class SearchSettings : ISettings
	{
		public SearchSettings()
		{
			SearchMode = SearchMode.Contains;
			SearchFields = new List<string> { "sku", "shortdescription", "tagname", "manufacturer", "category" };
			InstantSearchEnabled = true;
			ShowProductImagesInInstantSearch = true;
			InstantSearchNumberOfProducts = 10;
			InstantSearchTermMinLength = 2;
			FilterMinHitCount = 1;
			FilterMaxChoicesCount = 20;
            DefaultSortOrder = ProductSortingEnum.Relevance;

			BrandDisplayOrder = 1;
			PriceDisplayOrder = 2;
			RatingDisplayOrder = 3;
			DeliveryTimeDisplayOrder = 4;
			AvailabilityDisplayOrder = 5;
			NewArrivalsDisplayOrder = 6;
		}

		/// <summary>
		/// Gets or sets the search mode
		/// </summary>
		public SearchMode SearchMode { get; set; }

		/// <summary>
		/// Gets or sets name of fields to be searched. The name field is always searched.
		/// </summary>
		public List<string> SearchFields { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether instant-search is enabled
		/// </summary>
		public bool InstantSearchEnabled { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to show product images in instant-search result
		/// </summary>
		public bool ShowProductImagesInInstantSearch { get; set; }

		/// <summary>
		/// Gets or sets the number of products to return when using "instant-search" feature
		/// </summary>
		public int InstantSearchNumberOfProducts { get; set; }

		/// <summary>
		/// Gets or sets a minimum instant-search term length
		/// </summary>
		public int InstantSearchTermMinLength { get; set; }

		/// <summary>
		/// Gets or sets the minimum hit count for a filter value. Values with a lower hit count are not displayed.
		/// </summary>
		public int FilterMinHitCount { get; set; }

		/// <summary>
		/// Gets or sets the maximum number of filter values to be displayed.
		/// </summary>
		public int FilterMaxChoicesCount { get; set; }

        /// <summary>
        /// Gets or sets the default sort order in search results
        /// </summary>
        public ProductSortingEnum DefaultSortOrder { get; set; }

		// TBD: what about area specific searchin setting (product, blog, etc.)

		#region Common facet settings

		/// <summary>
		/// Gets or sets the a value indicating whether to include or exclude not available products by default.
		/// </summary>
		public bool IncludeNotAvailable { get; set; }

		public bool BrandDisabled { get; set; }
		public bool PriceDisabled { get; set; }
		public bool RatingDisabled { get; set; }
		public bool DeliveryTimeDisabled { get; set; }
		public bool AvailabilityDisabled { get; set; }
		public bool NewArrivalsDisabled { get; set; }

		public int BrandDisplayOrder { get; set; }
		public int PriceDisplayOrder { get; set; }
		public int RatingDisplayOrder { get; set; }
		public int DeliveryTimeDisplayOrder { get; set; }
		public int AvailabilityDisplayOrder { get; set; }
		public int NewArrivalsDisplayOrder { get; set; }

		#endregion
	}
}
