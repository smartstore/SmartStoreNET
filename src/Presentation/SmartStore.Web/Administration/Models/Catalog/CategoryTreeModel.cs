using System.ComponentModel.DataAnnotations;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Catalog
{
    public class CategoryTreeModel : ModelBase
    {
        [UIHint("Stores")]
        [SmartResourceDisplayName("Admin.Common.Store.SearchFor")]
        public int SearchStoreId { get; set; }

        public bool IsSingleStoreMode { get; set; }
        public bool CanEdit { get; set; }
    }
}