using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Web.Models.Catalog
{
    public partial class ListOptionItem : ModelBase
    {
        public string Text { get; set; }
        public string Url { get; set; }
        public bool Selected { get; set; }
        public object ExtraData { get; set; }

        public SelectListItem ToSelectListItem()
        {
            return new SelectListItem
            {
                Text = this.Text,
                Value = this.Url,
                Selected = this.Selected
            };
        }
    }

    public partial class PagingFilteringModel : PagedListBase
    {
        
        public PagingFilteringModel()
        {
            this.AvailableSortOptions = new List<ListOptionItem>();
            this.AvailableViewModes = new List<ListOptionItem>();
            this.PageSizeOptions = new List<ListOptionItem>();
        }

        public bool AllowProductSorting { get; set; }
        public IList<ListOptionItem> AvailableSortOptions { get; set; }

        public bool AllowProductViewModeChanging { get; set; }
        public IList<ListOptionItem> AvailableViewModes { get; set; }

        public bool AllowCustomersToSelectPageSize { get; set; }
        public IList<ListOptionItem> PageSizeOptions { get; set; }

        /// <summary>
        /// Order by
        /// </summary>
        [SmartResourceDisplayName("Categories.OrderBy")]
        public int OrderBy { get; set; }

        /// <summary>
        /// Product sorting
        /// </summary>
        [SmartResourceDisplayName("Categories.ViewMode")]
        public string ViewMode { get; set; }

    }

}