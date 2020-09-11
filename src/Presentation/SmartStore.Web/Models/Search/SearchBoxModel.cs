using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Search
{
    public partial class SearchBoxModel : ModelBase
    {
        public string Origin { get; set; }
        public string SearchUrl { get; set; }
        public string InputPlaceholder { get; set; }
        public int SearchTermMinimumLength { get; set; }

        public string InstantSearchUrl { get; set; }
        public bool InstantSearchEnabled { get; set; }
        public bool ShowThumbsInInstantSearch { get; set; }

        public string CurrentQuery { get; set; }
    }
}