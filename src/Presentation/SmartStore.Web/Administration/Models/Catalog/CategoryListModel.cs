using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Catalog
{
    public class CategoryListModel : ModelBase
    {
		public CategoryListModel()
		{
			AvailableStores = new List<SelectListItem>();
		}

        [SmartResourceDisplayName("Admin.Catalog.Categories.List.SearchCategoryName")]
        [AllowHtml]
        public string SearchCategoryName { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Categories.List.SearchAlias")]
		public string SearchAlias { get; set; }

		[SmartResourceDisplayName("Admin.Common.Store.SearchFor")]
		public int SearchStoreId { get; set; }
		public IList<SelectListItem> AvailableStores { get; set; }

		public int GridPageSize { get; set; }
    }
}