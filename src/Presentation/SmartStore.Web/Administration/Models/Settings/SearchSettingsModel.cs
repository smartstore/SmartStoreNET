using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Core.Search;
using SmartStore.Core.Search.Filter;
using SmartStore.Web.Framework;

namespace SmartStore.Admin.Models.Settings
{
	public partial class SearchSettingsModel
	{
		[SmartResourceDisplayName("Admin.Configuration.Settings.Search.SearchMode")]
		public SearchMode SearchMode { get; set; }
		public List<SelectListItem> AvailableSearchModes { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Settings.Search.SearchFields")]
		public List<string> SearchFields { get; set; }
		public List<SelectListItem> AvailableSearchFields { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Settings.Search.InstantSearchEnabled")]
		public bool InstantSearchEnabled { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Settings.Search.ShowProductImagesInInstantSearch")]
		public bool ShowProductImagesInInstantSearch { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Settings.Search.InstantSearchNumberOfProducts")]
		public int InstantSearchNumberOfProducts { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Settings.Search.InstantSearchTermMinLength")]
		public int InstantSearchTermMinLength { get; set; }

		public List<SearchFilterDescriptor> GlobalFilters { get; set; }
	}
}