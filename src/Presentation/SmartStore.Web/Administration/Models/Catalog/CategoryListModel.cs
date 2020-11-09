using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Catalog
{
    public class CategoryListModel : ModelBase
    {
        [SmartResourceDisplayName("Admin.Catalog.Categories.List.SearchCategoryName")]
        [AllowHtml]
        public string SearchCategoryName { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Categories.List.SearchAlias")]
        public string SearchAlias { get; set; }

        [UIHint("Stores")]
        [SmartResourceDisplayName("Admin.Common.Store.SearchFor")]
        public int SearchStoreId { get; set; }

        public bool IsSingleStoreMode { get; set; }
        public int GridPageSize { get; set; }
    }
}