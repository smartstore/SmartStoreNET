using System.Collections.Generic;
using SmartStore.Core.Configuration;

namespace SmartStore.Core.Search
{
	public class SearchSettings : ISettings
	{
		public SearchSettings()
		{
			SearchMode = SearchMode.Contains;
			SearchFields = new List<string> { "shortdescription", "tagname", "manufacturer", "category" };
			InstantSearchEnabled = true;
			ShowProductImagesInInstantSearch = true;
			InstantSearchNumberOfProducts = 10;
			InstantSearchTermMinLength = 2;
			FilterMinHitCount = 1;
			FilterMaxChoicesCount = 20;
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
		/// Json serialized information about global search filters
		/// </summary>
		public string GlobalFilters { get; set; }

		/// <summary>
		/// Gets or sets the minimum hit count for a filter value. Values with a lower hit count are not displayed.
		/// </summary>
		public int FilterMinHitCount { get; set; }

		/// <summary>
		/// Gets or sets the maximum number of filter values to be displayed.
		/// </summary>
		public int FilterMaxChoicesCount { get; set; }

		// TBD: what about area specific searchin setting (product, blog, etc.)
	}
}
