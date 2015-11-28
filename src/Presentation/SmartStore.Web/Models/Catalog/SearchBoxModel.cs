using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Web.Models.Catalog
{
    public partial class SearchBoxModel : ModelBase
    {
        public bool AutoCompleteEnabled { get; set; }
        public bool ShowProductImagesInSearchAutoComplete { get; set; }
        public int SearchTermMinimumLength { get; set; }
    }
}