using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Models.Catalog
{
    public class CategoryListModel : ModelBase
    {
        [SmartResourceDisplayName("Admin.Catalog.Categories.List.SearchCategoryName")]
        [AllowHtml]
        public string SearchCategoryName { get; set; }

		/// <remarks>codehint: sm-add</remarks>
		[SmartResourceDisplayName("Admin.Catalog.Categories.List.SearchAlias")]
		public string SearchAlias { get; set; }

        public GridModel<CategoryModel> Categories { get; set; }
    }
}