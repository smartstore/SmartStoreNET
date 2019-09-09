using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Catalog
{
	public class ContentSliderListModel : ModelBase
    {
        [SmartResourceDisplayName("Admin.CMS.ContentSlider.List.SearchContentSliderName")]
        [AllowHtml]
        public string SearchContentSliderName { get; set; }

		[SmartResourceDisplayName("Admin.Common.Store.SearchFor")]
		public int SearchStoreId { get; set; }
		public IList<SelectListItem> AvailableStores { get; set; }

		public int GridPageSize { get; set; }
    }
}