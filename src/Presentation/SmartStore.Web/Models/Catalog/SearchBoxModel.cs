using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Catalog
{
    public partial class SearchBoxModel : ModelBase
    {
        public bool InstantSearchEnabled { get; set; }
        public bool ShowProductImagesInInstantSearch { get; set; }
        public int SearchTermMinimumLength { get; set; }

		public string CurrentQuery { get; set; }
    }
}