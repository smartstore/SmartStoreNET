using System;
using SmartStore.Core.Configuration;

namespace SmartStore.Core.Search
{
	public class SearchSettings : ISettings
	{
		public SearchSettings()
		{
			InstantSearchEnabled = true;
			ShowProductImagesInInstantSearch = true;
			InstantSearchNumberOfProducts = 10;
			InstantSearchTermMinLength = 2;
		}

		/// <summary>
		/// Gets or sets a value indicating whether instant-search is enabled
		/// </summary>
		public bool InstantSearchEnabled { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to show product images in instant-search rwesult
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

		// TBD: what about area specific searchin setting (product, blog, etc.)
	}
}
