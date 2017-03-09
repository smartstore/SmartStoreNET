using SmartStore.Web.Framework.Modelling;
using System.Collections.Generic;

namespace SmartStore.Web.Models.Catalog
{
    public partial class AjaxMenuItemModel : EntityModelBase
    {
        public AjaxMenuItemModel()
        {
            SubCategories = new List<AjaxMenuItemModel>();
            ParentCategory = new AjaxParentCategoryModel();
        }

        public string Name { get; set; }

        public string SortDescription { get; set; }

        public string SeName { get; set; }

        public bool HasChildren { get; set; }
        
        public IList<AjaxMenuItemModel> SubCategories { get; set; }

        public AjaxParentCategoryModel ParentCategory { get; set; }
    }

    public partial class AjaxParentCategoryModel : EntityModelBase
    {
        public string Name { get; set; }

        public string SeName { get; set; }
    }
}