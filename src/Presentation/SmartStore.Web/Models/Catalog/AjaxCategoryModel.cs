using SmartStore.Web.Framework.Modelling;
using System.Collections.Generic;

namespace SmartStore.Web.Models.Catalog
{
    public partial class AjaxCategoryModel : EntityModelBase
    {
        public AjaxCategoryModel()
        {
            SubCategories = new List<AjaxCategoryModel>();
            ParentCategory = new AjaxParentCategoryModel();
        }

        public string Name { get; set; }

        public string SortDescription { get; set; }

        public string SeName { get; set; }

        public bool HasChildren { get; set; }

        public IList<AjaxCategoryModel> SubCategories { get; set; }

        public AjaxParentCategoryModel ParentCategory { get; set; }
    }

    public partial class AjaxParentCategoryModel : EntityModelBase
    {
        public string Name { get; set; }

        public string SeName { get; set; }
    }
}